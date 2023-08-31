using Microsoft.Extensions.Options;
using MQTT.Api;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<MessageTransport>(builder.Configuration.GetSection(nameof(MessageTransport)));
var messageTransportSettings = builder.Services.BuildServiceProvider().GetService<IOptions<MessageTransport>>()!.Value;

var broker = messageTransportSettings.Host;
string userName = messageTransportSettings.UserName;
string password = messageTransportSettings.Password;
int port = messageTransportSettings.MQTT;
string clientId = $"Profio-{new Random().Next(1, 1000)}";
string topic = "/test/hello";

var factory = new MqttFactory();


// Create a MQTT client instance
using var mqttClient = factory.CreateMqttClient();
// Create MQTT client options
var options = new MqttClientOptionsBuilder()
    .WithTcpServer(broker, port) // MQTT broker address and port
    .WithCredentials(userName, password)
    /*.WithCredentials(string.Empty, string.Empty)*/ // Set username and password
    .WithClientId(clientId)
    //.WithCleanSession()
    ////.WithTls(
    ////    o =>
    ////    {
    ////        // The used public broker sometimes has invalid certificates. This sample accepts all
    ////        // certificates. This should not be used in live environments.
    ////        o.CertificateValidationHandler = _ => true;

    ////        // The default value is determined by the OS. Set manually to force version.
    ////        o.SslProtocol = SslProtocols.Tls12;

    ////        // Please provide the file path of your certificate file. The current directory is /bin.
    ////        var certificate = new X509Certificate("/opt/emqxsl-ca.crt", "");
    ////        o.Certificates = new List<X509Certificate> { certificate };
    ////    }
    ////)
    .WithTls(new MqttClientOptionsBuilderTlsParameters
    {
        UseTls = true,
        SslProtocol = System.Security.Authentication.SslProtocols.Tls12,

        //Certificates = new[] { certificate },
        CertificateValidationHandler = delegate { return true; },
    })
    //.WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
    //.WithTls(
    //                o => o.CertificateValidationHandler =
    //                    // The used public broker sometimes has invalid certificates. This sample accepts all
    //                    // certificates. This should not be used in live environments.
    //                    _ => true)
    .Build();

var connectResult = await mqttClient.ConnectAsync(options, CancellationToken.None);

if (connectResult.ResultCode == MqttClientConnectResultCode.Success)
{
    Console.WriteLine("Connected to MQTT broker successfully.");

    // Subscribe to a topic
    await mqttClient.SubscribeAsync(topic);

    // Callback function when a message is received
    mqttClient.ApplicationMessageReceivedAsync += e =>
    {
        Console.WriteLine($"Received message: {Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment)}");
        return Task.CompletedTask;
    };

    // Publish a message 10 times
    for (int i = 0; i < 1000; i++)
    {
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload($"Hello, MQTT! Message number {i}")
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .WithRetainFlag()
            .Build();

        await mqttClient.PublishAsync(message);
        await Task.Delay(1000); // Wait for 1 second
    }

    // Unsubscribe and disconnect
    await mqttClient.UnsubscribeAsync(topic);
    await mqttClient.DisconnectAsync();
}
else
{
    Console.WriteLine($"Failed to connect to MQTT broker: {connectResult.ResultCode}");
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
