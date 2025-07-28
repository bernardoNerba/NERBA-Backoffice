using System;
using NERBABO.ApiService.Core.People.Dtos;

namespace NERBABO.ApiService.Core.People.Cache;

public interface ICachePeopleRepository
{
    Task RemovePeopleCacheAsync(long? id = null);

    // Manage single person cache

    /// <summary>
    /// Sets a single person data with the cache key `Person:{id}`
    /// </summary>
    /// <param name="person">The person data to be stored.</param>
    Task SetSinglePersonCacheAsync(RetrievePersonDto person);

    /// <summary>
    /// Gets the single person data associated with the cache key `People:{id}`
    /// </summary>
    /// <param name="id">The id of the desired person.</param>
    Task<RetrievePersonDto?> GetSinglePersonCacheAsync(long id);

    // Manage people cache

    /// <summary>
    /// Gets the data associated with the cache key `People:list` 
    /// </summary>
    Task<IEnumerable<RetrievePersonDto>?> GetCacheAllPeopleAsync();

    /// <summary>
    /// Sets the data of all the people with the cache key `People:list`
    /// </summary>
    /// <param name="people">The data to store on the cache.</param>
    Task SetAllPeopleCacheAsync(IEnumerable<RetrievePersonDto> people);

    // Manage active people cache

    /// <summary>
    /// Gets the data associated with the cache key `People:list:without:{profile}` 
    /// </summary>
    /// <param name="profile">The profile string which the people where filtered on.</param>
    /// <returns>People with a given profile</returns>
    Task<IEnumerable<RetrievePersonDto>?> GetCachePeopleWithoutProfileAsync(string profile);

    /// <summary>
    /// Sets the people data filtered by a given profile on the cache with key `People:list:without:{profile}`
    /// </summary>
    /// <param name="people">The people without the given profile.</param>
    /// <param name="profile">The profile string which the people where filtered on.</param>
    Task SetPeopleWithoutProfileCacheAsync(IEnumerable<RetrievePersonDto> people, string profile);
}
