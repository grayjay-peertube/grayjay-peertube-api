using Microsoft.AspNetCore.Mvc;

namespace GrayjayPeerTube.Api.Controllers;

[ApiController]
public class HealthController : ControllerBase
{
    private const string Version = "2.0.0";

    /// <summary>
    /// GET /version - Returns the application version.
    /// </summary>
    [HttpGet("/version")]
    public IActionResult GetVersion()
    {
        return Ok(new { version = Version });
    }
}
