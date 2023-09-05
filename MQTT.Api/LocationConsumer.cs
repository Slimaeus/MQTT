using MQTTnet.Client;
using Newtonsoft.Json.Linq;
using System.Text;

namespace MQTT.Api;

public class LocationConsumer : BackgroundService
{
    private readonly IMqttClient _mqttClient;

    public LocationConsumer(IMqttClient mqttClient)
    {
        _mqttClient = mqttClient;
    }
    private static VehicleLocation ToVehicleLocation(string payload)
    {
        // Deserialize the JSON payload into a JObject
        var jsonObject = JObject.Parse(payload);

        // Map the JSON properties to the VehicleLocation class
        var vehicleLocation = new VehicleLocation
        {
            Id = jsonObject.Value<string>("id"),
            Latitude = jsonObject.Value<double?>("latitude"),
            Longitude = jsonObject.Value<double?>("longitude"),
        };

        return vehicleLocation;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
            if (_mqttClient.IsConnected)
            {
                await _mqttClient.SubscribeAsync("/location", cancellationToken: stoppingToken);
                {
                    _mqttClient.ApplicationMessageReceivedAsync += e =>
                    {
                        if (e.ApplicationMessage != null)
                        {
                            Console.WriteLine($"Received message: {Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment)}");

                            string payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
                            try
                            {

                                var vehicleLocation = ToVehicleLocation(payload);
                                // Now, you can use the vehicleLocation object as needed.
                                Console.WriteLine($"Vehicle Location: Id={vehicleLocation.Id}, Latitude={vehicleLocation.Latitude}, Longitude={vehicleLocation.Longitude}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error deserializing message: {ex.Message}");
                            }
                        }
                        return Task.CompletedTask;
                    };
                    await Task.Delay(1000, stoppingToken);
                }
                break;
            }
    }
}
