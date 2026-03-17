using MediatR;
using TaskManager.Notification.Application.Commands.CreateNotification;
using TaskManager.Notification.Application.DTOs;
using TaskManager.Notification.Domain.Interfaces;
using TaskManager.Shared.Application.Models;

namespace TaskManager.Notification.Application.Queries.GetNotifications;

public class GetNotificationsQueryHandler(
    INotificationRepository repository) : IRequestHandler<GetNotificationsQuery, Result<PagedResult<NotificationResponse>>>
{
    public async Task<Result<PagedResult<NotificationResponse>>> Handle(GetNotificationsQuery request, CancellationToken ct)
    {
        var items = await repository.GetByUserIdAsync(request.UserId, request.Page, request.PageSize, request.IsRead, ct);
        var totalCount = await repository.CountAsync(
            n => n.UserId == request.UserId && (request.IsRead == null || n.IsRead == request.IsRead), ct);

        var result = new PagedResult<NotificationResponse>
        {
            Items = items.Select(CreateNotificationCommandHandler.MapToResponse).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return Result<PagedResult<NotificationResponse>>.Success(result);
    }
}
