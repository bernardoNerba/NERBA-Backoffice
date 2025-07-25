using System;
using NERBABO.ApiService.Core.Courses.Dtos;

namespace NERBABO.ApiService.Core.Courses.Cache;

public interface ICacheCourseRepository
{
    Task RemoveCourseCacheAsync(long? id = null);

    // Manage single course cache
    Task SetSingleCourseCacheAsync(RetrieveCourseDto c);
    Task<RetrieveCourseDto?> GetSingleCourseCacheAsync(long id);

    // Manage active courses cache
    Task<IEnumerable<RetrieveCourseDto>?> GetCacheActiveCoursesAsync();
    Task SetActiveCoursesCacheAsync(IEnumerable<RetrieveCourseDto> c);

    // Manage courses cache
    Task<IEnumerable<RetrieveCourseDto>?> GetCacheAllCoursesAsync();
    Task SetAllCoursesCacheAsync(IEnumerable<RetrieveCourseDto> c);

    // Manage courses with given frame
    Task<IEnumerable<RetrieveCourseDto>?> GetCacheCoursesByFrameAsync(long frameId);
    Task SetCoursesByFrameCacheAsync(long frameId, IEnumerable<RetrieveCourseDto> courses);

    // Manage courses with given module
    Task<IEnumerable<RetrieveCourseDto>?> GetCacheCoursesByModuleAsync(long moduleId);
    Task SetCoursesByModuleCacheAsync(long moduleId, IEnumerable<RetrieveCourseDto> courses);
}
