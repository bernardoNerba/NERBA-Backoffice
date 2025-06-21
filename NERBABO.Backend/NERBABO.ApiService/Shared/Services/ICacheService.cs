namespace NERBABO.ApiService.Shared.Services;

/// <summary>
/// Provides a Interface for Redis-based caching functionality, including methods to get, set, and remove cached values.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Retrieves a cached value by its key and deserializes it into the specified type.
    /// </summary>
    /// <param name="key">
    /// The cache key used to identify the stored value.
    /// </param>
    /// <returns>
    /// Returns the cached object of type <typeparamref name="T"/>, or null if not found or deserialization fails.
    /// </returns>
    Task<T?> GetAsync<T>(string key);

    /// <summary>
    /// Serializes the given object and stores it in the cache with the specified key.
    /// </summary>
    /// <param name="key">
    /// The cache key under which the value will be stored.
    /// </param>
    /// <param name="value">
    /// The value to be cached.
    /// </param>
    /// <param name="expiry">
    /// Optional expiration time for the cached entry. If null, the entry will persist indefinitely.
    /// </param>
    /// <returns>
    /// Returns a completed task when the cache entry is set, or logs an error if the operation fails.
    /// </returns>
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);

    /// <summary>
    /// Removes a specific cache entry identified by the provided key.
    /// </summary>
    /// <param name="key">
    /// The cache key identifying the entry to be removed.
    /// </param>
    /// <returns>
    /// Returns a completed task once the cache entry is deleted, or logs an error if the operation fails.
    /// </returns>
    Task RemoveAsync(string key);

    /// <summary>
    /// Removes all cache entries that match the specified pattern.
    /// </summary>
    /// <param name="pattern">
    /// The pattern to match cache keys against (e.g., "user:*").
    /// </param>
    /// <returns>
    /// Returns a completed task after all matching entries are removed, or logs an error if the operation fails.
    /// </returns>
    Task RemovePatternAsync(string pattern);
}
