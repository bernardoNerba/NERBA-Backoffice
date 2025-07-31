using System;
using NERBABO.ApiService.Core.Students.Dtos;

namespace NERBABO.ApiService.Core.Students.Cache;

public interface ICacheStudentsRepository
{
    /// <summary>
    /// Removes student entries from the cache.
    /// If an ID is provided, it removes the single student entry and all list entries.
    /// If no ID is provided, it removes all cache entries associated with students.
    /// </summary>
    /// <param name="id">The ID of the student to remove. If null, removes all student-related cache entries.</param>
    /// <example>
    /// To remove a single student and all list caches: RemoveStudentsCacheAsync(1)
    /// To remove all student caches: RemoveStudentsCacheAsync()
    /// </example>
    Task RemoveStudentsCacheAsync(long? id = null);

    /// <summary>
    /// Sets a single student in the cache.
    /// </summary>
    /// <param name="student">The retrieve student DTO to cache.</param>
    /// <example>
    /// Cache key: "Student:1"
    /// </example>
    Task SetSingleStudentsCacheAsync(RetrieveStudentDto student);

    /// <summary>
    /// Retrieves a single student from the cache based on their ID.
    /// </summary>
    /// <param name="id">The ID of the student.</param>
    /// <returns>A retrieve student DTO, or null if not found.</returns>
    /// <example>
    /// Cache key: "Student:1"
    /// </example>
    Task<RetrieveStudentDto?> GetSingleStudentsCacheAsync(long id);

    /// <summary>
    /// Retrieves all students from the cache.
    /// </summary>
    /// <returns>A collection of retrieve student DTOs, or null if not found.</returns>
    /// <example>
    /// Cache key: "Student:list"
    /// </example>
    Task<IEnumerable<RetrieveStudentDto>?> GetCacheAllStudentsAsync();

    /// <summary>
    /// Sets all students in the cache.
    /// </summary>
    /// <param name="students">The collection of retrieve student DTOs to cache.</param>
    /// <example>
    /// Cache key: "Student:list"
    /// </example>
    Task SetAllStudentsCacheAsync(IEnumerable<RetrieveStudentDto> students);

    /// <summary>
    /// Retrieves all students related to a company from the cache.
    /// </summary>
    /// <param name="companyId">The company Id.</param>
    /// <returns>A collection of retrieve student DTOs, or null if not found.</returns>
    /// <example>
    /// Cache key: "Student:list:Company:1"
    /// </example>
    Task<IEnumerable<RetrieveStudentDto>?> GetStudentsByCompanyCacheAsync(long companyId);

    /// <summary>
    /// Sets all students related to ac ompany in the cache.
    /// </summary>
    /// <param name="companyId">The company Id.</param>
    /// <param name="students">The collection of retrieve student DTOs to cache.</param>
    /// <example>
    /// Cache key: "Student:list:Company:1"
    /// </example>
    Task SetStudentsByCompanyCacheAsync(long companyId, IEnumerable<RetrieveStudentDto> students);
}