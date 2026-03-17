using MediatR;
using TaskManager.Notification.Application.DTOs;
using TaskManager.Shared.Application.Models;

namespace TaskManager.Notification.Application.Queries.GetNotifications;

public record GetNotificationsQuery(string UserId, int Page = 1, int PageSize = 20, bool? IsRead = null)
    : IRequest<Result<PagedResult<NotificationResponse>>>;
