using Notifications.Api.Core.Enums;

namespace Notifications.Api.Application.Dtos;

public class NotificationMessageCreateRequest
{
    public string? Type { get; set; }
    public MessageChannel? MessageChannel { get; set; }
    public string? Recipient { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; } 
    public Status Status { get; set; } = Status.Pending;
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}