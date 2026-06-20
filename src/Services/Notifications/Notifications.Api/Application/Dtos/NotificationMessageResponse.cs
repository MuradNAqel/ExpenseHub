
namespace Notifications.Api.Application.Dtos;

public class NotificationMessageResponse
{
    public long Id { get; set; }
    public string? Type { get; set; }
    public MessageChannel? MessageChannel { get; set; }
    public string? Recipient { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public Status Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
}