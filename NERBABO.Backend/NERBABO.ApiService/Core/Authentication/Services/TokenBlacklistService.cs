using Microsoft.Extensions.Caching.Distributed;
using System.Text;

namespace NERBABO.ApiService.Core.Authentication.Services;

/// <summary>
/// Service for managing blacklisted JWT tokens using Redis distributed cache
/// </summary>
public class TokenBlacklistService : ITokenBlacklistService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<TokenBlacklistService> _logger;
    private const string BlacklistKeyPrefix = "blacklist:token:";

    public TokenBlacklistService(
        IDistributedCache cache,
        ILogger<TokenBlacklistService> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Adds a token to the blacklist
    /// </summary>
    /// <param name="token">The JWT token to blacklist</param>
    /// <param name="expirationTime">When the token expires (for automatic cleanup)</param>
    public async Task BlacklistTokenAsync(string token, DateTime expirationTime)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("Attempted to blacklist null or empty token");
            return;
        }

        try
        {
            var key = $"{BlacklistKeyPrefix}{token}";
            var value = Encoding.UTF8.GetBytes(expirationTime.ToString("o"));

            // Calculate how long to keep the token in the blacklist
            // Keep it until the token would naturally expire
            var timeUntilExpiration = expirationTime - DateTime.UtcNow;

            // Ensure we have a positive timespan
            if (timeUntilExpiration.TotalSeconds < 0)
            {
                _logger.LogInformation("Token already expired, not adding to blacklist");
                return;
            }

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = expirationTime
            };

            await _cache.SetAsync(key, value, options);
            _logger.LogInformation("Token added to blacklist, expires at {ExpirationTime}", expirationTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding token to blacklist");
            throw;
        }
    }

    /// <summary>
    /// Checks if a token is blacklisted
    /// </summary>
    /// <param name="token">The JWT token to check</param>
    /// <returns>True if token is blacklisted, false otherwise</returns>
    public async Task<bool> IsTokenBlacklistedAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        try
        {
            var key = $"{BlacklistKeyPrefix}{token}";
            var value = await _cache.GetAsync(key);

            var isBlacklisted = value != null;

            if (isBlacklisted)
            {
                _logger.LogWarning("Blacklisted token detected");
            }

            return isBlacklisted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking token blacklist status");
            // On error, deny access (fail secure)
            return true;
        }
    }
}
