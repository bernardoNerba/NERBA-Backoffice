using System;
using NERBABO.ApiService.Core.People.Dtos;

namespace NERBABO.ApiService.Core.People.Cache;

public interface ICachePeopleRepository
{
    /// <summary>
    /// Removes people entries from the cache.
    /// If an ID is provided, it removes the single person entry and all list entries.
    /// If no ID is provided, it removes all cache entries associated with people.
    /// </summary>
    /// <param name="id">The ID of the person to remove. If null, removes all people-related cache entries.</param>
    /// <example>
    /// To remove a single person and all list caches: RemovePeopleCacheAsync(1)
    /// To remove all people caches: RemovePeopleCacheAsync()
    /// </example>
    Task RemovePeopleCacheAsync(long? id = null);

    /// <summary>
    /// Sets a single person in the cache.
    /// </summary>
    /// <param name="person">The retrieve person DTO to cache.</param>
    /// <example>
    /// Cache key: "Person:1"
    /// </example>
    Task SetSinglePersonCacheAsync(RetrievePersonDto person);

    /// <summary>
    /// Retrieves a single person from the cache based on their ID.
    /// </summary>
    /// <param name="id">The ID of the person.</param>
    /// <returns>A retrieve person DTO, or null if not found.</returns>
    /// <example>
    /// Cache key: "Person:1"
    /// </example>
    Task<RetrievePersonDto?> GetSinglePersonCacheAsync(long id);

    /// <summary>
    /// Retrieves all people from the cache.
    /// </summary>
    /// <returns>A collection of retrieve person DTOs, or null if not found.</returns>
    /// <example>
    /// Cache key: "Person:list"
    /// </example>
    Task<IEnumerable<RetrievePersonDto>?> GetCacheAllPeopleAsync();

    /// <summary>
    /// Sets all people in the cache.
    /// </summary>
    /// <param name="people">The collection of retrieve person DTOs to cache.</param>
    /// <example>
    /// Cache key: "Person:list"
    /// </example>
    Task SetAllPeopleCacheAsync(IEnumerable<RetrievePersonDto> people);

    /// <summary>
    /// Retrieves a collection of people from the cache that do not have a specific profile.
    /// </summary>
    /// <param name="profile">The profile to exclude.</param>
    /// <returns>A collection of retrieve person DTOs, or null if not found.</returns>
    /// <example>
    /// Cache key: "Person:list:without:{profile}"
    /// </example>
    Task<IEnumerable<RetrievePersonDto>?> GetCachePeopleWithoutProfileAsync(string profile);

    /// <summary>
    /// Sets a collection of people in the cache that do not have a specific profile.
    /// </summary>
    /// <param name="people">The collection of retrieve person DTOs to cache.</param>
    /// <param name="profile">The profile to exclude.</param>
    /// <example>
    /// Cache key: "Person:list:without:{profile}"
    /// </example>
    Task SetPeopleWithoutProfileCacheAsync(IEnumerable<RetrievePersonDto> people, string profile);
}
