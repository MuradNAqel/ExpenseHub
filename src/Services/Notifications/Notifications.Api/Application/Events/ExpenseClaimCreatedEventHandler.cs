using EventBus.Events;
using EventBus.Interfaces;
using Notifications.Api.Application.Abstractions;
using Notifications.Api.Application.Commands;
using Notifications.Api.Application.Dtos;
using Notifications.Api.Core.Enums;

namespace Notifications.Api.Application.Events;

public sealed class ExpenseClaimCreatedEventHandler(
    ICommandHandler<CreateNotificationCommand, NotificationMessageResponse> createNotificationHandler)
    : IIntegrationEventHandler<ExpenseClaimCreatedEvent>
{
    public async Task HandleAsync(
        ExpenseClaimCreatedEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var request = new NotificationMessageCreateRequest
        {
            Type = nameof(ExpenseClaimCreatedEvent),
            MessageChannel = MessageChannel.Telegram,
            Recipient = "telegram-chat-id-placeholder",
            Subject = "Expense claim created",
            Body =
                $"Expense claim #{integrationEvent.ExpenseClaimId} was created. Total: {integrationEvent.TotalAmount}.",
            Status = Status.Pending
        };

        await createNotificationHandler.HandleAsync(new CreateNotificationCommand(request));
    }
}
