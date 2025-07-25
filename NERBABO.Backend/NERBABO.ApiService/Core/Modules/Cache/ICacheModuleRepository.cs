using System;
using NERBABO.ApiService.Core.Modules.Dtos;

namespace NERBABO.ApiService.Core.Modules.Cache;

public interface ICacheModuleRepository
{
    Task RemoveModuleCacheAsync(long? id = null);

    // Manage single module cache
    Task SetSingleModuleCacheAsync(RetrieveModuleDto m);
    Task<RetrieveModuleDto?> GetSingleModuleCacheAsync(long id);

    // Manage modules cache
    Task<IEnumerable<RetrieveModuleDto>?> GetCacheAllModulesAsync();
    Task SetAllModulesCacheAsync(IEnumerable<RetrieveModuleDto> m);

    // Manage active modules cache
    Task<IEnumerable<RetrieveModuleDto>?> GetCacheActiveModulesAsync();
    Task SetActiveModulesCacheAsync(IEnumerable<RetrieveModuleDto> m);

}
