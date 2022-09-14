using System.Collections.Concurrent;
using Cocona;
using FiatChamp;
using FiatChamp.HA;
using Microsoft.Extensions.Configuration;

// TODO: session handling / timeout and location refresh
// TODO: ha addon. debug mode. debug print. experimental flag. update interval. pin on button press ?
// TODO: mqtt tls or not ?

var builder = CoconaApp.CreateBuilder();
builder.Configuration.AddEnvironmentVariables("FiatChamp_");

var app = builder.Build();

await app.RunAsync(async (IConfiguration config, CoconaAppContext ctx) =>
{
  var envOptions = config.Get<EnvOptions>();

  if (string.IsNullOrWhiteSpace(envOptions.FiatUser))
  {
    throw new Exception("FiatUser NOT FOUND");
  }
  
  if (string.IsNullOrWhiteSpace(envOptions.FiatPw))
  {
    throw new Exception("FiatPw NOT FOUND");
  }
  
  if (string.IsNullOrWhiteSpace(envOptions.MqttServer) || 
      string.IsNullOrWhiteSpace(envOptions.MqttUser) || 
      string.IsNullOrWhiteSpace(envOptions.MqttPw) || 
      envOptions.MqttPort is null or 0)
  {
    throw new Exception("Mqtt settings not found.");
  }
  
  var mqttClient = new SimpleMqttClient(envOptions.MqttServer, 
    envOptions.MqttPort, 
    envOptions.MqttUser, 
    envOptions.MqttPw, 
    "FiatChamp");
  await mqttClient.Connect();
  
  var haEntities = new ConcurrentDictionary<string, HaEntity[]>();

  while (!ctx.CancellationToken.IsCancellationRequested)
  {
    Console.WriteLine($"FETCH DATA... {DateTime.Now}");
    
    try
    {
      var fiatClient = new FiatClient(envOptions.FiatUser,envOptions.FiatPw);
      await fiatClient.Login();
      
      var vehicles = await fiatClient.Fetch();
  
      foreach (var vehicle in vehicles)
      {
        var vehicleName = string.IsNullOrEmpty(vehicle.Nickname) ? "Car" : vehicle.Nickname;
  
        var haDevice = new HaDevice()
        {
          Name = vehicleName,
          Identifier = vehicle.Vin,
          Manufacturer = vehicle.Make,
          Model = vehicle.ModelDescription,
          Version = "1.0"
        };
  
        var tracker = new HaDeviceTracker(mqttClient, "CAR_LOCATION", haDevice);
  
        tracker.Lat = vehicle.Location.Latitude;
        tracker.Lon = vehicle.Location.Longitude;
  
        await tracker.Announce();
        await tracker.PublishState();
  
        var compactDetails = vehicle.Details.Compact("car");
        
        await Parallel.ForEachAsync(compactDetails, async (sensorData, token) =>
        {
          var sensor = new HaSensor(mqttClient, sensorData.Key, haDevice)
          {
            Value = sensorData.Value
          };
          
          await sensor.Announce();
          await sensor.PublishState();
        });
        
        TrySetupCommandsForVehicle(vehicle, haDevice);
        
        foreach (var haEntity in haEntities.Values.SelectMany(a => a))
        {
          await haEntity.Announce();
        }
      }
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
    }
    
    await Task.Delay(TimeSpan.FromMinutes(15), ctx.CancellationToken);
  }
  
  bool TrySetupCommandsForVehicle(Vehicle vehicle, HaDevice haDevice)
  {
    if (haEntities.ContainsKey(vehicle.Vin))
      return false;
    
    var updateLocationButton = new 
      HaButton(mqttClient, "UpdateLocation", haDevice, async button => { Console.WriteLine("VF"); });
  
    var deepRefreshButton = new
      HaButton(mqttClient, "DeepRefresh", haDevice, async button => { Console.WriteLine("DEEPREFRESH"); });
    
    var locateLightsButton = new 
      HaButton(mqttClient, "Blink", haDevice, async button => { Console.WriteLine("HBLF"); });
  
    var chargeNowButton = new
      HaButton(mqttClient, "ChargeNOW", haDevice, async button => { Console.WriteLine("CNOW"); });
    
    var trunkSwitch = new HaSwitch(mqttClient, "Trunk", haDevice, async sw =>
    {
      if (sw.IsOn)
      {
        Console.WriteLine("ROTRUNKUNLOCK");
      }
      else
      {
        Console.WriteLine("ROTRUNKLOCK");
      }
    });
  
    var hvacSwitch = new HaSwitch(mqttClient, "HVAC", haDevice, async sw =>
    {
      if (sw.IsOn)
      {
        Console.WriteLine("ROPRECOND");
      }
      else
      {
        Console.WriteLine("ROPRECOND_OFF");
      }
    });
    
    haEntities.TryAdd(vehicle.Vin, new HaEntity[]
    {
      hvacSwitch,
      trunkSwitch,
      chargeNowButton,
      deepRefreshButton,
      locateLightsButton,
      updateLocationButton
    });
  
    return true;
  }
});

public class EnvOptions
{
  public string? FiatUser { get; set; }
  public string? FiatPw { get; set; }
  public string? MqttServer { get; set; }
  public int? MqttPort { get; set; }
  public string? MqttUser { get; set; }
  public string? MqttPw { get; set; }
}