using NERBABO.ApiService.Core.Actions.Dtos;
using NERBABO.ApiService.Core.Actions.Models;
using NERBABO.ApiService.Core.Courses.Models;
using NERBABO.ApiService.Core.Modules.Models;
using NERBABO.ApiService.Shared.Cache;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Actions.Cache;

public class CacheActionRepository(
    ICacheKeyFabric<CourseAction> cacheKeyFabric,
    ICacheService cacheService
) : ICacheActionRepository
{
    private readonly ICacheKeyFabric<CourseAction> _cacheKeyFabric = cacheKeyFabric;
    private readonly ICacheService _cacheService = cacheService;

    public async Task<IEnumerable<RetrieveCourseActionDto>?> GetCacheActionsByCourseAsync(long courseId)
    {
        return await _cacheService.GetAsync<IEnumerable<RetrieveCourseActionDto>>(
            _cacheKeyFabric.GenerateCacheKeyManyToOne(courseId.ToString(), typeof(CourseAction)));
    }

    public async Task<IEnumerable<RetrieveCourseActionDto>?> GetCacheActionsByModuleAsync(long moduleId)
    {
        return await _cacheService.GetAsync<IEnumerable<RetrieveCourseActionDto>>(
            _cacheKeyFabric.GenerateCacheKeyManyToOne(moduleId.ToString(), typeof(CourseAction)));
    }

    public async Task<IEnumerable<RetrieveCourseActionDto>?> GetCacheAllActionsAsync()
    {
        return await _cacheService.GetAsync<IEnumerable<RetrieveCourseActionDto>>(
            _cacheKeyFabric.GenerateCacheKeyList());
    }

    public async Task<RetrieveCourseActionDto?> GetSingleActionCacheAsync(long id)
    {
        return await _cacheService.GetAsync<RetrieveCourseActionDto>(
            _cacheKeyFabric.GenerateCacheKey(id.ToString()));
    }

    public async Task RemoveActionCacheAsync(long? id = null)
    {
        if (id is not null)
        {
            // remove "ActionCourse:id" cache entry
            await _cacheService.RemoveAsync(
                _cacheKeyFabric.GenerateCacheKey($"{id}"));

            // remove "ActionCourse:list" cache entry
            await _cacheService.RemoveAsync(_cacheKeyFabric.GenerateCacheKeyList());

            // remove "ActionCourse:list:Module:*" cache entries
            await _cacheService.RemovePatternAsync(
                _cacheKeyFabric.GenerateCacheKeyManyToOnePattern(typeof(Module)));

            // remove "ActionCourse:list:Course:*" cache entries
            await _cacheService.RemovePatternAsync(
                _cacheKeyFabric.GenerateCacheKeyManyToOnePattern(typeof(Course)));
        }
        else // remove all action related cache
        {
            await _cacheService.RemovePatternAsync($"{typeof(CourseAction).Name}:*");
        }
    }

    public async Task SetActionsByCourseCacheAsync(long courseId, IEnumerable<RetrieveCourseActionDto> actions)
    {
        await _cacheService.SetAsync(
            _cacheKeyFabric.GenerateCacheKeyManyToOne(courseId.ToString(), typeof(CourseAction)),
            actions);
    }

    public async Task SetActionsByModuleCacheAsync(long moduleId, IEnumerable<RetrieveCourseActionDto> actions)
    {
        await _cacheService.SetAsync(
            _cacheKeyFabric.GenerateCacheKeyManyToOne(moduleId.ToString(), typeof(CourseAction)),
            actions);
    }

    public async Task SetAllActionsCacheAsync(IEnumerable<RetrieveCourseActionDto> actions)
    {
        await _cacheService.SetAsync(
            _cacheKeyFabric.GenerateCacheKeyList(),
            actions);
    }

    public async Task SetSingleActionCacheAsync(RetrieveCourseActionDto action)
    {
        await _cacheService.SetAsync(
            _cacheKeyFabric.GenerateCacheKey(action.Id.ToString()),
            action);
    }
}
