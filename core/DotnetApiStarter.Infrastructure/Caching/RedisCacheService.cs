using DotnetApiStarter.Application.Common.Caching;
using DotnetApiStarter.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace DotnetApiStarter.Infrastructure.Caching;

public sealed class RedisCacheService(IDistributedCache distributedCache
    , IConnectionMultiplexer connectionMultiplexer
    , ILogger<RedisCacheService> logger) 
    : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<CacheResult<T>> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = await distributedCache.GetStringAsync(key, cancellationToken);

            if (json is null)
                return new CacheResult<T>(default, true);

            var value = JsonSerializer.Deserialize<T>(json, JsonOptions);
            return new CacheResult<T>(value, true);
        }
        catch (Exception ex) when (IsRedisException(ex))
        {
            logger.LogError(ex, "Redis cache read failed for key {CacheKey}", key);
            return new CacheResult<T>(default, false);
        }
    }

    public async Task<bool> SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(value, JsonOptions);

            await distributedCache.SetStringAsync(key, json, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            },
            cancellationToken);

            return true;
        }
        catch (Exception ex) when (IsRedisException(ex))
        {
            logger.LogError(ex, "Redis cache write failed for key {CacheKey}", key);
            return false;
        }
    }

    public async Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await distributedCache.RemoveAsync(key, cancellationToken);
            return true;
        }
        catch (Exception ex) when (IsRedisException(ex))
        {
            logger.LogError(ex, "Redis cache remove failed for key {CacheKey}", key);
            return false;
        }
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await connectionMultiplexer
                .GetDatabase()
                .PingAsync()
                .WaitAsync(cancellationToken);

            return true;
        }
        catch (RedisException ex)
        {
            logger.LogWarning(ex, "Redis is unavailable.");
            return false;
        }
    }

    private static bool IsRedisException(Exception ex)
    {
        return ex is RedisConnectionException
            or RedisTimeoutException;
    }
}
