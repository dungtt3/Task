using MongoDB.Driver;
using TaskManager.Project.Domain.Interfaces;
using TaskManager.Shared.Infrastructure.Persistence;

namespace TaskManager.Project.Infrastructure.Persistence;

public class ProjectRepository(IMongoDatabase database)
    : MongoRepository<Domain.Entities.Project>(database, "projects"), IProjectRepository
{
    public async Task<IReadOnlyList<Domain.Entities.Project>> GetByOwnerIdAsync(string ownerId, CancellationToken ct = default)
    {
        return await Collection.Find(p => p.OwnerId == ownerId).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Domain.Entities.Project>> GetByMemberIdAsync(string userId, CancellationToken ct = default)
    {
        return await Collection.Find(p => p.Members.Any(m => m.UserId == userId)).ToListAsync(ct);
    }
}
