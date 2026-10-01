using CodeGames.DTOs;
using CodeGames.Exceptions;
using CodeGames.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeGames.Controllers;

[Authorize]
[ApiController]
[Route("api/profile")]
public class ProfileController : ControllerBase
{
    private readonly ProfileService _profile;

    public ProfileController(ProfileService profile)
    {
        _profile = profile;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ProfileStatsDto>> Stats(CancellationToken cancellationToken)
    {
        var stats = await _profile.GetStatsAsync(CurrentUserId(), cancellationToken);
        return Ok(stats);
    }

    private int CurrentUserId()
    {
        var idValue = User.FindFirst("id")?.Value;
        if (!int.TryParse(idValue, out var userId))
        {
            throw new AppException(StatusCodes.Status401Unauthorized, "Niste prijavljeni.");
        }

        return userId;
    }
}
