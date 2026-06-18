using Microsoft.AspNetCore.Mvc;
using Notifications.Api.Application.Dtos;
using Notifications.Api.Core.Interfaces;

namespace Notifications.Api.Application.V1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NotificationsController(INotificationService notificationService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<NotificationMessageResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<NotificationMessageResponse>>> GetAllAsync()
    {
        var notifications = await notificationService.GetAllAsync();
        return Ok(notifications);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(NotificationMessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationMessageResponse>> GetByIdAsync(long id)
    {
        var notifications = await notificationService.GetByIdAsync(id);
        if (notifications == null)
        {
            return NotFound();
        }
        return Ok(notifications);
    }
}