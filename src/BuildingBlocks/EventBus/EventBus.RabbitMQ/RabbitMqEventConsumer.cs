using System.Text.Json;
using EventBus.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventBus.RabbitMQ;

public abstract class RabbitMqEventConsumer<TEvent>(
    IServiceProvider serviceProvider,
    IOptions<RabbitMqOptions> options,
    ILogger logger,
    string queueName,
    string? routingKey = null)
    : BackgroundService
    where TEvent : class
{
    private readonly string _routingKey = routingKey ?? typeof(TEvent).Name;
    private IConnection? _connection;
    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await StartConsumingAsync(settings, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "RabbitMQ consumer for {EventName} could not connect. Retrying in {RetryDelaySeconds} seconds.",
                    typeof(TEvent).Name,
                    settings.RetryDelaySeconds);

                DisposeConnection();

                await Task.Delay(
                    TimeSpan.FromSeconds(settings.RetryDelaySeconds),
                    stoppingToken);
            }
        }
    }

    private async Task StartConsumingAsync(
        RabbitMqOptions settings,
        CancellationToken stoppingToken)
    {
        var factory = RabbitMqEventBus.CreateConnectionFactory(settings);

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.ExchangeDeclareAsync(
            exchange: settings.ExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await _channel.QueueBindAsync(
            queue: queueName,
            exchange: settings.ExchangeName,
            routingKey: _routingKey,
            cancellationToken: stoppingToken);

        await _channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += HandleMessageAsync;

        await _channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        logger.LogInformation(
            "RabbitMQ consumer for {EventName} is listening on queue {QueueName}.",
            typeof(TEvent).Name,
            queueName);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleMessageAsync(object sender, BasicDeliverEventArgs eventArgs)
    {
        if (_channel is null)
        {
            return;
        }

        try
        {
            var integrationEvent = JsonSerializer.Deserialize<TEvent>(eventArgs.Body.Span);

            if (integrationEvent is null)
            {
                await _channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false);
                return;
            }

            using var scope = serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IIntegrationEventHandler<TEvent>>();

            await handler.HandleAsync(integrationEvent);
            await _channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to consume RabbitMQ event {EventName}.", typeof(TEvent).Name);
            await _channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: true);
        }
    }

    public override void Dispose()
    {
        DisposeConnection();
        base.Dispose();
    }

    private void DisposeConnection()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        _channel = null;
        _connection = null;
    }
}
