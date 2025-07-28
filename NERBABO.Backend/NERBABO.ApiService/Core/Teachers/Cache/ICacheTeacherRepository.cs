using System;
using NERBABO.ApiService.Core.Teachers.Dtos;

namespace NERBABO.ApiService.Core.Teachers.Cache;

public interface ICacheTeacherRepository
{
    Task RemoveTeacherCacheAsync(long? id = null);

    // Manage single Teacher cache

    /// <summary>
    /// Sets a single Teacher data with the cache key `Teacher:{id}`
    /// </summary>
    /// <param name="teacher">The Teacher data to be stored.</param>
    Task SetSingleTeacherCacheAsync(RetrieveTeacherDto teacher);

    /// <summary>
    /// Gets the single Teacher data associated with the cache key `People:{id}`
    /// </summary>
    /// <param name="id">The id of the desired Teacher.</param>
    Task<RetrieveTeacherDto?> GetSingleTeacherCacheAsync(long id);

    // Manage list of Teacher cache

    /// <summary>
    /// Gets the data associated with the cache key `Teacher:list` 
    /// </summary>
    Task<IEnumerable<RetrieveTeacherDto>?> GetCacheAllTeacherAsync();

    /// <summary>
    /// Sets the data of all the Teacher with the cache key `Teacher:list`
    /// </summary>
    /// <param name="teacher">The data to store on the cache.</param>
    Task SetAllTeacherCacheAsync(IEnumerable<RetrieveTeacherDto> teacher);
}
