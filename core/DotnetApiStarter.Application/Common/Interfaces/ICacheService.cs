using DotnetApiStarter.Application.Common.Caching;

namespace DotnetApiStarter.Application.Common.Interfaces;

public interface ICacheService
{
    Task<CacheResult<T>> GetAsync<T>(
        string key,
        CancellationToken cancellationToken = default);

    Task<bool> SetAsync<T>(
        string key,
        T value,
        TimeSpan expiration,
        CancellationToken cancellationToken = default);

    Task<bool> RemoveAsync(
        string key,
        CancellationToken cancellationToken = default);

    Task<bool> IsAvailableAsync(
        CancellationToken cancellationToken = default);
}
