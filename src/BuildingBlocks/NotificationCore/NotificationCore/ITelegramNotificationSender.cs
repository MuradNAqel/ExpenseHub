namespace NotificationCore;

public interface ITelegramNotificationSender
{
    string DefaultChatId { get; }

    Task SendAsync(
        string chatId,
        string message,
        CancellationToken cancellationToken = default);
}
