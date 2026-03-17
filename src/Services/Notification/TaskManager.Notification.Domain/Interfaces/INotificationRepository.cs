using TaskManager.Notification.Domain.Entities;
using TaskManager.Shared.Application.Interfaces;

namespace TaskManager.Notification.Domain.Interfaces;

public interface INotificationRepository : IRepository<Entities.Notification>
{
    Task<IReadOnlyList<Entities.Notification>> GetByUserIdAsync(string userId, int page, int pageSize, bool? isRead = null, CancellationToken ct = default);
    Task<long> GetUnreadCountAsync(string userId, CancellationToken ct = default);
    Task MarkAsReadAsync(string id, CancellationToken ct = default);
    Task MarkAllAsReadAsync(string userId, CancellationToken ct = default);
}
