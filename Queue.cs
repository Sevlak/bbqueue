using System.Text;
using RabbitMQ.Client;
using IModel = RabbitMQ.Client.IModel;

namespace BBQueue;

public class Queue
{
    private readonly IModel _channel;
    private readonly IConfiguration _configuration;

    public Queue(IConfiguration configuration)
    {
        _configuration = configuration;
        var uri = _configuration.GetConnectionString("Queue");
        var factory = new ConnectionFactory{Uri = new Uri(uri)};
        var connection = factory.CreateConnection();
        _channel = connection.CreateModel();

        _channel.QueueDeclare("votos_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
    }

    public void Publish(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish(string.Empty, "votos_queue", null, body);
    }
}