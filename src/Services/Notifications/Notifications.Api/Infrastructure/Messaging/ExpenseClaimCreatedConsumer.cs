using EventBus.Events;
using EventBus.RabbitMQ;
using Microsoft.Extensions.Options;

namespace Notifications.Api.Infrastructure.Messaging;

public sealed class ExpenseClaimCreatedConsumer(
    IServiceProvider serviceProvider,
    IOptions<RabbitMqOptions> options,
    ILogger<ExpenseClaimCreatedConsumer> logger)
    : RabbitMqEventConsumer<ExpenseClaimCreatedEvent>(
        serviceProvider,
        options,
        logger,
        queueName: "notifications.expense-claim-created",
        routingKey: nameof(ExpenseClaimCreatedEvent));
