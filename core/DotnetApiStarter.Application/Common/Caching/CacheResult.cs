namespace DotnetApiStarter.Application.Common.Caching;

public sealed record CacheResult<T>(
    T? Value,
    bool IsAvailable);
