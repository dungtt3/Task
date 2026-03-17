using MediatR;
using TaskManager.Notification.Domain.Interfaces;
using TaskManager.Shared.Application.Models;

namespace TaskManager.Notification.Application.Commands.MarkAllAsRead;

public class MarkAllAsReadCommandHandler(
    INotificationRepository repository) : IRequestHandler<MarkAllAsReadCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(MarkAllAsReadCommand request, CancellationToken ct)
    {
        await repository.MarkAllAsReadAsync(request.UserId, ct);
        return Result<bool>.Success(true);
    }
}
