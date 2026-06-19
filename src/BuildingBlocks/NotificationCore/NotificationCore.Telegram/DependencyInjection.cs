using NotificationCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NotificationCore.Telegram;

public static class DependencyInjection
{
    public static IServiceCollection AddTelegramNotifications(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<TelegramOptions>(configuration.GetSection("Telegram"));

        services.AddHttpClient<ITelegramNotificationSender, TelegramNotificationSender>(client =>
        {
            client.BaseAddress = new Uri("https://api.telegram.org/");
        });

        return services;
    }
}
