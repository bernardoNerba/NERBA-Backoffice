using System;

namespace NERBABO.ApiService.Shared.Cache;

public interface ICacheKeyFabric
{
    /// <summary>
    /// Generates a cache key based on the provided type and identifier.
    /// </summary>
    /// <param name="type">The type of the object to cache.</param>
    /// <param name="id">The identifier of the object to cache.</param>
    /// <returns>A string representing the cache key. e.g. "Course:1"</returns>
    string GenerateCacheKey(Type type, string id);

    /// <summary>
    /// Generates a cache key of a list of objects of the specified type.
    /// </summary>
    /// <param name="many">The type of the many side in a many-to-one realationship.</param>
    /// <param name="id">The identifier of the one side in a many-to-one relationship.</param>
    /// <param name="one">The type of the one side in a many-to-one relationship.</param>
    /// <returns>A string representing the cache key for a many-to-one relationship. e.g. "Course:list:Student:1"</returns>
    string GenerateCacheKeyManyToOne(Type many, string id, Type one);

    /// <summary>
    /// Generates a cache key for a list of objects of the specified type.
    /// </summary>
    /// <param name="type">The type of the objects in the list.</param>
    /// <returns>A string representing the cache key for a list of objects. e.g. "Course:list"</returns>
    string GenerateCacheKeyList(Type type);

    /// <summary>
    /// Generates a cache key for a list of objects of the specified type with a filter.
    /// </summary>
    /// <param name="type">The type of the objects in the list.</param>
    /// <param name="filter">The filter to apply to the list.</param>
    /// <returns>A string representing the cache key for a filtered list of objects. e.g. "Course:list:active"</returns>
    string GenerateCacheKeyList(Type type, string filter);

}
