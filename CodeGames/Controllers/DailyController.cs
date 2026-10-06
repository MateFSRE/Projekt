using CodeGames.DTOs;
using CodeGames.Exceptions;
using CodeGames.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeGames.Controllers;

[Authorize]
[ApiController]
[Route("api/daily")]
public class DailyController : ControllerBase
{
    private readonly DailyChallengeService _daily;

    public DailyController(DailyChallengeService daily)
    {
        _daily = daily;
    }

    [HttpGet]
    public async Task<ActionResult<DailyBoardDto>> Get(CancellationToken cancellationToken)
    {
        var board = await _daily.GetBoardAsync(CurrentUserId(), cancellationToken);
        return Ok(board);
    }

    [RequestSizeLimit(8 * 1024)]
    [HttpPost("{challengeId:int}/answer")]
    public async Task<ActionResult<DailyAnswerResultDto>> Answer(
        int challengeId,
        ChallengeAnswerDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _daily.SubmitAsync(CurrentUserId(), challengeId, dto.Answer, cancellationToken);
        return Ok(result);
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
