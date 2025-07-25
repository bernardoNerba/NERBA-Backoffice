using System;

namespace NERBABO.ApiService.Shared.Cache;

public class CacheKeyFabirc : ICacheKeyFabric
{
    public string GenerateCacheKey(Type type, string id)
    {
        return $"{type.Name}:{id}";
    }

    public string GenerateCacheKeyList(Type type)
    {
        return $"{type.Name}:list";
    }

    public string GenerateCacheKeyList(Type type, string filter)
    {
        return $"{type.Name}:list:{filter}";
    }

    public string GenerateCacheKeyManyToOne(Type many, string id, Type one)
    {
        return $"{many.Name}:list:{one.Name}:{id}";
    }
}
