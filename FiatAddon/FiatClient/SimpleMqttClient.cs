using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;
using Serilog;

namespace FiatChamp;

public class SimpleMqttClient
{
  private readonly string _server;
  private readonly string _user;
  private readonly string _pass;
  private readonly string _clientId;
  private readonly bool _useTls;
  private readonly IManagedMqttClient _mqttClient;
  private readonly int? _port;

  public SimpleMqttClient(string server, int? port, string user, string pass, string clientId, bool useTls = false)
  {
    _server = server;
    _port = port;
    _user = user;
    _pass = pass;
    _clientId = clientId;
    _useTls = useTls;
    _mqttClient = new MqttFactory().CreateManagedMqttClient();
  }
  
  public async Task Connect()
  {
    var mqttClientOptions = new MqttClientOptionsBuilder()
      .WithCleanSession()
      .WithClientId(_clientId)
      .WithTcpServer(_server, _port);
    
    if (string.IsNullOrWhiteSpace(_user) || string.IsNullOrWhiteSpace(_pass))
    {
      Log.Warning("Mqtt User/Password is EMPTY.");
    }
    else
    {
      mqttClientOptions.WithCredentials(_user, _pass);
    }

    if (_useTls)
    {
      mqttClientOptions.WithTls();
    }

    var options = new ManagedMqttClientOptionsBuilder()
      .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
      .WithClientOptions(mqttClientOptions.Build())
      .Build();

    await _mqttClient.StartAsync(options);
    
    _mqttClient.ConnectedAsync += async args =>
    {
      Log.Information("Mqtt connection successful");
    };

    _mqttClient.ConnectingFailedAsync += async args =>
    {
      Log.Information("Mqtt connection failed: {0}", args.Exception);
    };
  }
  
  public async Task Sub(string topic, Func<string, Task> callback)
  {
    _mqttClient.ApplicationMessageReceivedAsync += async args =>
    {
      var msg = args.ApplicationMessage;

      if (msg.Topic == topic)
      {
        try
        {
          await callback(msg.ConvertPayloadToString());
        }
        catch (Exception e)
        {
          Log.Error("{0}", e);
        }
      }
    };

    await _mqttClient.SubscribeAsync(topic);
  }
  
  public async Task Pub(string topic, string payload)
  {
    await _mqttClient.EnqueueAsync(topic, payload, retain: true);
  }
}