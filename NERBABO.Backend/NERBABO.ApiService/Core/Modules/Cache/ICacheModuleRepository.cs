using System;
using NERBABO.ApiService.Core.Modules.Dtos;

namespace NERBABO.ApiService.Core.Modules.Cache;

public interface ICacheModuleRepository
{
    /// <summary>
    /// Removes module entries from the cache.
    /// </summary>
    /// <param name="id">The ID of the module to remove. If null, removes all related cache entries.</param>
    /// <example>
    /// Cache key for a single module: "Module:1"
    /// Cache key for a list of modules: "Module:list"
    /// Cache key for a list of active modules: "Module:list:active"
    /// </example>
    Task RemoveModuleCacheAsync(long? id = null);

    /// <summary>
    /// Sets a single module in the cache.
    /// </summary>
    /// <param name="m">The retrieve module DTO to cache.</param>
    /// <example>
    /// Cache key: "Module:1"
    /// </example>
    Task SetSingleModuleCacheAsync(RetrieveModuleDto m);

    /// <summary>
    /// Retrieves a single module from the cache based on its ID.
    /// </summary>
    /// <param name="id">The ID of the module.</param>
    /// <returns>A retrieve module DTO, or null if not found.</returns>
    /// <example>
    /// Cache key: "Module:1"
    /// </example>
    Task<RetrieveModuleDto?> GetSingleModuleCacheAsync(long id);

    /// <summary>
    /// Retrieves all modules from the cache.
    /// </summary>
    /// <returns>A collection of retrieve module DTOs, or null if not found.</returns>
    /// <example>
    /// Cache key: "Module:list"
    /// </example>
    Task<IEnumerable<RetrieveModuleDto>?> GetCacheAllModulesAsync();

    /// <summary>
    /// Sets all modules in the cache.
    /// </summary>
    /// <param name="m">The collection of retrieve module DTOs to cache.</param>
    /// <example>
    /// Cache key: "Module:list"
    /// </example>
    Task SetAllModulesCacheAsync(IEnumerable<RetrieveModuleDto> m);

    /// <summary>
    /// Retrieves a collection of active modules from the cache.
    /// </summary>
    /// <returns>A collection of retrieve module DTOs, or null if not found.</returns>
    /// <example>
    /// Cache key: "Module:list:active"
    /// </example>
    Task<IEnumerable<RetrieveModuleDto>?> GetCacheActiveModulesAsync();

    /// <summary>
    /// Sets a collection of active modules in the cache.
    /// </summary>
    /// <param name="m">The collection of retrieve module DTOs to cache.</param>
    /// <example>
    /// Cache key: "Module:list:active"
    /// </example>
    Task SetActiveModulesCacheAsync(IEnumerable<RetrieveModuleDto> m);

}
