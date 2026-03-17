using MediatR;
using TaskManager.Notification.Application.DTOs;
using TaskManager.Notification.Domain.Interfaces;
using TaskManager.Shared.Application.Models;

namespace TaskManager.Notification.Application.Commands.CreateNotification;

public class CreateNotificationCommandHandler(
    INotificationRepository repository) : IRequestHandler<CreateNotificationCommand, Result<NotificationResponse>>
{
    public async Task<Result<NotificationResponse>> Handle(CreateNotificationCommand request, CancellationToken ct)
    {
        var notification = new Domain.Entities.Notification
        {
            UserId = request.UserId,
            Title = request.Title,
            Message = request.Message,
            Type = request.Type,
            ReferenceId = request.ReferenceId,
            ReferenceType = request.ReferenceType
        };

        await repository.CreateAsync(notification, ct);

        return Result<NotificationResponse>.Success(MapToResponse(notification), 201);
    }

    internal static NotificationResponse MapToResponse(Domain.Entities.Notification n) => new(
        n.Id, n.UserId, n.Title, n.Message, n.Type, n.ReferenceId, n.ReferenceType,
        n.IsRead, n.ReadAt, n.CreatedAt);
}
