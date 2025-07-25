using System;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Shared.Cache;

public class CacheKeyFabirc<T> : ICacheKeyFabric<T>
{
    public string GenerateCacheKey(string id)
    {
        return $"{typeof(T)}:{id}";
    }

    public string GenerateCacheKeyList()
    {
        return $"{typeof(T)}:list";
    }

    public string GenerateCacheKeyList(string filter)
    {
        return $"{typeof(T)}:list:{filter}";
    }

    public string GenerateCacheKeyManyToOne(string id, Type one)
    {
        return $"{typeof(T)}:list:{one.Name}:{id}";
    }

    public string GenerateCacheKeyManyToOnePattern(Type one)
    {
        return $"{typeof(T)}:list:{one.Name}:*";
    }
}
