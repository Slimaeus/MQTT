using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System.Text;
using System.Text.Json;

namespace MQTT.Api;

public class HelloPublisher : BackgroundService
{
    private readonly IMqttClient _mqttClient;

    public HelloPublisher(IMqttClient mqttClient)
    {
        _mqttClient = mqttClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)

            if (_mqttClient.IsConnected)
            {
                for (int i = 0; i < 10; i++)
                {
                    var message = new MqttApplicationMessageBuilder()
                        .WithTopic("/test/hello")
                        .WithPayload(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(new { content = $"Hello, MQTT! Message number {i}" })))
                        .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                        .WithRetainFlag()
                        .Build();

                    await _mqttClient.PublishAsync(message, stoppingToken);
                    await Task.Delay(1000, stoppingToken); // Wait for 1 second
                }
                break;
            }
    }
}
