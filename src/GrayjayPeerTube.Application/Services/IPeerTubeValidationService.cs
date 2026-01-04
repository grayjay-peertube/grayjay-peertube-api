using GrayjayPeerTube.Application.DTOs;

namespace GrayjayPeerTube.Application.Services;

/// <summary>
/// Service for validating PeerTube instances.
/// </summary>
public interface IPeerTubeValidationService
{
    /// <summary>
    /// Validates a PeerTube instance URL.
    /// </summary>
    /// <param name="peerTubePlatformUrl">The PeerTube platform URL to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result indicating success or failure with error message</returns>
    Task<ValidationResultDto> ValidateInstanceAsync(
        string? peerTubePlatformUrl,
        CancellationToken cancellationToken = default);
}
