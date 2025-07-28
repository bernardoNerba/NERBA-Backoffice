using System;
using NERBABO.ApiService.Core.Teachers.Dtos;
using NERBABO.ApiService.Core.Teachers.Models;
using NERBABO.ApiService.Shared.Cache;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Teachers.Cache;

public class CacheTeacherRepository(
        ICacheKeyFabric<Teacher> cacheKeyFabric,
    ICacheService cacheService
) : ICacheTeacherRepository
{
    private readonly ICacheKeyFabric<Teacher> _cacheKeyFabric = cacheKeyFabric;
    private readonly ICacheService _cacheService = cacheService;

    public async Task<IEnumerable<RetrieveTeacherDto>?> GetCacheAllTeacherAsync()
    {
        return await _cacheService.GetAsync<IEnumerable<RetrieveTeacherDto>>(
            _cacheKeyFabric.GenerateCacheKeyList());
    }

    public async Task<RetrieveTeacherDto?> GetSingleTeacherCacheAsync(long id)
    {
        return await _cacheService.GetAsync<RetrieveTeacherDto>(
            _cacheKeyFabric.GenerateCacheKey(id.ToString())
        );
    }

    public async Task RemoveTeacherCacheAsync(long? id = null)
    {
        // remove "Teacher:id" cache entry
        if (id is not null)
            await _cacheService.RemoveAsync(
                _cacheKeyFabric.GenerateCacheKey($"{id}")
            );

        // remove "Teacher:list" cache entry
        await _cacheService.RemoveAsync(
            _cacheKeyFabric.GenerateCacheKeyList());
    }

    public async Task SetAllTeacherCacheAsync(IEnumerable<RetrieveTeacherDto> teacher)
    {
        await _cacheService.SetAsync(
            _cacheKeyFabric.GenerateCacheKeyList(),
            teacher
        );
    }

    public async Task SetSingleTeacherCacheAsync(RetrieveTeacherDto teacher)
    {
        await _cacheService.SetAsync(
            _cacheKeyFabric.GenerateCacheKey(teacher.Id.ToString()),
            teacher
        );
    }
}
