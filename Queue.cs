using System.Text;
using RabbitMQ.Client;
using IModel = RabbitMQ.Client.IModel;

namespace BBQueue;

public class Queue
{
    private readonly IModel _channel;

    public Queue(IConfiguration configuration)
    {
        var uri = configuration.GetConnectionString("Queue");
        var factory = new ConnectionFactory{Uri = new Uri(uri)};
        var connection = factory.CreateConnection();
        _channel = connection.CreateModel();

        _channel.QueueDeclare("votes_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
    }

    public void Publish(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish(string.Empty, "votes_queue", null, body);
    }
}