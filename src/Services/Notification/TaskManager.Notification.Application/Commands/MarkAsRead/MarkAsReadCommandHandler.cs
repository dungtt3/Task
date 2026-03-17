using MediatR;
using TaskManager.Notification.Domain.Interfaces;
using TaskManager.Shared.Application.Models;

namespace TaskManager.Notification.Application.Commands.MarkAsRead;

public class MarkAsReadCommandHandler(
    INotificationRepository repository) : IRequestHandler<MarkAsReadCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(MarkAsReadCommand request, CancellationToken ct)
    {
        var notification = await repository.GetByIdAsync(request.Id, ct);
        if (notification is null)
            return Result<bool>.Failure("Notification not found", 404);

        await repository.MarkAsReadAsync(request.Id, ct);
        return Result<bool>.Success(true);
    }
}
