using NERBABO.ApiService.Core.People.Dtos;
using NERBABO.ApiService.Core.People.Models;
using NERBABO.ApiService.Shared.Cache;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.People.Cache;

public class CachePeopleRepository(
    ICacheKeyFabric<Person> cacheKeyFabric,
    ICacheService cacheService
) : ICachePeopleRepository
{
    private readonly ICacheKeyFabric<Person> _cacheKeyFabric = cacheKeyFabric;
    private readonly ICacheService _cacheService = cacheService;

    public async Task<IEnumerable<RetrievePersonDto>?> GetCacheAllPeopleAsync()
    {
        return await _cacheService.GetAsync<IEnumerable<RetrievePersonDto>>(
            _cacheKeyFabric.GenerateCacheKeyList());
    }

    public async Task<IEnumerable<RetrievePersonDto>?> GetCachePeopleWithoutProfileAsync(string profile)
    {
        return await _cacheService.GetAsync<IEnumerable<RetrievePersonDto>>(
            _cacheKeyFabric.GenerateCacheKeyList($"without:{profile}")
        );
    }

    public async Task<RetrievePersonDto?> GetSinglePersonCacheAsync(long id)
    {
        return await _cacheService.GetAsync<RetrievePersonDto>(
            _cacheKeyFabric.GenerateCacheKey(id.ToString())
        );
    }

    public async Task RemovePeopleCacheAsync(long? id = null)
    {
        // remove "Person:id" cache entry
        if (id is not null)
            await _cacheService.RemoveAsync(_cacheKeyFabric.GenerateCacheKey($"{id}"));
        
        // remove "Person:list" cache entry
        await _cacheService.RemoveAsync(_cacheKeyFabric.GenerateCacheKeyList());

        // remove "Person:list:without:*" cache entries
        await _cacheService.RemovePatternAsync(_cacheKeyFabric.GenerateCacheKeyList("without:*"));
    }

    public async Task SetAllPeopleCacheAsync(IEnumerable<RetrievePersonDto> people)
    {
        await _cacheService.SetAsync(
            _cacheKeyFabric.GenerateCacheKeyList(),
            people
        );
    }

    public async Task SetPeopleWithoutProfileCacheAsync(IEnumerable<RetrievePersonDto> people, string profile)
    {
        await _cacheService.SetAsync(
            _cacheKeyFabric.GenerateCacheKeyList($"without:{profile}"),
            people
        );
    }

    public async Task SetSinglePersonCacheAsync(RetrievePersonDto person)
    {
        await _cacheService.SetAsync(
            _cacheKeyFabric.GenerateCacheKey(person.Id.ToString()),
            person
        );
    }
}
