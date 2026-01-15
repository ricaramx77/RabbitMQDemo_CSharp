using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Configuration;
using System.Text;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

string? exchangeName = config["RabbitMQ:ExchangeName"];
string? queueName = config["RabbitMQ:QueueName"];
string? routingKey = config["RabbitMQ:RoutingKey"];

if (exchangeName is null)
    throw new InvalidOperationException("RabbitMQ:ExchangeName is not configured.");
if (queueName is null)
    throw new InvalidOperationException("RabbitMQ:QueueName is not configured.");
if (routingKey is null)
    throw new InvalidOperationException("RabbitMQ:RoutingKey is not configured.");


await channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Direct);

await channel.QueueDeclareAsync(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

await channel.QueueBindAsync(queue: queueName, exchange: exchangeName, routingKey: routingKey);

Console.WriteLine(" [*] Waiting for messages.");

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += (model, ea) =>
{
    Task.Delay(TimeSpan.FromSeconds(5)).Wait();
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine(" [x] Received " + message);
    return Task.CompletedTask;
};

await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();