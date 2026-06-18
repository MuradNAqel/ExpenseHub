using Dapper;
using Notifications.Api.Application.Abstractions;
using Notifications.Api.Application.Dtos;
using Notifications.Api.Application.Mappers;
using Notifications.Api.Core.Interfaces;

namespace Notifications.Api.Application.Commands;

public sealed record CreateNotificationCommand(NotificationMessageCreateRequest Request);

public sealed class CreateNotificationCommandHandler(IDbConnectionFactory connectionFactory)
    : ICommandHandler<CreateNotificationCommand, NotificationMessageResponse>
{
    public async Task<NotificationMessageResponse> HandleAsync(CreateNotificationCommand command)
    {
        var request = command.Request;

        using var connection = connectionFactory.CreateConnection();
        connection.Open();

        using var transaction = connection.BeginTransaction();

        try
        {
            const string sql = """
                               INSERT INTO NotificationMessages
                                   (
                                       Type,
                                       MessageChannel,
                                       Recipient,
                                       Subject,
                                       Body,
                                       Status,
                                       ErrorMessage,
                                       CreatedAt,
                                       SentAt
                                   )
                               OUTPUT
                                   INSERTED.Id,
                                   INSERTED.Type,
                                   INSERTED.MessageChannel,
                                   INSERTED.Recipient,
                                   INSERTED.Subject,
                                   INSERTED.Body,
                                   INSERTED.Status,
                                   INSERTED.ErrorMessage,
                                   INSERTED.CreatedAt,
                                   INSERTED.SentAt
                               VALUES
                                   (
                                       @Type,
                                       @MessageChannel,
                                       @Recipient,
                                       @Subject,
                                       @Body,
                                       @Status,
                                       @ErrorMessage,
                                       SYSUTCDATETIME(),
                                       @SentAt
                                   );
                               """;
            var notification = await connection.QuerySingleAsync<NotificationMessageDataRow>(
                sql,
                new
                {
                    request.Type,
                    MessageChannel = request.MessageChannel?.ToString(),
                    request.Recipient,
                    request.Subject,
                    request.Body,
                    Status = request.Status.ToString(),
                    request.ErrorMessage,
                    SentAt = (DateTime?)null
                },
                transaction);

            transaction.Commit();
            return NotificationMapper.MapNotification(notification);
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
