using MongoDB.Driver;
using TaskManager.Shared.Domain.Enums;
using TaskManager.Shared.Infrastructure.Persistence;
using TaskManager.Task.Domain.Entities;
using TaskManager.Task.Domain.Interfaces;

namespace TaskManager.Task.Infrastructure.Persistence;

public class TaskItemRepository(IMongoDatabase database)
    : MongoRepository<TaskItem>(database, "tasks"), ITaskItemRepository
{
    public async Task<IReadOnlyList<TaskItem>> GetByProjectIdAsync(string projectId, CancellationToken ct = default)
    {
        return await Collection.Find(t => t.ProjectId == projectId).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TaskItem>> GetByAssigneeIdAsync(string assigneeId, CancellationToken ct = default)
    {
        return await Collection.Find(t => t.AssigneeId == assigneeId).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TaskItem>> GetByStatusAsync(TaskItemStatus status, CancellationToken ct = default)
    {
        return await Collection.Find(t => t.Status == status).ToListAsync(ct);
    }
}
