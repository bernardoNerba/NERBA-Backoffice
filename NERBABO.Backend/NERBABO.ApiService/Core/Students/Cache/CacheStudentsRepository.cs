using NERBABO.ApiService.Core.Companies.Models;
using NERBABO.ApiService.Core.Students.Dtos;
using NERBABO.ApiService.Core.Students.Models;
using NERBABO.ApiService.Shared.Cache;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Students.Cache;

public class CacheStudentsRepository(
    ICacheKeyFabric<Student> cacheKeyFabric,
    ICacheService cacheService
) : ICacheStudentsRepository
{
    private readonly ICacheKeyFabric<Student> _cacheKeyFabric = cacheKeyFabric;
    private readonly ICacheService _cacheService = cacheService;

    public async Task<IEnumerable<RetrieveStudentDto>?> GetCacheAllStudentsAsync()
    {
        return await _cacheService.GetAsync<IEnumerable<RetrieveStudentDto>>(
            _cacheKeyFabric.GenerateCacheKeyList());
    }

    public async Task<RetrieveStudentDto?> GetSingleStudentsCacheAsync(long id)
    {
        return await _cacheService.GetAsync<RetrieveStudentDto>(
            _cacheKeyFabric.GenerateCacheKey(id.ToString())
        );
    }

    public async Task RemoveStudentsCacheAsync(long? id = null)
    {
        if (id is not null)
        {
            await _cacheService.RemoveAsync(_cacheKeyFabric.GenerateCacheKey($"{id}"));

            await _cacheService.RemoveAsync(_cacheKeyFabric.GenerateCacheKeyList());
            await _cacheService.RemovePatternAsync(_cacheKeyFabric.GenerateCacheKeyManyToOnePattern(typeof(Company)));
        }
        else
        {
            await _cacheService.RemovePatternAsync($"{typeof(Student).Name}:*");
        }
    }

    public async Task SetAllStudentsCacheAsync(IEnumerable<RetrieveStudentDto> students)
    {
        await _cacheService.SetAsync(_cacheKeyFabric.GenerateCacheKeyList(), students);
    }

    public async Task SetSingleStudentsCacheAsync(RetrieveStudentDto student)
    {
        await _cacheService.SetAsync(_cacheKeyFabric.GenerateCacheKey($"{student.Id}"), student);
    }

    public async Task<IEnumerable<RetrieveStudentDto>?> GetStudentsByCompanyCacheAsync(long companyId)
    {
        return await _cacheService.GetAsync<IEnumerable<RetrieveStudentDto>>(
            _cacheKeyFabric.GenerateCacheKeyManyToOne($"{companyId}", typeof(Company)));
    }

    public async Task SetStudentsByCompanyCacheAsync(long companyId, IEnumerable<RetrieveStudentDto> students)
    {
        await _cacheService.SetAsync(
            _cacheKeyFabric.GenerateCacheKeyManyToOne($"{companyId}", typeof(Company)), students);
    }
}
