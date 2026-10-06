using CodeGames.DTOs;
using CodeGames.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeGames.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/daily")]
public class AdminDailyController : ControllerBase
{
    private readonly DailyChallengeService _daily;

    public AdminDailyController(DailyChallengeService daily)
    {
        _daily = daily;
    }

    [HttpGet]
    public async Task<ActionResult<AdminDailyDto>> Get(CancellationToken cancellationToken)
    {
        var view = await _daily.GetAdminViewAsync(cancellationToken);
        return Ok(view);
    }
}
