using System.Text;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BBQueue;

public class Worker : BackgroundService
{
    private readonly IModel _channel;
    private EventingBasicConsumer _consumer;
    private readonly IServiceProvider _serviceProvider;

    public Worker(IServiceProvider provider, IConfiguration configuration)
    {
        var uri = configuration.GetConnectionString("Queue");
        var factory = new ConnectionFactory{Uri = new Uri(uri)};
        var connection = factory.CreateConnection();
        _channel = connection.CreateModel();

        _channel.QueueDeclare("votes_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
        _serviceProvider = provider;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //We don't need a loop to consume votes here since they'll trigger the Received event.
        _consumer = new EventingBasicConsumer(_channel);

        _consumer.Received += async (channel, ea) =>
        {
            var body = ea.Body.ToArray();
            var vote = Encoding.UTF8.GetString(body);

            if (!Int32.TryParse(vote, out var id))
            {
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<VotesContext>();

            await ctx.Contestants.Where(c => c.Id == id)
                .ExecuteUpdateAsync(b => b.SetProperty(x => x.VoteQty, x => x.VoteQty + 1), stoppingToken);

            _channel.BasicAck(ea.DeliveryTag, false);
        };

        _channel.BasicConsume("votes_queue", false, _consumer);

        return Task.CompletedTask;
    }
}