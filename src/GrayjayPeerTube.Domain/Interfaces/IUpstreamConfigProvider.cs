using GrayjayPeerTube.Domain.Entities;

namespace GrayjayPeerTube.Domain.Interfaces;

/// <summary>
/// Provider for fetching upstream plugin configuration from plugins.grayjay.app.
/// </summary>
public interface IUpstreamConfigProvider
{
    /// <summary>
    /// Gets the upstream plugin configuration with caching.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The upstream plugin configuration</returns>
    Task<UpstreamPluginConfig> GetUpstreamConfigAsync(
        CancellationToken cancellationToken = default);
}
