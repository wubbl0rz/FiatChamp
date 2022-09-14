using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

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
      .WithTcpServer(_server, _port)
      .WithCredentials(_user, _pass);

    if (_useTls)
    {
      mqttClientOptions.WithTls();
    }

    var options = new ManagedMqttClientOptionsBuilder()
      .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
      .WithClientOptions(mqttClientOptions.Build())
      .Build();

    await _mqttClient.StartAsync(options);
  }
  
  public async Task Sub(string topic, Func<string, Task> callback)
  {
    _mqttClient.ApplicationMessageReceivedAsync += async args =>
    {
      var msg = args.ApplicationMessage;

      if (msg.Topic == topic)
      {
        await callback(msg.ConvertPayloadToString());
      }
    };

    await _mqttClient.SubscribeAsync(topic);
  }
  
  public async Task Pub(string topic, string payload)
  {
    await _mqttClient.EnqueueAsync(topic, payload);
  }
}