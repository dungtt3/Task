using System.Linq.Expressions;
using MongoDB.Driver;
using TaskManager.Shared.Application.Interfaces;
using TaskManager.Shared.Domain.Entities;

namespace TaskManager.Shared.Infrastructure.Persistence;

public class MongoRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly IMongoCollection<T> Collection;

    public MongoRepository(IMongoDatabase database, string collectionName)
    {
        Collection = database.GetCollection<T>(collectionName);
    }

    public async Task<T?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        return await Collection.Find(e => e.Id == id).FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
    {
        return await Collection.Find(_ => true).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
    {
        return await Collection.Find(predicate).ToListAsync(ct);
    }

    public async Task<T> CreateAsync(T entity, CancellationToken ct = default)
    {
        await Collection.InsertOneAsync(entity, cancellationToken: ct);
        return entity;
    }

    public async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        await Collection.ReplaceOneAsync(e => e.Id == entity.Id, entity, cancellationToken: ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        await Collection.DeleteOneAsync(e => e.Id == id, ct);
    }

    public async Task<long> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
    {
        return predicate is null
            ? await Collection.CountDocumentsAsync(_ => true, cancellationToken: ct)
            : await Collection.CountDocumentsAsync(predicate, cancellationToken: ct);
    }
}
