using Microsoft.Extensions.Caching.Memory;

namespace CompressionForce.Services;

public class PlcMemoryCache
{
    private readonly IMemoryCache _cache;

    public PlcMemoryCache(IMemoryCache cache)
    {
        _cache = cache;
    }

    public void Set(string key, object value)
    {
        _cache.Set(key, value);
    }

    // NON-GENERIC (optional, keep if already used)
    public object? Get(string key)
    {
        _cache.TryGetValue(key, out object? value);
        return value;
    }
    public bool TryGet<T>(string key, out T value)
    {
        if (_cache.TryGetValue(key, out var obj) && obj is T typed)
        {
            value = typed;
            return true;
        }

        value = default!;
        return false;
    }


    // ✅ GENERIC VERSION (THIS FIXES YOUR ERROR)
    public T? Get<T>(string key)
    {
        if (_cache.TryGetValue(key, out var value))
        {
            if (value is T typed)
                return typed;

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return default;
            }
        }
        return default;
    }
}
