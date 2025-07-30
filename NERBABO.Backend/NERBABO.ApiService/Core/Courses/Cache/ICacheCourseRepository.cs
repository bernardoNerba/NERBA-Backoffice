using System;
using NERBABO.ApiService.Core.Courses.Dtos;

namespace NERBABO.ApiService.Core.Courses.Cache;

public interface ICacheCourseRepository
{
    /// <summary>
    /// Removes course entries from the cache.
    /// </summary>
    /// <param name="id">The ID of the course to remove. If null, removes all related cache entries.</param>
    /// <example>
    /// Cache key for a single course: "Course:1"
    /// Cache key for a list of courses: "Course:list"
    /// Cache key for a list of active courses: "Course:list:active"
    /// Cache key for a list of courses by frame: "Course:list:Frame:1"
    /// Cache key for a list of courses by module: "Course:list:Module:1"
    /// </example>
    Task RemoveCourseCacheAsync(long? id = null);

    /// <summary>
    /// Sets a single course in the cache.
    /// </summary>
    /// <param name="c">The retrieve course DTO to cache.</param>
    /// <example>
    /// Cache key: "Course:1"
    /// </example>
    Task SetSingleCourseCacheAsync(RetrieveCourseDto c);

    /// <summary>
    /// Retrieves a single course from the cache based on its ID.
    /// </summary>
    /// <param name="id">The ID of the course.</param>
    /// <returns>A retrieve course DTO, or null if not found.</returns>
    /// <example>
    /// Cache key: "Course:1"
    /// </example>
    Task<RetrieveCourseDto?> GetSingleCourseCacheAsync(long id);

    /// <summary>
    /// Retrieves a collection of active courses from the cache.
    /// </summary>
    /// <returns>A collection of retrieve course DTOs, or null if not found.</returns>
    /// <example>
    /// Cache key: "Course:list:active"
    /// </example>
    Task<IEnumerable<RetrieveCourseDto>?> GetCacheActiveCoursesAsync();

    /// <summary>
    /// Sets a collection of active courses in the cache.
    /// </summary>
    /// <param name="c">The collection of retrieve course DTOs to cache.</param>
    /// <example>
    /// Cache key: "Course:list:active"
    /// </example>
    Task SetActiveCoursesCacheAsync(IEnumerable<RetrieveCourseDto> c);

    /// <summary>
    /// Retrieves all courses from the cache.
    /// </summary>
    /// <returns>A collection of retrieve course DTOs, or null if not found.</returns>
    /// <example>
    /// Cache key: "Course:list"
    /// </example>
    Task<IEnumerable<RetrieveCourseDto>?> GetCacheAllCoursesAsync();

    /// <summary>
    /// Sets all courses in the cache.
    /// </summary>
    /// <param name="c">The collection of retrieve course DTOs to cache.</param>
    /// <example>
    /// Cache key: "Course:list"
    /// </example>
    Task SetAllCoursesCacheAsync(IEnumerable<RetrieveCourseDto> c);

    /// <summary>
    /// Retrieves a collection of courses from the cache based on the frame ID.
    /// </summary>
    /// <param name="frameId">The ID of the frame.</param>
    /// <returns>A collection of retrieve course DTOs, or null if not found.</returns>
    /// <example>
    /// Cache key: "Course:list:Frame:1"
    /// </example>
    Task<IEnumerable<RetrieveCourseDto>?> GetCacheCoursesByFrameAsync(long frameId);

    /// <summary>
    /// Sets a collection of courses in the cache for a specific frame.
    /// </summary>
    /// <param name="frameId">The ID of the frame.</param>
    /// <param name="courses">The collection of retrieve course DTOs to cache.</param>
    /// <example>
    /// Cache key: "Course:list:Frame:1"
    /// </example>
    Task SetCoursesByFrameCacheAsync(long frameId, IEnumerable<RetrieveCourseDto> courses);

    /// <summary>
    /// Retrieves a collection of courses from the cache based on the module ID.
    /// </summary>
    /// <param name="moduleId">The ID of the module.</param>
    /// <returns>A collection of retrieve course DTOs, or null if not found.</returns>
    /// <example>
    /// Cache key: "Course:list:Module:1"
    /// </example>
    Task<IEnumerable<RetrieveCourseDto>?> GetCacheCoursesByModuleAsync(long moduleId);

    /// <summary>
    /// Sets a collection of courses in the cache for a specific module.
    /// </summary>
    /// <param name="moduleId">The ID of the module.</param>
    /// <param name="courses">The collection of retrieve course DTOs to cache.</param>
    /// <example>
    /// Cache key: "Course:list:Module:1"
    /// </example>
    Task SetCoursesByModuleCacheAsync(long moduleId, IEnumerable<RetrieveCourseDto> courses);
}
