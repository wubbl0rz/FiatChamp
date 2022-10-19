using CoordinateSharp;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace FiatChamp.HA;

public class HaRestApiUnitSystem
{
  public string Length { get; set; }
  public string Mass { get; set; }
  public string Pressure { get; set; }
  public string Temperature { get; set; }
  public string Volume { get; set; }
  [JsonProperty("wind_speed")] public string WindSpeed { get; set; }

  [JsonProperty("accumulated_precipitation")]
  public string AccumulatedPrecipitation { get; set; }
}

public class HaRestApiEntityState
{
  [JsonProperty("entity_id")] public string EntityId { get; set; } = null!;
  public string State { get; set; } = null!;
  public JObject Attributes { get; set; } = new ();
  
  public T AttrTo<T>()
  {
    return this.Attributes.ToObject<T>();
  }
}

public class HaRestApiZone
{
  public double Latitude { get; set; }
  public double Longitude { get; set; }
  public long Radius { get; set; }
  public bool Passive { get; set; }
  public string? Icon { get; set; }
  [JsonProperty("friendly_name")] public string FriendlyName { get; set; } = null!;
  [JsonIgnore] public Coordinate Coordinate => new Coordinate(this.Latitude, this.Longitude);
}

public class HaRestApi
{
  private readonly string _url;
  private readonly string _token;

  public HaRestApi(string url, string token)
  {
    _url = url.AppendPathSegment("api");
    _token = token;
  }

  public HaRestApi(string token)
  {
    _token = token;
  }
  
  private async Task<JObject> GetConfig()
  {
    return await _url
      .WithOAuthBearerToken(_token)
      .AppendPathSegment("config")
      .GetJsonAsync<JObject>();
  }
  
  public async Task<string> GetTimeZone()
  {
    var config = await this.GetConfig();

    return config["time_zone"].ToString();
  }
  
  public async Task<HaRestApiUnitSystem> GetUnitSystem()
  {
    var config = await this.GetConfig();

    return config["unit_system"].ToObject<HaRestApiUnitSystem>();
  }

  public async Task<IReadOnlyList<HaRestApiZone>> GetZones()
  {
    var states = await this.GetStates();
    var zones = states
      .Where(state => state.EntityId.StartsWith("zone."))
      .Select(state => state.AttrTo<HaRestApiZone>())
      .ToArray();

    return zones;
  }
  
  public async Task<IReadOnlyList<HaRestApiZone>> GetZonesAscending(Coordinate inside)
  {
    var zones = await this.GetZones();
    return zones
      .Where(zone => zone.Coordinate.Get_Distance_From_Coordinate(inside).Meters <= zone.Radius)
      .OrderBy(zone => zone.Coordinate.Get_Distance_From_Coordinate(inside).Meters)
      .ToArray();
  }

  public async Task<IReadOnlyList<HaRestApiEntityState>> GetStates()
  {
    var result = await _url
      .WithOAuthBearerToken(_token)
      .AppendPathSegment("states")
      .GetJsonAsync<HaRestApiEntityState[]>();
    
    return result.ToArray();
  }
}

public abstract class HaEntity
{
  protected readonly SimpleMqttClient _mqttClient;
  protected readonly string _name;
  protected readonly HaDevice _haDevice;
  protected readonly string _id;
  
  public string Name => _name;

  protected HaEntity(SimpleMqttClient mqttClient, string name, HaDevice haDevice)
  {
    _mqttClient = mqttClient;
    _name = name;
    _haDevice = haDevice;
    _id = $"{haDevice.Identifier}_{name}";
  }

  public abstract Task PublishState();
  public abstract Task Announce();
}

public class HaDeviceTracker : HaEntity
{
  private readonly string _stateTopic;
  private readonly string _configTopic;
  private readonly string _attributesTopic;

  public double Lat { get; set; }
  public double Lon { get; set; }
  public string StateValue { get; set; }

  public HaDeviceTracker(SimpleMqttClient mqttClient, string name, HaDevice haDevice) 
    : base(mqttClient, name, haDevice)
  {
    _stateTopic = $"homeassistant/sensor/{_id}/state";
    _configTopic = $"homeassistant/sensor/{_id}/config";
    _attributesTopic = $"homeassistant/sensor/{_id}/attributes";
  }

  public override async Task PublishState()
  {
    await _mqttClient.Pub(_stateTopic, $"{this.StateValue}");
    
    await _mqttClient.Pub(_attributesTopic, $$"""
    {
      "latitude": {{this.Lat}}, 
      "longitude": {{this.Lon}},
      "source_type":"gps", 
      "gps_accuracy": 2
    }
    
    """ );
  }

  public override async Task Announce()
  {
    await _mqttClient.Pub(_configTopic, $$""" 
    {
      "device":{
        "identifiers":["{{ _haDevice.Identifier }}"],
        "manufacturer":"{{ _haDevice.Manufacturer }}", 
        "model":"{{ _haDevice.Model }}",
        "name":"{{ _haDevice.Name }}",
        "sw_version":"{{ _haDevice.Version }}"},
      "name":"{{ _name }}",
      "state_topic":"{{ _stateTopic }}",
      "unique_id":"{{ _id }}",
      "platform":"mqtt",
      "json_attributes_topic": "{{ _attributesTopic }}"
    }
    
    """);

    await Task.Delay(200);
  }
}

