using GrayjayPeerTube.Domain.Entities;
using GrayjayPeerTube.Domain.ValueObjects;

namespace GrayjayPeerTube.Domain.Interfaces;

/// <summary>
/// Client for interacting with PeerTube instances.
/// </summary>
public interface IPeerTubeClient
{
    /// <summary>
    /// Gets the configuration from a PeerTube instance.
    /// </summary>
    /// <param name="host">The normalized PeerTube host</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The instance configuration</returns>
    Task<PeerTubeInstanceConfig> GetInstanceConfigAsync(
        PeerTubeHost host,
        CancellationToken cancellationToken = default);
}
