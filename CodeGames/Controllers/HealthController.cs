using CodeGames.DTOs;
using CodeGames.Services;
using Microsoft.AspNetCore.Mvc;

namespace CodeGames.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly HealthService _healthService;

    public HealthController(HealthService healthService)
    {
        _healthService = healthService;
    }

    [HttpGet]
    public async Task<ActionResult<HealthDto>> Get(CancellationToken cancellationToken)
    {
        var connected = await _healthService.CanConnectAsync(cancellationToken);
        var dto = new HealthDto
        {
            Status = connected ? "Healthy" : "Unhealthy",
            Database = connected ? "Connected" : "Disconnected"
        };

        if (!connected)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, dto);
        }

        return Ok(dto);
    }
}
