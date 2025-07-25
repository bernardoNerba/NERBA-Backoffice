using System;
using System.Reflection;
using NERBABO.ApiService.Core.Courses.Dtos;
using NERBABO.ApiService.Core.Courses.Models;
using NERBABO.ApiService.Core.Frames.Models;
using NERBABO.ApiService.Shared.Cache;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Courses.Cache;

public class CacheCourseRepository(
    ICacheKeyFabric<Course> cacheKeyFabric,
    ICacheService cacheService
) : ICacheCourseRepository
{
    private readonly ICacheKeyFabric<Course> _cacheKeyFabric = cacheKeyFabric;
    private readonly ICacheService _cacheService = cacheService;

    // Manage active courses cache
    public async Task<IEnumerable<RetrieveCourseDto>?> GetCacheActiveCoursesAsync()
    {
        return await _cacheService.GetAsync<IEnumerable<RetrieveCourseDto>>(
            _cacheKeyFabric.GenerateCacheKeyList("active"));
    }

    public async Task SetActiveCoursesCacheAsync(IEnumerable<RetrieveCourseDto> c)
    {
        await _cacheService.SetAsync(_cacheKeyFabric.GenerateCacheKeyList("active"), c);
    }

    // Manage single course cache
    public async Task SetSingleCourseCacheAsync(RetrieveCourseDto c)
    {
        await _cacheService.SetAsync(_cacheKeyFabric.GenerateCacheKey($"{c.Id}"), c);
    }

    public async Task<RetrieveCourseDto?> GetSingleCourseCacheAsync(long id)
    {
        return await _cacheService.GetAsync<RetrieveCourseDto>(
            _cacheKeyFabric.GenerateCacheKey($"{id}"));
    }

    // Manage all courses cache
    public Task<IEnumerable<RetrieveCourseDto>?> GetCacheAllCoursesAsync()
    {
        return _cacheService.GetAsync<IEnumerable<RetrieveCourseDto>>(
            _cacheKeyFabric.GenerateCacheKeyList());
    }

    public async Task SetAllCoursesCacheAsync(IEnumerable<RetrieveCourseDto> c)
    {
        await _cacheService.SetAsync(_cacheKeyFabric.GenerateCacheKeyList(), c);
    }

    // Manage courses with given frame cache
    public Task<IEnumerable<RetrieveCourseDto>?> GetCacheCoursesByFrameAsync(long frameId)
    {
        return _cacheService.GetAsync<IEnumerable<RetrieveCourseDto>>(
            _cacheKeyFabric.GenerateCacheKeyManyToOne($"{frameId}", typeof(Frame)));
    }

    public Task SetCoursesByFrameCacheAsync(long frameId, IEnumerable<RetrieveCourseDto> courses)
    {
        return _cacheService.SetAsync(
            _cacheKeyFabric.GenerateCacheKeyManyToOne($"{frameId}", typeof(Frame)), courses);
    }

    public Task<IEnumerable<RetrieveCourseDto>?> GetCacheCoursesByModuleAsync(long moduleId)
    {
        return _cacheService.GetAsync<IEnumerable<RetrieveCourseDto>>(
            _cacheKeyFabric.GenerateCacheKeyManyToOne($"{moduleId}", typeof(Module)));
    }

    public Task SetCoursesByModuleCacheAsync(long moduleId, IEnumerable<RetrieveCourseDto> courses)
    {
        return _cacheService.SetAsync(
            _cacheKeyFabric.GenerateCacheKeyManyToOne($"{moduleId}", typeof(Module)), courses);
    }

    public async Task RemoveCourseCacheAsync(long? id = null)
    {
        // remove "Course:id" cache entry
        if (id is not null)
            await _cacheService.RemoveAsync(_cacheKeyFabric.GenerateCacheKey($"{id}"));

        // remove "Course:list" cache entry
        await _cacheService.RemoveAsync(_cacheKeyFabric.GenerateCacheKeyList());

        // remove "Course:list:active" cache entry
        await _cacheService.RemoveAsync(_cacheKeyFabric.GenerateCacheKeyList("active"));

        // remove patern "Course:list:Frame:*"
        await _cacheService.RemovePatternAsync(_cacheKeyFabric.GenerateCacheKeyManyToOnePattern(typeof(Frame)));

        // remove patern "Course:list:Module:*"
        await _cacheService.RemovePatternAsync(_cacheKeyFabric.GenerateCacheKeyManyToOnePattern(typeof(Module)));
    }

}
