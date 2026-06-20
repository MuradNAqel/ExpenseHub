using Notifications.Api.Application.Dtos;

namespace Notifications.Api.Application.Mappers;

internal static class NotificationMapper
{
    public static NotificationMessageResponse MapNotification(NotificationMessageDataRow message)
    {
        return new NotificationMessageResponse
        {
            Id = message.Id,
            Body = message.Body,
            CreatedAt = message.CreatedAt,
            SentAt = message.SentAt,
            Recipient = message.Recipient,
            Subject = message.Subject,
            Type = message.Type,
            ErrorMessage = message.ErrorMessage,
            MessageChannel = message.MessageChannel,
            Status = message.Status,
        };
    }
}
