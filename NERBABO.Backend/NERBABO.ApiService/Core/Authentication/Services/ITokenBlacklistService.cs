namespace NERBABO.ApiService.Core.Authentication.Services;

/// <summary>
/// Service for managing blacklisted JWT tokens (invalidated tokens)
/// </summary>
public interface ITokenBlacklistService
{
    /// <summary>
    /// Adds a token to the blacklist
    /// </summary>
    /// <param name="token">The JWT token to blacklist</param>
    /// <param name="expirationTime">When the token expires (for automatic cleanup)</param>
    /// <returns>Task representing the async operation</returns>
    Task BlacklistTokenAsync(string token, DateTime expirationTime);

    /// <summary>
    /// Checks if a token is blacklisted
    /// </summary>
    /// <param name="token">The JWT token to check</param>
    /// <returns>True if token is blacklisted, false otherwise</returns>
    Task<bool> IsTokenBlacklistedAsync(string token);
}
