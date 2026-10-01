using CodeGames.DTOs;
using CodeGames.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CodeGames.Controllers;

[ApiController]
[AllowAnonymous]
[EnableRateLimiting("demo")]
[Route("api/demo")]
public class DemoController : ControllerBase
{
    private readonly DemoService _demoService;

    public DemoController(DemoService demoService)
    {
        _demoService = demoService;
    }

    [HttpGet("challenge")]
    public async Task<ActionResult<ChallengeDto>> GetChallenge(CancellationToken cancellationToken)
    {
        var challenge = await _demoService.GetChallengeAsync(cancellationToken);
        return Ok(challenge);
    }

    [RequestSizeLimit(8 * 1024)]
    [HttpPost("answer")]
    public async Task<ActionResult<DemoResultDto>> Answer(ChallengeAnswerDto dto, CancellationToken cancellationToken)
    {
        var result = await _demoService.CheckAnswerAsync(dto.Answer, cancellationToken);
        return Ok(result);
    }
}
