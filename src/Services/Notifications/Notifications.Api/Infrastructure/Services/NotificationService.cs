using Notifications.Api.Application.Abstractions;
using Notifications.Api.Application.Dtos;
using Notifications.Api.Application.V1.Queries;
using Notifications.Api.Core.Interfaces;

namespace Notifications.Api.Infrastructure.Services;

public class NotificationService(
    IQueryHandler<GetAllNotificationMessagesQuery, IReadOnlyList<NotificationMessageResponse>> getAllNotificationMessagesHandler,
    IQueryHandler<GetNotificationMessageByIdQuery, NotificationMessageResponse> getNotificationMessageByIdHandler)
    : INotificationService
{
    public async Task<IReadOnlyList<NotificationMessageResponse>> GetAllAsync()
    {
        return await getAllNotificationMessagesHandler.HandleAsync(new GetAllNotificationMessagesQuery());
    }

    public async Task<NotificationMessageResponse?> GetByIdAsync(long id)
    {
        return await getNotificationMessageByIdHandler.HandleAsync(new GetNotificationMessageByIdQuery(id));
    }
}
