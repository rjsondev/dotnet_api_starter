using DotnetApiStarter.Application.Common.Caching;
using DotnetApiStarter.Application.Common.Interfaces;

namespace DotnetApiStarter.Application.Common.Extensions;

public static class CacheServiceExtension
{
    public static async Task<CacheResult<T>> GetFromCacheAsync<T>(this ICacheService cache
        , string cacheKey
        , CancellationToken cancellationToken = default)
    {
        var isCacheAvailable = await cache.IsAvailableAsync(cancellationToken);

        return isCacheAvailable
            ? await cache.GetAsync<T>(cacheKey, cancellationToken)
            : new CacheResult<T>(default, false);
    }

    public static async Task<CacheResult<T>> WriteToCacheAsync<T>(this ICacheService cache
        , string cacheKey
        , T value
        , TimeSpan expiration
        , CancellationToken cancellationToken = default)
    {
        var isCacheAvailable = await cache.IsAvailableAsync(cancellationToken);
        if (isCacheAvailable && await cache.SetAsync(cacheKey, value, expiration, cancellationToken))
            return new CacheResult<T>(value, true);

        return new CacheResult<T>(default, false);
    }

    public static async Task<CacheResult<T>> RemoveFromCacheAsync<T>(this ICacheService cache
        , string cacheKey
        , T value
        , CancellationToken cancellationToken = default)
    {
        var isCacheAvailable = await cache.IsAvailableAsync(cancellationToken);
        if (isCacheAvailable && await cache.RemoveAsync(cacheKey, cancellationToken))
            return new CacheResult<T>(default, true);

        return new CacheResult<T>(default, false);
    }
}
