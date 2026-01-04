using System.ComponentModel.DataAnnotations;
using GrayjayPeerTube.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace GrayjayPeerTube.Api.Controllers;

[ApiController]
[Route("api/v1")]
public class ValidationController : ControllerBase
{
    private readonly IPeerTubeValidationService _validationService;

    public ValidationController(IPeerTubeValidationService validationService)
    {
        _validationService = validationService;
    }

    /// <summary>
    /// GET /api/v1/validatePeerTube - Validates a PeerTube instance.
    /// Always returns HTTP 200 with valid: true/false (matching original behavior).
    /// </summary>
    [HttpGet("validatePeerTube")]
    public async Task<IActionResult> ValidatePeerTube(
        [FromQuery][MaxLength(2048)] string? peerTubePlatformUrl,
        CancellationToken cancellationToken)
    {
        var result = await _validationService.ValidateInstanceAsync(
            peerTubePlatformUrl,
            cancellationToken);

        // Always return 200 OK (matching Node.js behavior)
        return Ok(result);
    }
}
