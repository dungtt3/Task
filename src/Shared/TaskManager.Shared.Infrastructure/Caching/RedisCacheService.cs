using System.Text.Json;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using TaskManager.Shared.Application.Interfaces;
using TaskManager.Shared.Infrastructure.Settings;

namespace TaskManager.Shared.Infrastructure.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _db;
    private readonly IConnectionMultiplexer _redis;
    private readonly TimeSpan _defaultExpiration;

    public RedisCacheService(IConnectionMultiplexer redis, IOptions<RedisSettings> settings)
    {
        _redis = redis;
        _db = redis.GetDatabase();
        _defaultExpiration = TimeSpan.FromSeconds(settings.Value.DefaultExpirationSeconds);
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var value = await _db.StringGetAsync(key);
        return value.HasValue ? JsonSerializer.Deserialize<T>((string)value!) : default;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, json, expiration ?? _defaultExpiration);
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        await _db.KeyDeleteAsync(key);
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        var endpoints = _redis.GetEndPoints();
        var server = _redis.GetServer(endpoints[0]);

        var keys = server.Keys(pattern: $"{prefix}*").ToArray();
        if (keys.Length > 0)
        {
            await _db.KeyDeleteAsync(keys);
        }
    }
}
