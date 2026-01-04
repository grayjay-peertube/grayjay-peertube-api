using GrayjayPeerTube.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace GrayjayPeerTube.Infrastructure.Caching;

/// <summary>
/// Memory cache service implementation using IMemoryCache.
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemoryCacheService> _logger;

    public MemoryCacheService(IMemoryCache cache, ILogger<MemoryCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var value = _cache.Get<T>(key);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        };
        _cache.Set(key, value, options);
        return Task.CompletedTask;
    }

    public async Task<T> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue<T>(key, out var cachedValue) && cachedValue is not null)
        {
            _logger.LogDebug("Cache hit for key: {Key}", key);
            return cachedValue;
        }

        _logger.LogDebug("Cache miss for key: {Key}, fetching from source", key);
        var value = await factory();

        await SetAsync(key, value, ttl, cancellationToken);
        return value;
    }
}
