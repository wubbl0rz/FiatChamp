using System.Collections.Concurrent;
using System.Globalization;
using Cocona;
using CoordinateSharp;
using FiatUconnect;
using FiatUconnect.HA;
using Flurl.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Events;

var builder = CoconaApp.CreateBuilder();

builder.Configuration.AddEnvironmentVariables("FiatUconnect_");

builder.Services.AddOptions<AppConfig>()
  .Bind(builder.Configuration)
  .ValidateDataAnnotations()
  .ValidateOnStart();

var app = builder.Build();

var persistentHaEntities = new ConcurrentDictionary<string, DateTime>();

var appConfig = builder.Configuration.Get<AppConfig>();
var forceLoopResetEvent = new AutoResetEvent(false);
var haClient = new HaRestApi(appConfig.HomeAssistantUrl, appConfig.SupervisorToken);

Log.Logger = new LoggerConfiguration()
  .MinimumLevel.Is(appConfig.Debug ? LogEventLevel.Debug : LogEventLevel.Information)
  .WriteTo.Console()
  .CreateLogger();

Log.Information("Delay start for seconds: {0}", appConfig.StartDelaySeconds);
await Task.Delay(TimeSpan.FromSeconds(appConfig.StartDelaySeconds));


await app.RunAsync(async (CoconaAppContext ctx) =>
{
    Log.Information("{0}", appConfig.ToStringWithoutSecrets());
    Log.Debug("{0}", appConfig.Dump());

    FiatClient fiatClient = new FiatClient(appConfig.FiatUser, appConfig.FiatPw);

    var mqttClient = new SimpleMqttClient(appConfig.MqttServer, appConfig.MqttPort, appConfig.MqttUser, appConfig.MqttPw, "FiatUconnect");

    await mqttClient.Connect();

    while (!ctx.CancellationToken.IsCancellationRequested)
    {
        Log.Information("Now fetching new data...");

        GC.Collect();

        try
        {
            await fiatClient.LoginAndKeepSessionAlive();

            foreach (var vehicle in await fiatClient.Fetch())
            {
                Log.Information($"Found : {vehicle.Nickname} {vehicle.Vin}");

                //await Task.Delay(TimeSpan.FromSeconds(5), ctx.CancellationToken);

                var haDevice = new HaDevice()
                {
                    Name = string.IsNullOrEmpty(vehicle.Nickname) ? "Fiat" : vehicle.Nickname,
                    Identifier = vehicle.Vin,
                    Manufacturer = vehicle.Make,
                    Model = vehicle.ModelDescription,
                    Version = "1.0"
                };

                IEnumerable<HaEntity> haEntities = await GetHaEntities(haClient, mqttClient, vehicle, haDevice);

                if (persistentHaEntities.TryAdd(vehicle.Vin,DateTime.Now))
                {
                    Log.Information("Pushing new sensors to Home Assistant");
                    await Parallel.ForEachAsync(haEntities, async (sensor, token) => { await sensor.Announce(); });

                    Log.Information("Pushing new buttons to Home Assistant");
                    var haInteractiveEntities = CreateInteractiveEntities(ctx, fiatClient, mqttClient, vehicle, haDevice);
                    await Parallel.ForEachAsync(haInteractiveEntities, async (button, token) => { await button.Announce(); });
                }

                Log.Information("Pushing sensors values to Home Assistant");
                await Parallel.ForEachAsync(haEntities, async (sensor, token) => { await sensor.PublishState(); });

                var lastUpdate = new HaSensor(mqttClient, "500e_LastUpdate", haDevice, false) { Value = DateTime.Now.ToString("dd/MM HH:mm:ss"), DeviceClass = "duration" };
                await lastUpdate.Announce();
                await lastUpdate.PublishState();
            }
        }
        catch (FlurlHttpException httpException)
        {
            Log.Warning($"Error connecting to the FIAT API. \n" +
                        $"This can happen from time to time. Retrying in {appConfig.RefreshInterval} minutes.");

            Log.Debug("ERROR: {0}", httpException.Message);
            Log.Debug("STATUS: {0}", httpException.StatusCode);

            var task = httpException.Call?.Response?.GetStringAsync();

            if (task != null) { Log.Debug("RESPONSE: {0}", await task); }
        }
        catch (Exception e)
        {
            Log.Error("{0}", e);
        }

        Log.Information("Fetching COMPLETED. Next update in {0} minutes.", appConfig.RefreshInterval);

        WaitHandle.WaitAny(new[] { ctx.CancellationToken.WaitHandle, forceLoopResetEvent }, TimeSpan.FromMinutes(appConfig.RefreshInterval));
    }
});

