using MQTTnet.Client;
using System.Text;

namespace MQTT.Api;

public class HelloConsumer : BackgroundService
{
    private readonly IMqttClient _mqttClient;

    public HelloConsumer(IMqttClient mqttClient)
    {
        _mqttClient = mqttClient;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
            if (_mqttClient.IsConnected)
            {
                await _mqttClient.SubscribeAsync("/test/hello", cancellationToken: stoppingToken);
                {
                    _mqttClient.ApplicationMessageReceivedAsync += e =>
                    {
                        if (e.ApplicationMessage != null)
                        {
                            Console.WriteLine($"Received message: {Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment)}");
                        }
                        return Task.CompletedTask;
                    };
                    await Task.Delay(1000, stoppingToken);
                }
                break;
            }
    }
}
