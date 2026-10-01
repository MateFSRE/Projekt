using CodeGames.DTOs;
using CodeGames.Exceptions;
using CodeGames.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeGames.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/leaderboard")]
public class LeaderboardController : ControllerBase
{
    private readonly LeaderboardService _leaderboard;

    public LeaderboardController(LeaderboardService leaderboard)
    {
        _leaderboard = leaderboard;
    }

    [HttpGet]
    public async Task<ActionResult<LeaderboardDto>> Get([FromQuery] string? period, CancellationToken cancellationToken)
    {
        var board = await _leaderboard.GetAsync(period, CurrentUserId(), cancellationToken);
        return Ok(board);
    }

    private int? CurrentUserId()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var idValue = User.FindFirst("id")?.Value;
        if (!int.TryParse(idValue, out var userId))
        {
            throw new AppException(StatusCodes.Status401Unauthorized, "Niste prijavljeni.");
        }

        return userId;
    }
}
