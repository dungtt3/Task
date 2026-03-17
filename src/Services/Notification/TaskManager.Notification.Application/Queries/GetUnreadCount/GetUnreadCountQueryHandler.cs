using MediatR;
using TaskManager.Notification.Domain.Interfaces;
using TaskManager.Shared.Application.Models;

namespace TaskManager.Notification.Application.Queries.GetUnreadCount;

public class GetUnreadCountQueryHandler(
    INotificationRepository repository) : IRequestHandler<GetUnreadCountQuery, Result<long>>
{
    public async Task<Result<long>> Handle(GetUnreadCountQuery request, CancellationToken ct)
    {
        var count = await repository.GetUnreadCountAsync(request.UserId, ct);
        return Result<long>.Success(count);
    }
}
