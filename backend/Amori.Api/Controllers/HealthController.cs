using Amori.Api.Common.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Amori.Api.Controllers;

[ApiController]
[Route("api/health")]
[AllowAnonymous]
public sealed class HealthController : ControllerBase
{
    /// <summary>
    /// Returns a healthy status to confirm the API is running.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<HealthResponse>), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        var data = new HealthResponse(
            Status: "healthy",
            Timestamp: DateTime.UtcNow,
            Version: "1.0.0"
        );

        return Ok(ApiResponse<HealthResponse>.Ok(data));
    }

    public sealed record HealthResponse(
        string Status,
        DateTime Timestamp,
        string Version
    );
}
