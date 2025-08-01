using System;
using NERBABO.ApiService.Core.Teachers.Dtos;

namespace NERBABO.ApiService.Core.Teachers.Cache;

public interface ICacheTeacherRepository
{
    /// <summary>
    /// Removes teacher entries from the cache.
    /// If an ID is provided, it removes the single teacher entry and all list entries.
    /// If no ID is provided, it removes all cache entries associated with teachers.
    /// </summary>
    /// <param name="id">The ID of the teacher to remove. If null, removes all teacher-related cache entries.</param>
    /// <example>
    /// To remove a single teacher and all list caches: RemoveTeacherCacheAsync(1)
    /// To remove all teacher caches: RemoveTeacherCacheAsync()
    /// </example>
    Task RemoveTeacherCacheAsync(long? id = null);

    /// <summary>
    /// Sets a single teacher in the cache.
    /// </summary>
    /// <param name="teacher">The retrieve teacher DTO to cache.</param>
    /// <example>
    /// Cache key: "Teacher:1"
    /// </example>
    Task SetSingleTeacherCacheAsync(RetrieveTeacherDto teacher);

    /// <summary>
    /// Retrieves a single teacher from the cache based on their ID.
    /// </summary>
    /// <param name="id">The ID of the teacher.</param>
    /// <returns>A retrieve teacher DTO, or null if not found.</returns>
    /// <example>
    /// Cache key: "Teacher:1"
    /// </example>
    Task<RetrieveTeacherDto?> GetSingleTeacherCacheAsync(long id);

    /// <summary>
    /// Retrieves all teachers from the cache.
    /// </summary>
    /// <returns>A collection of retrieve teacher DTOs, or null if not found.</returns>
    /// <example>
    /// Cache key: "Teacher:list"
    /// </example>
    Task<IEnumerable<RetrieveTeacherDto>?> GetCacheAllTeacherAsync();

    /// <summary>
    /// Sets all teachers in the cache.
    /// </summary>
    /// <param name="teacher">The collection of retrieve teacher DTOs to cache.</param>
    /// <example>
    /// Cache key: "Teacher:list"
    /// </example>
    Task SetAllTeacherCacheAsync(IEnumerable<RetrieveTeacherDto> teacher);
}
