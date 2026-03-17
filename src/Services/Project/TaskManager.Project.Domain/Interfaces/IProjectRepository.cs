using TaskManager.Project.Domain.Entities;
using TaskManager.Shared.Application.Interfaces;

namespace TaskManager.Project.Domain.Interfaces;

public interface IProjectRepository : IRepository<Entities.Project>
{
    Task<IReadOnlyList<Entities.Project>> GetByOwnerIdAsync(string ownerId, CancellationToken ct = default);
    Task<IReadOnlyList<Entities.Project>> GetByMemberIdAsync(string userId, CancellationToken ct = default);
}
