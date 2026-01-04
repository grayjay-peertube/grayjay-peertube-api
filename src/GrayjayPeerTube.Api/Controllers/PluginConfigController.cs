using System.ComponentModel.DataAnnotations;
using GrayjayPeerTube.Application.Services;
using GrayjayPeerTube.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace GrayjayPeerTube.Api.Controllers;

[ApiController]
[Route("api/v1")]
public class PluginConfigController : ControllerBase
{
    private readonly IPluginConfigService _pluginConfigService;
    private readonly ILogger<PluginConfigController> _logger;

    public PluginConfigController(
        IPluginConfigService pluginConfigService,
        ILogger<PluginConfigController> logger)
    {
        _pluginConfigService = pluginConfigService;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/v1/PluginConfig.json - Generates a plugin configuration for a PeerTube instance.
    /// Returns HTTP 400 on error (matching original behavior).
    /// </summary>
    [HttpGet("PluginConfig.json")]
    public async Task<IActionResult> GetPluginConfig(
        [FromQuery][MaxLength(2048)] string? peerTubePlatformUrl,
        CancellationToken cancellationToken)
    {
        try
        {
            var protocol = Request.Scheme;
            var hostname = Request.Host.Host;

            // Include port if non-standard
            if (Request.Host.Port.HasValue)
            {
                hostname = $"{hostname}:{Request.Host.Port}";
            }

            var config = await _pluginConfigService.GetPluginConfigAsync(
                peerTubePlatformUrl,
                protocol,
                hostname,
                cancellationToken);

            return Ok(config);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Plugin config generation failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (PeerTubeValidationException ex)
        {
            _logger.LogWarning("Plugin config generation failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (UpstreamConfigException ex)
        {
            _logger.LogError(ex, "Upstream config error: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }
}
