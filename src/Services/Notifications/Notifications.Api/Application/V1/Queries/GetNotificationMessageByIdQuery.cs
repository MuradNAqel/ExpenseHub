using Dapper;
using Notifications.Api.Application.Abstractions;
using Notifications.Api.Application.Dtos;
using Notifications.Api.Application.Mappers;
using Notifications.Api.Core.Interfaces;

namespace Notifications.Api.Application.V1.Queries;

public sealed record GetNotificationMessageByIdQuery(long Id);

public sealed class GetNotificationMessageByIdQueryHandler(
    IDbConnectionFactory connectionFactory)
    : IQueryHandler<GetNotificationMessageByIdQuery, NotificationMessageResponse?>
{
    public async Task<NotificationMessageResponse?> HandleAsync(GetNotificationMessageByIdQuery query)
    {
        var connection = connectionFactory.CreateConnection();
        connection.Open();

        const string sql =
            """
                Select 
                        [Id]
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
                WHERE [Id] = @Id;
            """;
        var notification = await connection.QueryFirstOrDefaultAsync<NotificationMessageDataRow>(sql, new { query.Id });

        if (notification is null)
        {
            return null;
        }
        
        return NotificationMapper.MapNotification(notification);
    }
}