public class HaSensor : HaEntity
{
  public string Value { get; set; } = "";
  public string Icon { get; set; } = "mdi:eye";
  public string Unit { get; set; } = "";
  public string DeviceClass { get; set; } = "";
  
  private readonly string _stateTopic;
  private readonly string _configTopic;

  public HaSensor(SimpleMqttClient mqttClient, string name, HaDevice haDevice) : base(mqttClient, name, haDevice)
  {
    _stateTopic = $"homeassistant/sensor/{_id}/state";
    _configTopic = $"homeassistant/sensor/{_id}/config";
  }

  public override async Task PublishState()
  {
    await _mqttClient.Pub(_stateTopic, $"{this.Value}");
  }

  public override async Task Announce()
  {

    var unitOfMeasurementJson =
      string.IsNullOrWhiteSpace(this.Unit) ? "" : $"\"unit_of_measurement\":\"{this.Unit}\",";
    var deviceClassJson =
      string.IsNullOrWhiteSpace(this.DeviceClass) ? "" : $"\"device_class\":\"{this.DeviceClass}\"," ;
    var iconJson =
      string.IsNullOrWhiteSpace(this.DeviceClass) ? $"\"icon\":\"{this.Icon}\"," : "" ;

    await _mqttClient.Pub(_configTopic, $$""" 
    {
      "device":{
        "identifiers":["{{ _haDevice.Identifier }}"],
        "manufacturer":"{{ _haDevice.Manufacturer }}", 
        "model":"{{ _haDevice.Model }}",
        "name":"{{ _haDevice.Name }}",
        "sw_version":"{{ _haDevice.Version }}"},
      "name":"{{ _name }}",
      {{ unitOfMeasurementJson }}
      {{ deviceClassJson }}
      {{ iconJson }}
      "state_topic":"{{ _stateTopic }}",
      "unique_id":"{{ _id }}",
      "platform":"mqtt"
    }
    
    """);

    await Task.Delay(200);
  }
}

public class HaButton : HaEntity
{
  private readonly string _commandTopic;
  private readonly string _configTopic;

  public HaButton(SimpleMqttClient mqttClient, string name, HaDevice haDevice, Func<HaButton, Task> onPressedCommand)
    : base(mqttClient, name, haDevice)
  {
    _commandTopic = $"homeassistant/button/{_id}/set";
    _configTopic = $"homeassistant/button/{_id}/config";

    _ = mqttClient.Sub(_commandTopic, async _ =>
    {
      await onPressedCommand.Invoke(this);
    });
  }

  public override Task PublishState()
  {
    return Task.CompletedTask;
  }

  public override async Task Announce()
  {
    await _mqttClient.Pub(_configTopic, $$""" 
    {
      "device":{
        "identifiers":["{{ _haDevice.Identifier }}"],
        "manufacturer":"{{ _haDevice.Manufacturer }}", 
        "model":"{{ _haDevice.Model }}",
        "name":"{{ _haDevice.Name }}",
        "sw_version":"{{ _haDevice.Version }}"},
      "name":"{{ _name }}",
      "command_topic":"{{ _commandTopic }}",
      "unique_id":"{{ _id }}",
      "platform":"mqtt"
    }
    
    """);
  }
}

public class HaSwitch : HaEntity
{
  private readonly string _commandTopic;
  private readonly string _stateTopic;
  private readonly string _configTopic;

  public bool IsOn { get; private set; }

  public void SwitchTo(bool onOrOff)
  {
    this.IsOn = onOrOff;
    _ = this.PublishState();
  }

  public HaSwitch(SimpleMqttClient mqttClient, string name, HaDevice haDevice, Func<HaSwitch, Task> onSwitchCommand) 
    : base(mqttClient, name, haDevice)
  {
    _commandTopic = $"homeassistant/switch/{_id}/set";
    _stateTopic = $"homeassistant/switch/{_id}/state";
    _configTopic = $"homeassistant/switch/{_id}/config";

    _ = mqttClient.Sub(_commandTopic, async message =>
    {
      this.SwitchTo(message == "ON");
      await Task.Delay(100);
      await onSwitchCommand.Invoke(this);
    });
  }

  public override async Task PublishState()
  {
    var mqttState = this.IsOn ? "ON" : "OFF";
    
    await _mqttClient.Pub(_stateTopic, $"{mqttState}");
  }
  
  public override async Task Announce()
  {
    await _mqttClient.Pub(_configTopic, $$""" 
    {
      "device":{
        "identifiers":["{{ _haDevice.Identifier }}"],
        "manufacturer":"{{ _haDevice.Manufacturer }}", 
        "model":"{{ _haDevice.Model }}",
        "name":"{{ _haDevice.Name }}",
        "sw_version":"{{ _haDevice.Version }}"},
      "name":"{{ _name }}",
      "command_topic":"{{ _commandTopic }}",
      "state_topic":"{{ _stateTopic }}",
      "unique_id":"{{ _id }}",
      "platform":"mqtt"
    }
    
    """);
  }
}

public class HaDevice
{
  public string Name { get; set;  }
  public string Manufacturer { get; set; }
  public string Model { get; set; }
  public string Version { get; set; }
  public string Identifier { get; set;  }
}