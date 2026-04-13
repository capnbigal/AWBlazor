using Microsoft.Extensions.Caching.Memory;

namespace AWBlazorApp.Services;

/// <summary>
/// Thin wrapper around <see cref="IMemoryCache"/> for analytics dashboard data.
/// Provides a consistent 5-minute TTL and typed cache keys.
/// </summary>
public sealed class AnalyticsCacheService(IMemoryCache cache)
{
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(5);

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory)
    {
        return (await cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = DefaultTtl;
            return await factory();
        }))!;
    }

    public void Invalidate(string key) => cache.Remove(key);

    public void InvalidateAll(string prefix)
    {
        // IMemoryCache doesn't support prefix-based eviction natively.
        // For a simple implementation, we use a generation counter.
        var genKey = $"__gen_{prefix}";
        var current = cache.Get<int>(genKey);
        cache.Set(genKey, current + 1);
    }

    /// <summary>
    /// Builds a cache key incorporating the generation counter so that
    /// <see cref="InvalidateAll"/> effectively busts all keys with that prefix.
    /// </summary>
    public string Key(string prefix, string suffix)
    {
        var genKey = $"__gen_{prefix}";
        var gen = cache.GetOrCreate(genKey, _ => 0);
        return $"{prefix}:v{gen}:{suffix}";
    }
}
