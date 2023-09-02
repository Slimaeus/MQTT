using Microsoft.Extensions.Options;
using MQTT.Api;
using MQTTnet;
using MQTTnet.Client;

var builder = WebApplication.CreateBuilder(args);


//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.Configure<MessageTransport>(builder.Configuration.GetSection(nameof(MessageTransport)));
builder.Services.AddSingleton(provider =>
{
    var messageTransportSettings = provider.GetRequiredService<IOptions<MessageTransport>>()!.Value;

    var broker = messageTransportSettings.Host;
    string userName = messageTransportSettings.UserName;
    string password = messageTransportSettings.Password;
    int port = messageTransportSettings.MQTT;
    string clientId = $"Profio-{new Random().Next(1, 1000)}";

    var factory = new MqttFactory();


    var mqttClient = factory.CreateMqttClient();
    var options = new MqttClientOptionsBuilder()
        .WithTcpServer(broker, port)
        .WithCredentials(userName, password)
        .WithClientId(clientId).WithTls(new MqttClientOptionsBuilderTlsParameters
        {
            UseTls = true,
            SslProtocol = System.Security.Authentication.SslProtocols.Tls12,
            CertificateValidationHandler = delegate { return true; },
        })
        .Build();

    var result = mqttClient.ConnectAsync(options).GetAwaiter().GetResult();
    if (result.ResultCode == MqttClientConnectResultCode.Success)
    {
        Console.WriteLine("Connected");
        return mqttClient;
    }
    else
    {
        Console.WriteLine("Did not Connect");
        throw new Exception();
    }
});

builder.Services.AddHostedService<HelloConsumer>();
builder.Services.AddHostedService<HelloPublisher>();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

app.Run();
