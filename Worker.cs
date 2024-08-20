using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BBQueue;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IModel _channel;
    private EventingBasicConsumer _consumer;

    public Worker(ILogger<Worker> logger, IConfiguration configuration)
    {
        var uri = configuration.GetConnectionString("Queue");
        var factory = new ConnectionFactory{Uri = new Uri(uri)};
        var connection = factory.CreateConnection();
        _channel = connection.CreateModel();

        _channel.QueueDeclare("votes_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);

        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //We don't need a loop to consume votes here since they'll trigger the Received event.
        _consumer = new EventingBasicConsumer(_channel);

        _consumer.Received += (channel, ea) =>
        {
            var body = ea.Body.ToArray();

            //do something here

            _channel.BasicAck(ea.DeliveryTag, false);
        };

        _channel.BasicConsume("votes_queue", false, _consumer);

        return Task.CompletedTask;
    }
}