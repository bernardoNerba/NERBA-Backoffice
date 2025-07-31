using System;
using NERBABO.ApiService.Core.Actions.Dtos;

namespace NERBABO.ApiService.Core.Actions.Cache;

public interface ICacheActionRepository
{
    /// <summary>
    /// Removes course action entries from the cache.
    /// If an ID is provided, it removes the single action entry and all list entries.
    /// If no ID is provided, it removes all cache entries associated with actions.
    /// </summary>
    /// <param name="id">The ID of the action to remove. If null, removes all action-related cache entries.</param>
    /// <example>
    /// To remove a single action and all list caches: RemoveActionCacheAsync(1)
    /// To remove all action caches: RemoveActionCacheAsync()
    /// </example>
    Task RemoveActionCacheAsync(long? id = null);

    /// <summary>
    /// Sets a single course action in the cache.
    /// </summary>
    /// <param name="action">The retrieve course action DTO to cache.</param>
    /// <example>
    /// Cache key: "CourseAction:1"
    /// </example>
    Task SetSingleActionCacheAsync(RetrieveCourseActionDto action);

    /// <summary>
    /// Retrieves a single course action from the cache based on its ID.
    /// </summary>
    /// <param name="id">The ID of the course action.</param>
    /// <returns>A retrieve course action DTO, or null if not found.</returns>
    /// <example>
    /// Cache key: "CourseAction:1"
    /// </example>
    Task<RetrieveCourseActionDto?> GetSingleActionCacheAsync(long id);

    /// <summary>
    /// Retrieves all course actions from the cache.
    /// </summary>
    /// <returns>A collection of retrieve course action DTOs, or null if not found.</returns>
    /// <example>
    /// Cache key: "CourseAction:list"
    /// </example>
    Task<IEnumerable<RetrieveCourseActionDto>?> GetCacheAllActionsAsync();

    /// <summary>
    /// Sets all course actions in the cache.
    /// </summary>
    /// <param name="actions">The collection of retrieve course action DTOs to cache.</param>
    /// <example>
    /// Cache key: "CourseAction:list"
    /// </example>
    Task SetAllActionsCacheAsync(IEnumerable<RetrieveCourseActionDto> actions);

    /// <summary>
    /// Retrieves a collection of course actions from the cache based on the course ID.
    /// </summary>
    /// <param name="courseId">The ID of the course.</param>
    /// <returns>A collection of retrieve course action DTOs, or null if not found.</returns>
    /// <example>
    /// Cache key: "CourseAction:list:Course:1"
    /// </example>
    Task<IEnumerable<RetrieveCourseActionDto>?> GetCacheActionsByCourseAsync(long courseId);

    /// <summary>
    /// Sets a collection of course actions in the cache for a specific course.
    /// </summary>
    /// <param name="courseId">The ID of the course.</param>
    /// <param name="actions">The collection of retrieve course action DTOs to cache.</param>
    /// <example>
    /// Cache key: "CourseAction:list:Course:1"
    /// </example>
    Task SetActionsByCourseCacheAsync(long courseId, IEnumerable<RetrieveCourseActionDto> actions);

    /// <summary>
    /// Retrieves a collection of course actions from the cache based on the module ID.
    /// </summary>
    /// <param name="moduleId">The ID of the module.</param>
    /// <returns>A collection of retrieve course action DTOs, or null if not found.</returns>
    /// <example>
    /// Cache key: "CourseAction:list:Module:1"
    /// </example>
    Task<IEnumerable<RetrieveCourseActionDto>?> GetCacheActionsByModuleAsync(long moduleId);

    /// <summary>
    /// Sets a collection of course actions in the cache for a specific module.
    /// </summary>
    /// <param name="moduleId">The ID of the module.</param>
    /// <param name="actions">The collection of retrieve course action DTOs to cache.</param>
    /// <example>
    /// Cache key: "CourseAction:list:Module:1"
    /// </example>
    Task SetActionsByModuleCacheAsync(long moduleId, IEnumerable<RetrieveCourseActionDto> actions);
}
