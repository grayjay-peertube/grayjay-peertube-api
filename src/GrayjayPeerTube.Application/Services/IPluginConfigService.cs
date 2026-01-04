using GrayjayPeerTube.Application.DTOs;

namespace GrayjayPeerTube.Application.Services;

/// <summary>
/// Service for generating plugin configurations.
/// </summary>
public interface IPluginConfigService
{
    /// <summary>
    /// Generates a plugin configuration for a PeerTube instance.
    /// </summary>
    /// <param name="peerTubePlatformUrl">The PeerTube platform URL</param>
    /// <param name="requestProtocol">The protocol of the incoming request (http/https)</param>
    /// <param name="requestHostname">The hostname of the incoming request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The generated plugin configuration</returns>
    Task<PluginConfigDto> GetPluginConfigAsync(
        string? peerTubePlatformUrl,
        string requestProtocol,
        string requestHostname,
        CancellationToken cancellationToken = default);
}
