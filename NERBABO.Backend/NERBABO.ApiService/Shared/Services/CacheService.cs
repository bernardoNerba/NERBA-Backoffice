using StackExchange.Redis;
using System.Text.Json;

namespace NERBABO.ApiService.Shared.Services;

/// <summary>
/// Provides Redis-based caching functionality, including methods to get, set, and remove cached values.
/// </summary>
/// <param name="redis">
/// Redis connection multiplexer used to access the Redis database and server.
/// </param>
/// <param name="logger">
/// Logger instance used to log errors or cache operation information.
/// </param>
public class CacheService(
    IConnectionMultiplexer redis,
    ILogger<CacheService> logger)
    : ICacheService
{
    // Internal Redis database and server instance
    private readonly IDatabase _database = redis.GetDatabase();
    private readonly IServer _server = redis.GetServer(redis.GetEndPoints().FirstOrDefault()
        ?? throw new InvalidOperationException("Redis server endpoint not found."));
    private readonly ILogger<CacheService> _logger = logger;

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var value = await _database.StringGetAsync(key);
            if (value.IsNullOrEmpty)
                return default;

            return JsonSerializer.Deserialize<T?>(value!);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to get cache for {key}: {ex.Message}", key, ex.Message);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var serializedValue = JsonSerializer.Serialize(value);
            await _database.StringSetAsync(key, serializedValue, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to set cache for {key}: {ex.Message}", key, ex.Message);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _database.KeyDeleteAsync(key);
        }
        catch (Exception)
        {
            _logger.LogError("Failed to remove cache for key: {key}", key);
        }
    }

    public async Task RemovePatternAsync(string pattern)
    {
        try
        {
            var keys = _server.Keys(pattern: pattern);
            foreach (var key in keys)
            {
                await _database.KeyDeleteAsync(key);
            }
        }
        catch (Exception)
        {
            _logger.LogError("Failed to remove cache for pattern: {pattern}", pattern);
        }
    }
}
