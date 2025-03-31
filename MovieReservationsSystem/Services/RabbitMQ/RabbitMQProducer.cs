using RabbitMQ.Client;
using System.Text.Json;
using System.Text;
using Newtonsoft.Json;

namespace MovieReservationsSystem.Services.RabbitMQ;

public class RabbitMQProducer
{
    private readonly string _hostName = "localhost";
    private readonly string _queueName = "reservations";

    public async Task PublishMessage()
    {
        var factory = new ConnectionFactory()
        {
            HostName = _hostName,
            Port = 5672,
            UserName = "guest",
            Password = "guest",
        };

        factory.ClientProvidedName = "Rabbit Test";
        
        IConnection connection = await factory.CreateConnectionAsync();
        IChannel channel = await connection.CreateChannelAsync();
        
        await channel.QueueDeclareAsync(queue: _queueName, durable: true, exclusive: false, autoDelete: false, 
            arguments: null);

        // string emailJson = JsonConvert.SerializeObject(message);
    }
}