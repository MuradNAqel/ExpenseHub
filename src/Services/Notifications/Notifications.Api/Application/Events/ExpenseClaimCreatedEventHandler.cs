using EventBus.Events;
using EventBus.Interfaces;
using NotificationCore;
using Notifications.Api.Application.Abstractions;
using Notifications.Api.Application.Commands;
using Notifications.Api.Application.Dtos;

namespace Notifications.Api.Application.Events;

public sealed class ExpenseClaimCreatedEventHandler(
    ICommandHandler<CreateNotificationCommand, NotificationMessageResponse> createNotificationHandler,
    ITelegramNotificationSender telegramNotificationSender)
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
            Recipient = telegramNotificationSender.DefaultChatId,
            Subject = "Expense claim created",
            Body =
                $"Expense claim #{integrationEvent.ExpenseClaimId} was created. Total: {integrationEvent.TotalAmount}.",
            Status = Status.Pending
        };

        var notification = await createNotificationHandler.HandleAsync(new CreateNotificationCommand(request));

        await telegramNotificationSender.SendAsync(
            notification.Recipient!,
            notification.Body!,
            cancellationToken);
    }
}
