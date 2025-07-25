namespace NERBABO.ApiService.Shared.Cache;

public class CacheKeyFabirc<T> : ICacheKeyFabric<T>
{
    public string GenerateCacheKey(string id)
    {
        return $"{typeof(T).Name}:{id}";
    }

    public string GenerateCacheKeyList()
    {
        return $"{typeof(T).Name}:list";
    }

    public string GenerateCacheKeyList(string filter)
    {
        return $"{typeof(T).Name}:list:{filter}";
    }

    public string GenerateCacheKeyManyToOne(string id, Type one)
    {
        return $"{typeof(T).Name}:list:{one.Name}:{id}";
    }

    public string GenerateCacheKeyManyToOnePattern(Type one)
    {
        return $"{typeof(T).Name}:list:{one.Name}:*";
    }
}
