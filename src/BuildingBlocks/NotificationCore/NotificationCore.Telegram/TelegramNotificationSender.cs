using System.Net.Http.Json;
using NotificationCore;
using Microsoft.Extensions.Options;

namespace NotificationCore.Telegram;

public sealed class TelegramNotificationSender(
    HttpClient httpClient,
    IOptions<TelegramOptions> options)
    : ITelegramNotificationSender
{
    public string DefaultChatId => options.Value.DefaultChatId;

    public async Task SendAsync(
        string chatId,
        string message,
        CancellationToken cancellationToken = default)
    {
        var settings = options.Value;

        if (string.IsNullOrWhiteSpace(settings.BotToken))
        {
            throw new InvalidOperationException("Telegram BotToken is not configured.");
        }

        if (string.IsNullOrWhiteSpace(chatId))
        {
            throw new InvalidOperationException("Telegram chat id is not configured.");
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        var response = await httpClient.PostAsJsonAsync(
            $"/bot{settings.BotToken}/sendMessage",
            new
            {
                chat_id = chatId,
                text = message
            },
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var error = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException($"Telegram sendMessage failed: {error}");
    }
}
