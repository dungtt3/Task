using MongoDB.Driver;
using TaskManager.Auth.Domain.Entities;
using TaskManager.Auth.Domain.Interfaces;
using TaskManager.Shared.Infrastructure.Persistence;

namespace TaskManager.Auth.Infrastructure.Persistence;

public class UserRepository : MongoRepository<User>, IUserRepository
{
    public UserRepository(IMongoDatabase database)
        : base(database, "users")
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await Collection.Find(u => u.Email == email).FirstOrDefaultAsync(ct);
    }

    public async Task<User?> GetByRefreshTokenAsync(string token, CancellationToken ct = default)
    {
        var filter = Builders<User>.Filter.ElemMatch(
            u => u.RefreshTokens,
            rt => rt.Token == token);

        return await Collection.Find(filter).FirstOrDefaultAsync(ct);
    }
}
