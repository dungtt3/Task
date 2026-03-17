using TaskManager.Shared.Application.Interfaces;
using TaskManager.Shared.Domain.Enums;
using TaskManager.Task.Domain.Entities;

namespace TaskManager.Task.Domain.Interfaces;

public interface ITaskItemRepository : IRepository<TaskItem>
{
    Task<IReadOnlyList<TaskItem>> GetByProjectIdAsync(string projectId, CancellationToken ct = default);
    Task<IReadOnlyList<TaskItem>> GetByAssigneeIdAsync(string assigneeId, CancellationToken ct = default);
    Task<IReadOnlyList<TaskItem>> GetByStatusAsync(TaskItemStatus status, CancellationToken ct = default);
}
