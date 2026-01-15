using RabbitMQ.Client;
using System.Text;
using Microsoft.Extensions.Configuration;


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

for(int i = 0; i <= 5; i++)
{
    const string message = "Hello World 2!";
    var body = Encoding.UTF8.GetBytes(message);

    await channel.BasicPublishAsync(exchange: exchangeName, routingKey: routingKey, body: body);
    Console.WriteLine(" [x] Sent " + message);
}

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();