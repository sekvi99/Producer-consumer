// Consumer code for RabbitMQ messaging
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

// Create a connection factory pointing to the local RabbitMQ server
var factory = new ConnectionFactory() { HostName = "localhost" };

// Establish an asynchronous connection to the RabbitMQ server
using var connection = await factory.CreateConnectionAsync();

// Create a channel for communication with RabbitMQ
using var channel = await connection.CreateChannelAsync();

// Declare the same queue as the producer to ensure it exists
// Queue parameters must match those used by the producer
await channel.QueueDeclareAsync(queue: "messageQueue", durable: true, exclusive: false, autoDelete: false, arguments: null);

// Inform the user that the consumer is ready to receive messages
Console.WriteLine("Waiting for messagess...");

// Create an asynchronous consumer for processing incoming messages
var consumer = new AsyncEventingBasicConsumer(channel);

// Set up the event handler for received messages
consumer.ReceivedAsync += async (model, ea) =>
{
    // Extract the message body from the delivery arguments
    byte[] body = ea.Body.ToArray();

    // Convert the message bytes back to a string
    var message = Encoding.UTF8.GetString(body);

    // Display the received message
    Console.WriteLine($" [x] Received {message}");

    // Simulate message processing time with a 1-second delay
    await Task.Delay(1000);

    // Acknowledge the message to confirm it was successfully processed
    // - deliveryTag: unique identifier for the delivery
    // - multiple: false - acknowledge only this specific message
    await ((AsyncEventingBasicConsumer)model).Channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
};

// Start consuming messages from the queue
// - autoAck: false - explicit acknowledgment required (handled in the event handler)
await channel.BasicConsumeAsync(queue: "messageQueue", autoAck: false, consumer: consumer);

// Keep the application running until user presses Enter
Console.ReadLine();