using TaskManager.Auth.Domain.Entities;
using TaskManager.Shared.Application.Interfaces;

namespace TaskManager.Auth.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByRefreshTokenAsync(string token, CancellationToken ct = default);
}
