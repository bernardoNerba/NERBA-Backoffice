using System;
using NERBABO.ApiService.Core.Courses.Cache;
using NERBABO.ApiService.Core.Modules.Dtos;
using NERBABO.ApiService.Core.Modules.Models;
using NERBABO.ApiService.Shared.Cache;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Modules.Cache;

public class CacheModuleRepository(
    ICacheKeyFabric<Module> cacheKeyFabric,
    ICacheService cacheService
): ICacheModuleRepository
{
    private readonly ICacheKeyFabric<Module> _cacheKeyFabric = cacheKeyFabric;
    private readonly ICacheService _cacheService = cacheService;

    // Manage modules cache
    public async Task<IEnumerable<RetrieveModuleDto>?> GetCacheAllModulesAsync()
    {
        return await _cacheService.GetAsync<IEnumerable<RetrieveModuleDto>>(
            _cacheKeyFabric.GenerateCacheKeyList());
    }

    public async Task SetAllModulesCacheAsync(IEnumerable<RetrieveModuleDto> m)
    {
        await _cacheService.SetAsync(_cacheKeyFabric.GenerateCacheKeyList(), m);
    }

    // Manage single module cache
    public async Task<RetrieveModuleDto?> GetSingleModuleCacheAsync(long id)
    {
        return await _cacheService.GetAsync<RetrieveModuleDto>(
            _cacheKeyFabric.GenerateCacheKey($"{id}"));
    }

    public async Task SetSingleModuleCacheAsync(RetrieveModuleDto m)
    {
        await _cacheService.SetAsync(_cacheKeyFabric.GenerateCacheKey($"{m.Id}"), m);
    }

    // Manage active modules cache
    public async Task<IEnumerable<RetrieveModuleDto>?> GetCacheActiveModulesAsync()
    {
        return await _cacheService.GetAsync<IEnumerable<RetrieveModuleDto>>(
            _cacheKeyFabric.GenerateCacheKeyList("active"));
    }

    public async Task SetActiveModulesCacheAsync(IEnumerable<RetrieveModuleDto> m)
    {
        await _cacheService.SetAsync(_cacheKeyFabric.GenerateCacheKeyList("active"), m);
    }

    public async Task RemoveModuleCacheAsync(long? id = null)
    {
        if (id is not null)
        {
            await _cacheService.RemoveAsync(_cacheKeyFabric.GenerateCacheKey($"{id}"));

            await _cacheService.RemoveAsync(_cacheKeyFabric.GenerateCacheKeyList());
            await _cacheService.RemoveAsync(_cacheKeyFabric.GenerateCacheKeyList("active"));
        }
        else
        {
            await _cacheService.RemovePatternAsync($"{typeof(Module).Name}:*");
        }
    }

}
