using System.Collections.Concurrent;
using Cocona;
using FiatChamp;
using FiatChamp.HA;
using Flurl.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = CoconaApp.CreateBuilder();

builder.Configuration.AddEnvironmentVariables("FiatChamp_");
builder.Services.AddOptions<AppConfig>()
  .Bind(builder.Configuration)
  .ValidateDataAnnotations()
  .ValidateOnStart();

var app = builder.Build();

var persistentHaEntities = new ConcurrentDictionary<string, IEnumerable<HaEntity>>();
var appConfig = builder.Configuration.Get<AppConfig>();
var forceLoopResetEvent = new AutoResetEvent(false);

await app.RunAsync(async (CoconaAppContext ctx) =>
{
  var fiatClient = new FiatClient(appConfig.FiatUser, appConfig.FiatPw);

  var mqttClient = new SimpleMqttClient(appConfig.MqttServer,
    appConfig.MqttPort,
    appConfig.MqttUser,
    appConfig.MqttPw,
    appConfig.DevMode ? "FiatChampDEV" : "FiatChamp");

  await mqttClient.Connect();

  while (!ctx.CancellationToken.IsCancellationRequested)
  {
    Console.WriteLine($"FETCH DATA... {DateTime.Now}");
    
    try
    {
      await fiatClient.LoginAndKeepSessionAlive();

      foreach (var vehicle in await fiatClient.Fetch())
      {
        if (appConfig.AutoRefreshBattery)
        {
          await TrySendCommand(fiatClient, FiatCommand.DEEPREFRESH, vehicle.Vin);
        }
        
        if (appConfig.AutoRefreshLocation)
        {
          await TrySendCommand(fiatClient, FiatCommand.VF, vehicle.Vin);
        }
        
        await Task.Delay(TimeSpan.FromSeconds(10), ctx.CancellationToken);
        
        var vehicleName = string.IsNullOrEmpty(vehicle.Nickname) ? "Car" : vehicle.Nickname;
        var suffix = appConfig.DevMode ? "DEV" : "";

        var haDevice = new HaDevice()
        {
          Name = vehicleName + suffix,
          Identifier = vehicle.Vin + suffix,
          Manufacturer = vehicle.Make,
          Model = vehicle.ModelDescription,
          Version = "1.0"
        };

        var tracker = new HaDeviceTracker(mqttClient, "CAR_LOCATION", haDevice)
        {
          Lat = vehicle.Location.Latitude,
          Lon = vehicle.Location.Longitude
        };

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

        var haEntities = persistentHaEntities.GetOrAdd(vehicle.Vin, s => 
          CreateEntities(fiatClient, mqttClient, vehicle, haDevice));

        foreach (var haEntity in haEntities)
        {
          await haEntity.Announce();
        }
      }
    }
    catch (FlurlHttpException httpException)
    {
      Console.WriteLine($"Error connecting to the FIAT API. \n" +
                        $"This can happen from time to time. Retrying in {appConfig.RefreshInterval} minutes.");

      httpException.Message.Dump();
    }
    catch (Exception e)
    {
      Console.WriteLine(e.Message);
    }

    WaitHandle.WaitAny(new[]
    {
      ctx.CancellationToken.WaitHandle,
      forceLoopResetEvent
    }, TimeSpan.FromMinutes(appConfig.RefreshInterval));
  }
});

async Task<bool> TrySendCommand(FiatClient fiatClient, FiatCommand command, string vin)
{
  Console.WriteLine($"SEND COMMAND: {command.Message}");

  var pin = appConfig.FiatPin ?? throw new Exception("PIN NOT SET");

  if (command.IsDangerous && !appConfig.EnableDangerousCommands)
  {
    Console.WriteLine($"{command.Message} not sent. " +
                      "Set \"EnableDangerousCommands\" option if you want to use it.");
    return false;
  }

  try
  {
    await fiatClient.SendCommand(vin, command.Message, pin, command.Action);
    await Task.Delay(TimeSpan.FromSeconds(5));
    Console.WriteLine($"COMMAND: {command.Message} SUCCESSFUL");
  }
  catch (Exception e)
  {
    Console.WriteLine($"Error sending command: {command.Message}. Maybe wrong pin?");
    e.Message.Dump();
    return false;
  }

  return true;
}

IEnumerable<HaEntity> CreateEntities(FiatClient fiatClient, SimpleMqttClient mqttClient, Vehicle vehicle, HaDevice haDevice)
{
  var updateLocationButton = new HaButton(mqttClient, "UpdateLocation", haDevice, async button =>
  {
    if (await TrySendCommand(fiatClient, FiatCommand.VF, vehicle.Vin))
      forceLoopResetEvent.Set();
  });

  var deepRefreshButton = new HaButton(mqttClient, "DeepRefresh", haDevice, async button =>
  {
    if (await TrySendCommand(fiatClient, FiatCommand.DEEPREFRESH, vehicle.Vin))
      forceLoopResetEvent.Set();
  });

  var locateLightsButton = new HaButton(mqttClient, "Blink", haDevice, async button =>
  {
    if (await TrySendCommand(fiatClient, FiatCommand.HBLF, vehicle.Vin))
      forceLoopResetEvent.Set();
  });

  var chargeNowButton = new HaButton(mqttClient, "ChargeNOW", haDevice, async button =>
  {
    if (await TrySendCommand(fiatClient, FiatCommand.CNOW, vehicle.Vin))
      forceLoopResetEvent.Set();
  });

  var trunkSwitch = new HaSwitch(mqttClient, "Trunk", haDevice, async sw =>
  {
    if (await TrySendCommand(fiatClient, sw.IsOn ? FiatCommand.ROTRUNKUNLOCK : FiatCommand.ROTRUNKLOCK, vehicle.Vin))
      forceLoopResetEvent.Set();
  });

  var hvacSwitch = new HaSwitch(mqttClient, "HVAC", haDevice, async sw =>
  {
    if (await TrySendCommand(fiatClient, sw.IsOn ? FiatCommand.ROPRECOND : FiatCommand.ROPRECOND_OFF, vehicle.Vin))
      forceLoopResetEvent.Set();
  });

  return new HaEntity[]
    { hvacSwitch, trunkSwitch, chargeNowButton, deepRefreshButton, locateLightsButton, updateLocationButton };
}