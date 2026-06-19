namespace NotificationCore.Telegram;

public sealed class TelegramOptions
{
    public string BotToken { get; set; } = string.Empty;
    public string DefaultChatId { get; set; } = string.Empty;
}
