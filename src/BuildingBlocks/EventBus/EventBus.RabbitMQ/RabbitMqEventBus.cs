using System.Text.Json;
using EventBus.Interfaces;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace EventBus.RabbitMQ;

public sealed class RabbitMqEventBus(IOptions<RabbitMqOptions> options) : IEventBus
{
    public async Task PublishAsync<TEvent>(
        TEvent integrationEvent,
        CancellationToken cancellationToken = default)
        where TEvent : class
    {
        var settings = options.Value;
        var factory = CreateConnectionFactory(settings);

        await using var connection = await factory.CreateConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: settings.ExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        var routingKey = typeof(TEvent).Name;
        var body = JsonSerializer.SerializeToUtf8Bytes(integrationEvent);

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            Type = routingKey,
            MessageId = Guid.NewGuid().ToString()
        };

        await channel.BasicPublishAsync(
            exchange: settings.ExchangeName,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }

    internal static ConnectionFactory CreateConnectionFactory(RabbitMqOptions options)
    {
        return new ConnectionFactory
        {
            HostName = options.HostName,
            Port = options.Port,
            UserName = options.UserName,
            Password = options.Password,
            RequestedConnectionTimeout = TimeSpan.FromSeconds(options.ConnectionTimeoutSeconds)
        };
    }
}
