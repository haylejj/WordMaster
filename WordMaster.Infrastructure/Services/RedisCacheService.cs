using StackExchange.Redis;
using System.Text.Json;
using WordMaster.Application.Services.Abstract;

namespace WordMaster.Infrastructure.Services;

public class RedisCacheService(IConnectionMultiplexer redis) : ICacheService
{
    private readonly IDatabase _database = redis.GetDatabase();
    public static readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _database.StringGetAsync(key);

        if (!value.HasValue)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(value!, jsonSerializerOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var serializedValue = JsonSerializer.Serialize(value, jsonSerializerOptions);

        if (expiration.HasValue)
        {
            await _database.StringSetAsync(key, serializedValue, expiration);
        }
        else
        {
            await _database.StringSetAsync(key, serializedValue);
        }
    }

    public async Task<bool> RemoveAsync(string key)
    {
        return await _database.KeyDeleteAsync(key);
    }

    public async Task<bool> ExistsAsync(string key)
    {
        return await _database.KeyExistsAsync(key);
    }

    public async Task ClearAsync()
    {
        var server = redis.GetServer(redis.GetEndPoints().First());
        await foreach (var key in server.KeysAsync())
        {
            await _database.KeyDeleteAsync(key);
        }
    }
}

