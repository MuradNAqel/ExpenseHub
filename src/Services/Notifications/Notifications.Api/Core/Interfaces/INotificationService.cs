using Notifications.Api.Application.Dtos;

namespace Notifications.Api.Core.Interfaces;

public interface INotificationService
{
    Task<IReadOnlyList<NotificationMessageResponse>> GetAllAsync();
    Task<NotificationMessageResponse?> GetByIdAsync(long id);
}