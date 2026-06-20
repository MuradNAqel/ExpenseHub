using Dapper;
using Notifications.Api.Application.Abstractions;
using Notifications.Api.Application.Dtos;
using Notifications.Api.Application.Mappers;

namespace Notifications.Api.Application.V1.Queries;

public sealed record GetAllNotificationMessagesQuery;

public sealed class GetAllNotificationMessagesQueryHandler(IDbConnectionFactory connectionFactory)
    : IQueryHandler<GetAllNotificationMessagesQuery, IReadOnlyList<NotificationMessageResponse>>
{
    public async Task<IReadOnlyList<NotificationMessageResponse>> HandleAsync(GetAllNotificationMessagesQuery query)
    {
        using var connection = connectionFactory.CreateConnection();

        const string sqlQuery =
            """
                SELECT [Id]
                ,[Type]
                ,[MessageChannel]
                ,[Recipient]
                ,[Subject]
                ,[Body]
                ,[Status]
                ,[ErrorMessage]
                ,[CreatedAt]
                ,[SentAt]
            FROM [dbo].[NotificationMessages]
            ORDER BY [CreatedAt] Desc;
            """;

        var notifications = await connection.QueryAsync<NotificationMessageDataRow>(sqlQuery);
        return notifications.Select(NotificationMapper.MapNotification).ToList();
    }
}