async Task<bool> TrySendCommand(FiatClient fiatClient, FiatCommand command, string vin)
{
    Log.Information("SEND COMMAND {0}: ", command.Message);

    if (string.IsNullOrWhiteSpace(appConfig.FiatPin))
    {
        throw new Exception("PIN NOT SET");
    }

    var pin = appConfig.FiatPin;

    try
    {
        await fiatClient.SendCommand(vin, command.Message, pin, command.Action);
        await Task.Delay(TimeSpan.FromSeconds(5));
        Log.Information("Command: {0} SUCCESSFUL", command.Message);
    }
    catch (Exception e)
    {
        Log.Error("Command: {0} ERROR. Maybe wrong pin?", command.Message);
        Log.Debug("{0}", e);
        return false;
    }

    return true;
}



IEnumerable<HaEntity> CreateInteractiveEntities(CoconaAppContext ctx, FiatClient fiatClient, SimpleMqttClient mqttClient, Vehicle vehicle,
  HaDevice haDevice)
{
    var updateLocationButton = new HaButton(mqttClient, "500e_UpdateLocation", haDevice, async button =>
    {
        if (await TrySendCommand(fiatClient, FiatCommand.VF, vehicle.Vin))
        {
            await Task.Delay(TimeSpan.FromSeconds(8), ctx.CancellationToken);
            forceLoopResetEvent.Set();
        }
    });

    var deepRefreshButton = new HaButton(mqttClient, "500e_DeepRefresh", haDevice, async button =>
    {
        if (await TrySendCommand(fiatClient, FiatCommand.DEEPREFRESH, vehicle.Vin))
        {
            await Task.Delay(TimeSpan.FromSeconds(8), ctx.CancellationToken);
            forceLoopResetEvent.Set();
        }
    });

    var lightsButton = new HaButton(mqttClient, "500e_Light", haDevice, async button =>
    {
        if (await TrySendCommand(fiatClient, FiatCommand.ROLIGHTS, vehicle.Vin))
        {
            forceLoopResetEvent.Set();
        }
    });

    var chargeNowButton = new HaButton(mqttClient, "500e_ChargeNOW", haDevice, async button =>
    {
        if (await TrySendCommand(fiatClient, FiatCommand.CNOW, vehicle.Vin))
        {
            forceLoopResetEvent.Set();
        }
    });



    var hvacButton = new HaButton(mqttClient, "500e_HVAC", haDevice, async button =>
    {
        if (await TrySendCommand(fiatClient, FiatCommand.ROPRECOND, vehicle.Vin))
        {
            forceLoopResetEvent.Set();
        }
    });


    var lockButton = new HaButton(mqttClient, "500e_DoorLock", haDevice, async button =>
    {
        if (await TrySendCommand(fiatClient, FiatCommand.RDL, vehicle.Vin))
        {
            forceLoopResetEvent.Set();
        }
    });

    var unLockButton = new HaButton(mqttClient, "500e_DoorUnLock", haDevice, async button =>
    {
        if (await TrySendCommand(fiatClient, FiatCommand.RDU, vehicle.Vin))
        {
            forceLoopResetEvent.Set();
        }
    });

    var fetchNowButton = new HaButton(mqttClient, "500e_FetchNow", haDevice, async button =>
    {
        await Task.Run(() => forceLoopResetEvent.Set());
    });

    var haEntities = new HaEntity[] { hvacButton, chargeNowButton, deepRefreshButton, lightsButton, updateLocationButton, lockButton, unLockButton, fetchNowButton };

    Log.Debug("Announce haEntities : {0}", haEntities.Dump());

    return haEntities;
}

