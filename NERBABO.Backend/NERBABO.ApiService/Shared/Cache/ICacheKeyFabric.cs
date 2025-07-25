
namespace NERBABO.ApiService.Shared.Cache;

public interface ICacheKeyFabric<T>
{
    /// <summary>
    /// Generates a cache key based on the provided T type and identifier.
    /// </summary>
    /// <param name="id">The identifier of the object to cache.</param>
    /// <returns>A string representing the cache key. e.g. "Course:1"</returns>
    string GenerateCacheKey(string id);

    /// <summary>
    /// Generates a cache key of a list of objects of the specified T type.
    /// </summary>
    /// <param name="id">The identifier of the one side in a many-to-one relationship.</param>
    /// <param name="one">The T type of the one side in a many-to-one relationship.</param>
    /// <returns>A string representing the cache key for a many-to-one relationship. e.g. "Course:list:Student:1"</returns>
    string GenerateCacheKeyManyToOne(string id, Type one);

    /// <summary>
    /// Generates a cache key for a many-to-one relationship pattern.
    /// </summary>
    /// <param name="one">The T type of the one side in a many-to-one relationship.</param>
    /// <returns>A string representing the cache key pattern for a many-to-one relationship. e.g. "Course:list:Frame:*"</returns>
    string GenerateCacheKeyManyToOnePattern(Type one);


    /// <summary>
    /// Generates a cache key for a list of objects of the specified T type.
    /// </summary>
    /// <returns>A string representing the cache key for a list of objects. e.g. "Course:list"</returns>
    string GenerateCacheKeyList();

    /// <summary>
    /// Generates a cache key for a list of objects of the specified T type with a filter.
    /// </summary>
    /// <param name="filter">The filter to apply to the list.</param>
    /// <returns>A string representing the cache key for a filtered list of objects. e.g. "Course:list:active"</returns>
    string GenerateCacheKeyList(string filter);

}
