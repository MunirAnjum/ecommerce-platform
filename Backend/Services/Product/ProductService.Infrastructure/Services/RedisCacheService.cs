using Microsoft.Extensions.Logging;
using ProductService.Application.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace ProductService.Infrastructure.Services;

public class RedisCacheService : IRedisCacheService
{
    private readonly IDatabase _database;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(
        IConnectionMultiplexer redis,
        ILogger<RedisCacheService> logger)
    {
        _database = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var value = await _database.StringGetAsync(key);

            if (value.IsNullOrEmpty)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(value!);
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(
                ex,
                "Redis GET failed for key {Key}. Continuing without cache.",
                key);

            return default;
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiry = null)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);

            var expiration = expiry.HasValue
                ? new Expiration(expiry.Value)
                : Expiration.Default;

            await _database.StringSetAsync(
                key,
                json,
                expiration);
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(
                ex,
                "Redis SET failed for key {Key}. Continuing without cache.",
                key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _database.KeyDeleteAsync(key);
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(
                ex,
                "Redis DELETE failed for key {Key}. Continuing without cache.",
                key);
        }
    }
}