namespace FiatChamp.HA;

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

  public HaDeviceTracker(SimpleMqttClient mqttClient, string name, HaDevice haDevice) 
    : base(mqttClient, name, haDevice)
  {
    _stateTopic = $"homeassistant/sensor/{_id}/state";
    _configTopic = $"homeassistant/sensor/{_id}/config";
    _attributesTopic = $"homeassistant/sensor/{_id}/attributes";
  }

  public override async Task PublishState()
  {
    await _mqttClient.Pub(_attributesTopic, $$"""
    {
      "latitude": {{this.Lat}}, 
      "longitude": {{this.Lon}}, 
      "gps_accuracy": 1.2
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
    await _mqttClient.Pub(_configTopic, $$""" 
    {
      "device":{
        "identifiers":["{{ _haDevice.Identifier }}"],
        "manufacturer":"{{ _haDevice.Manufacturer }}", 
        "model":"{{ _haDevice.Model }}",
        "name":"{{ _haDevice.Name }}",
        "sw_version":"{{ _haDevice.Version }}"},
      "name":"{{ _name }}",
      "unit_of_measurement":"{{ this.Unit }}",
      "icon":"{{ this.Icon }}",
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