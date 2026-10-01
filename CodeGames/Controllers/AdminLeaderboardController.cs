using CodeGames.DTOs;
using CodeGames.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeGames.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/leaderboard")]
public class AdminLeaderboardController : ControllerBase
{
    private readonly AdminScoreService _scores;

    public AdminLeaderboardController(AdminScoreService scores)
    {
        _scores = scores;
    }

    [HttpPost("reset/{userId:int}")]
    public async Task<ActionResult<MessageDto>> ResetUser(int userId, CancellationToken cancellationToken)
    {
        var result = await _scores.ResetUserAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("reset")]
    public async Task<ActionResult<MessageDto>> ResetAll(CancellationToken cancellationToken)
    {
        var result = await _scores.ResetAllAsync(cancellationToken);
        return Ok(result);
    }
}
