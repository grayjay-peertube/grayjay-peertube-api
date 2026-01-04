namespace GrayjayPeerTube.Domain.Interfaces;

/// <summary>
/// Cache service interface for storing and retrieving cached data with TTL.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a cached value by key.
    /// </summary>
    /// <typeparam name="T">The type of the cached value</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The cached value, or default if not found</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a value in the cache with the specified TTL.
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="value">The value to cache</param>
    /// <param name="ttl">Time-to-live for the cached entry</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a value from cache, or sets it using the factory if not found.
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="factory">Factory function to create the value if not cached</param>
    /// <param name="ttl">Time-to-live for the cached entry</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The cached or newly created value</returns>
    Task<T> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan ttl,
        CancellationToken cancellationToken = default);
}
