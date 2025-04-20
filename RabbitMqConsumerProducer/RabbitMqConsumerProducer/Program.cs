// Producer code for RabbitMQ messaging
using RabbitMQ.Client;
using System.Text;

// Create a connection factory pointing to the local RabbitMQ server
var factory = new ConnectionFactory() { HostName = "localhost" };

// Establish an asynchronous connection to the RabbitMQ server
using var connection = await factory.CreateConnectionAsync();

// Create a channel for communication with RabbitMQ
using var channel = await connection.CreateChannelAsync();

// Declare a queue named "messageQueue" with following properties:
// - durable: true - queue will survive broker restarts
// - exclusive: false - queue can be accessed by other connections
// - autoDelete: false - queue won't be deleted when consumer disconnects
await channel.QueueDeclareAsync(queue: "messageQueue", durable: true, exclusive: false, autoDelete: false, arguments: null);

// Generate and send 10 messages with 1 second delay between each
for (int i = 0; i < 10; i++)
{
    // Create a unique message with GUID, counter, and timestamp
    var message = $"Message: ${Guid.NewGuid()} - Hello World {i} - ${DateTime.Now}";

    // Convert the message string to a byte array for transmission
    var body = Encoding.UTF8.GetBytes(message);

    // Publish the message to the queue with these properties:
    // - exchange: empty string (default exchange)
    // - routingKey: "messageQueue" (direct to our queue)
    // - mandatory: true - return message if it can't be routed
    // - basicProperties: setting Persistent=true ensures message survives broker restarts
    await channel.BasicPublishAsync(
        exchange: string.Empty,
        routingKey: "messageQueue",
        mandatory: true,
        basicProperties: new BasicProperties { Persistent = true },
        body: body);

    // Log that the message was sent
    Console.WriteLine($" [x] Sent {message}");

    // Wait for 1 second before sending the next message
    await Task.Delay(1000);
}