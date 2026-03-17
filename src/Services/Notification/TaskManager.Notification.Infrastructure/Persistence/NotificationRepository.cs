using MongoDB.Driver;
using TaskManager.Notification.Domain.Interfaces;
using TaskManager.Shared.Infrastructure.Persistence;

namespace TaskManager.Notification.Infrastructure.Persistence;

public class NotificationRepository(IMongoDatabase database)
    : MongoRepository<Domain.Entities.Notification>(database, "notifications"), INotificationRepository
{
    public async Task<IReadOnlyList<Domain.Entities.Notification>> GetByUserIdAsync(
        string userId, int page, int pageSize, bool? isRead = null, CancellationToken ct = default)
    {
        var filter = Builders<Domain.Entities.Notification>.Filter.Eq(n => n.UserId, userId);
        if (isRead.HasValue)
            filter &= Builders<Domain.Entities.Notification>.Filter.Eq(n => n.IsRead, isRead.Value);

        return await Collection
            .Find(filter)
            .SortByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);
    }

    public async Task<long> GetUnreadCountAsync(string userId, CancellationToken ct = default)
    {
        return await Collection.CountDocumentsAsync(
            n => n.UserId == userId && !n.IsRead, cancellationToken: ct);
    }

    public async Task MarkAsReadAsync(string id, CancellationToken ct = default)
    {
        var update = Builders<Domain.Entities.Notification>.Update
            .Set(n => n.IsRead, true)
            .Set(n => n.ReadAt, DateTime.UtcNow)
            .Set(n => n.UpdatedAt, DateTime.UtcNow);
        await Collection.UpdateOneAsync(n => n.Id == id, update, cancellationToken: ct);
    }

    public async Task MarkAllAsReadAsync(string userId, CancellationToken ct = default)
    {
        var update = Builders<Domain.Entities.Notification>.Update
            .Set(n => n.IsRead, true)
            .Set(n => n.ReadAt, DateTime.UtcNow)
            .Set(n => n.UpdatedAt, DateTime.UtcNow);
        await Collection.UpdateManyAsync(
            n => n.UserId == userId && !n.IsRead, update, cancellationToken: ct);
    }
}