async Task<IEnumerable<HaEntity>> GetHaEntities(HaRestApi haClient, SimpleMqttClient mqttClient, Vehicle vehicle, HaDevice haDevice)
{
    var compactDetails = vehicle.Details.Compact("500e");

    bool charging = false;
    string charginglevel = "battery_timetofullychargel2";
    DateTime refChargeEndTime = DateTime.Now;

    List<HaEntity> haEntities = compactDetails.Select(detail =>
       {

           bool binary = false;
           string deviceClass = "";
           string unit = "";
           string value = detail.Value;

           if (detail.Key.Contains("scheduleddays", StringComparison.InvariantCultureIgnoreCase)
             || detail.Key.Contains("pluginstatus", StringComparison.InvariantCultureIgnoreCase)
             || detail.Key.Contains("cabinpriority", StringComparison.InvariantCultureIgnoreCase)
             || detail.Key.Contains("chargetofull", StringComparison.InvariantCultureIgnoreCase)
             || detail.Key.Contains("enablescheduletype", StringComparison.InvariantCultureIgnoreCase)
             || detail.Key.Contains("repeatschedule", StringComparison.InvariantCultureIgnoreCase)
             )
           {
               binary = true;
           }

           if (detail.Key.Contains("battery_timetofullycharge", StringComparison.InvariantCultureIgnoreCase))
           {
               deviceClass = "duration";
               unit = "min";
           }

           if (detail.Key.EndsWith("chargingstatus", StringComparison.InvariantCultureIgnoreCase))
           {
               binary = true;
               deviceClass = "battery_charging";
               if (detail.Value == "CHARGING")
               {
                   value = "True";
                   charging = true;
               }
               else
               {
                   value = "False";
                   charging = false;
               }

           }

           if (detail.Key.EndsWith("evinfo_battery_charginglevel", StringComparison.InvariantCultureIgnoreCase))
           {
               charginglevel = $"battery_timetofullychargel{detail.Value.Last()}";
           }

           if (detail.Key.EndsWith("battery_stateofcharge", StringComparison.InvariantCultureIgnoreCase))
           {
               deviceClass = "battery";
               unit = "%";
           }

           if (detail.Key.EndsWith("evinfo_timestamp", StringComparison.InvariantCultureIgnoreCase))
           {
               refChargeEndTime = GetLocalTime(Convert.ToInt64(detail.Value));
           }

           if (detail.Key.EndsWith("_timestamp", StringComparison.InvariantCultureIgnoreCase))
           {
               value = GetLocalTime(Convert.ToInt64(detail.Value)).ToString("dd/MM HH:mm:ss");
               deviceClass = "duration";
           }

           if (detail.Key.EndsWith("_value", StringComparison.InvariantCultureIgnoreCase))
           {
               var unitKey = detail.Key.Replace("_value", "_unit", StringComparison.InvariantCultureIgnoreCase);

               compactDetails.TryGetValue(unitKey, out var tmpUnit);

               switch (tmpUnit)
               {
                   case "km":
                       deviceClass = "distance";
                       unit = "km";
                       break;
                   case "volts":
                       deviceClass = "voltage";
                       unit = "V";
                       break;
                   case null or "null":
                       unit = "";
                       break;
                   default:
                       unit = tmpUnit;
                       break;
               }
           }

           var sensor = new HaSensor(mqttClient, detail.Key, haDevice, binary)
           {
               DeviceClass = deviceClass,
               Unit = unit,
               Value = value,
           };

           return sensor as HaEntity;
       }).ToList();

    var textChargeDuration = "";
    var textChargeEndTime = "";
    if (charging)
    {
        var chargeDuration = Convert.ToInt32(haEntities.OfType<HaSensor>().Single(s => s.Name.EndsWith(charginglevel, StringComparison.InvariantCultureIgnoreCase)).Value);
        textChargeDuration = $"{chargeDuration / 60}:{$"{chargeDuration % 60}".PadLeft(2, '0')}";
        textChargeEndTime = refChargeEndTime.AddMinutes(chargeDuration).ToString("H:mm");
    }

    haEntities.Add(new HaSensor(mqttClient, "500e_Charge_Duration", haDevice, false)
    {
        DeviceClass = "duration",
        Value = textChargeDuration,
    });

    haEntities.Add(new HaSensor(mqttClient, "500e_Charge_Endtime", haDevice, false)
    {
        DeviceClass = "duration",
        Value = textChargeEndTime,
    });

    var currentCarLocation = new Coordinate(vehicle.Location.Latitude, vehicle.Location.Longitude);

    var zones = await haClient.GetZonesAscending(currentCarLocation);

    Log.Debug("Zones: {0}", zones.Dump());

    var tracker = new HaDeviceTracker(mqttClient, "500e_Location", haDevice)
    {
        Lat = currentCarLocation.Latitude.ToDouble(),
        Lon = currentCarLocation.Longitude.ToDouble(),
        StateValue = zones.FirstOrDefault()?.FriendlyName ?? "Away"
    };

    haEntities.Add(tracker);

    var trackerTimeStamp = new HaSensor(mqttClient, "500e_Location_TimeStamp", haDevice, false)
    {
        Value = GetLocalTime(vehicle.Location.TimeStamp).ToString("dd/MM HH:mm:ss"),
        DeviceClass = "duration"
    };

    haEntities.Add(trackerTimeStamp);

    Log.Debug("Announce haEntities : {0}", haEntities.Dump());

    return haEntities;
}

DateTime GetLocalTime(long timeStamp)
{
    return DateTimeOffset.FromUnixTimeMilliseconds(timeStamp).UtcDateTime.ToLocalTime();
}

