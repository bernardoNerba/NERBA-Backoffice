using System;
using NERBABO.ApiService.Core.Actions.Dtos;

namespace NERBABO.ApiService.Core.Actions.Cache;

public interface ICacheActionRepository
{
    Task RemoveActionCacheAsync(long? id = null);

    // Manage single action cache
    Task SetSingleActionCacheAsync(RetrieveCourseActionDto action);
    Task<RetrieveCourseActionDto?> GetSingleActionCacheAsync(long id);

    // Manage actions cache
    Task<IEnumerable<RetrieveCourseActionDto>?> GetCacheAllActionsAsync();
    Task SetAllActionsCacheAsync(IEnumerable<RetrieveCourseActionDto> actions);

    // Manage actions by course cache
    Task<IEnumerable<RetrieveCourseActionDto>?> GetCacheActionsByCourseAsync(long courseId);
    Task SetActionsByCourseCacheAsync(long courseId, IEnumerable<RetrieveCourseActionDto> actions);

    // Manage actions by module cache
    Task<IEnumerable<RetrieveCourseActionDto>?> GetCacheActionsByModuleAsync(long moduleId);
    Task SetActionsByModuleCacheAsync(long moduleId, IEnumerable<RetrieveCourseActionDto> actions);
}
