using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Notification.Application.Commands.MarkAllAsRead;
using TaskManager.Notification.Application.Commands.MarkAsRead;
using TaskManager.Notification.Application.Queries.GetNotifications;
using TaskManager.Notification.Application.Queries.GetUnreadCount;

namespace TaskManager.Notification.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool? isRead = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await mediator.Send(new GetNotificationsQuery(userId, page, pageSize, isRead));
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await mediator.Send(new GetUnreadCountQuery(userId));
        return result.IsSuccess ? Ok(new { count = result.Data }) : StatusCode(result.StatusCode, result.Error);
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(string id)
    {
        var result = await mediator.Send(new MarkAsReadCommand(id));
        return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, result.Error);
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await mediator.Send(new MarkAllAsReadCommand(userId));
        return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, result.Error);
    }
}
