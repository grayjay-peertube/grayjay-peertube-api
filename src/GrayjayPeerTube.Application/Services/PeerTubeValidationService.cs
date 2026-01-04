using GrayjayPeerTube.Application.DTOs;
using GrayjayPeerTube.Domain.Exceptions;
using GrayjayPeerTube.Domain.Interfaces;
using GrayjayPeerTube.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace GrayjayPeerTube.Application.Services;

/// <summary>
/// Service for validating PeerTube instances.
/// </summary>
public class PeerTubeValidationService : IPeerTubeValidationService
{
    private readonly IPeerTubeClient _peerTubeClient;
    private readonly ILogger<PeerTubeValidationService> _logger;

    public PeerTubeValidationService(
        IPeerTubeClient peerTubeClient,
        ILogger<PeerTubeValidationService> logger)
    {
        _peerTubeClient = peerTubeClient;
        _logger = logger;
    }

    public async Task<ValidationResultDto> ValidateInstanceAsync(
        string? peerTubePlatformUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Parse and normalize the URL
            var host = PeerTubeHost.FromUrl(peerTubePlatformUrl);

            _logger.LogDebug("Validating PeerTube instance: {Host}", host.NormalizedHost);

            // Fetch instance config to validate
            await _peerTubeClient.GetInstanceConfigAsync(host, cancellationToken);

            return ValidationResultDto.Success();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Validation failed: {Message}", ex.Message);
            return ValidationResultDto.Failure(ex.Message);
        }
        catch (PeerTubeValidationException ex)
        {
            _logger.LogWarning("Validation failed: {Message}", ex.Message);
            return ValidationResultDto.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during validation");
            return ValidationResultDto.Failure($"Invalid PeerTube instance: {ex.Message}");
        }
    }
}
