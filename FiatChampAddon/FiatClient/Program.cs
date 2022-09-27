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
        //todo: das läuft nu auch bei refresh loop auslagern in eigene loop
        if (appConfig.AutoRefreshBattery)
        {
          await TrySendCommand(fiatClient, FiatCommands.DEEPREFRESH, vehicle.Vin);
        }
        
        if (appConfig.AutoRefreshLocation)
        {
          await TrySendCommand(fiatClient, FiatCommands.DEEPREFRESH, vehicle.Vin);
        }
        
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

        if (!persistentHaEntities.ContainsKey(vehicle.Vin))
        {
          var entities = CreateEntities(fiatClient, mqttClient, vehicle, haDevice);

          persistentHaEntities.TryAdd(vehicle.Vin, entities);
        }

        foreach (var haEntity in persistentHaEntities.Values.SelectMany(entities => entities))
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

    if (WaitHandle.WaitTimeout !=
        WaitHandle.WaitAny(new[]
          {
            ctx.CancellationToken.WaitHandle,
            forceLoopResetEvent
          },
          TimeSpan.FromMinutes(appConfig.RefreshInterval)))
    {
      await Task.Delay(TimeSpan.FromSeconds(10), ctx.CancellationToken);
    }
  }
});

async Task<bool> TrySendCommand(FiatClient fiatClient, FiatCommands command, string vin)
{
  Console.WriteLine($"SEND COMMAND: {command}");
  
  var pin = appConfig.FiatPin ?? throw new Exception("PIN NOT SET");
  
  try
  {
    await fiatClient.SendCommand(vin, command.ToString(), pin, "ev");
    await Task.Delay(TimeSpan.FromSeconds(5));
    Console.WriteLine($"COMMAND: {command} SUCCESSFUL");
  }
  catch (Exception e)
  {
    Console.WriteLine($"Error sending command: {command}. Maybe wrong pin?");
    e.Message.Dump();
    return false;
  }

  return true;
}

IEnumerable<HaEntity> CreateEntities(FiatClient fiatClient, SimpleMqttClient mqttClient, Vehicle vehicle, HaDevice haDevice)
{
  var updateLocationButton = new HaButton(mqttClient, "UpdateLocation", haDevice, async button =>
  {
    if (appConfig.AutoRefreshLocation ||
        await TrySendCommand(fiatClient, FiatCommands.VF, vehicle.Vin))
      forceLoopResetEvent.Set();
  });

  var deepRefreshButton = new HaButton(mqttClient, "DeepRefresh", haDevice, async button =>
  {
    if (appConfig.AutoRefreshBattery ||
        await TrySendCommand(fiatClient, FiatCommands.DEEPREFRESH, vehicle.Vin))
      forceLoopResetEvent.Set();
  });

  var locateLightsButton = new HaButton(mqttClient, "Blink", haDevice, async button =>
  {
    Console.WriteLine("HBLF");
  });

  var chargeNowButton = new HaButton(mqttClient, "ChargeNOW", haDevice, async button =>
  {
    Console.WriteLine("CNOW");
  });

  var trunkSwitch = new HaSwitch(mqttClient, "Trunk", haDevice, async sw =>
  {
    Console.WriteLine(sw.IsOn ? "ROTRUNKUNLOCK" : "ROTRUNKLOCK");
  });

  var hvacSwitch = new HaSwitch(mqttClient, "HVAC", haDevice, async sw =>
  {
    if (sw.IsOn)
    {
      Console.WriteLine("ROPRECOND");
    }
    else if (appConfig.EnableDangerousCommands && !sw.IsOn)
    {
      Console.WriteLine("ROPRECOND_OFF");
    }
  });

  return new HaEntity[]
    { hvacSwitch, trunkSwitch, chargeNowButton, deepRefreshButton, locateLightsButton, updateLocationButton };
}

public enum FiatCommands
{
  DEEPREFRESH,
  VF,
  HBLF,
  CNOW,
  ROPRECOND,
  ROPRECOND_OFF,
  ROTRUNKUNLOCK,
  ROTRUNKLOCK
} 