using CodeGames.DTOs;
using CodeGames.Exceptions;
using CodeGames.Models;
using CodeGames.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeGames.Controllers;

[Authorize]
[ApiController]
[Route("api/challenges")]
public class ChallengesController : ControllerBase
{
    private readonly ChallengesService _challenges;

    public ChallengesController(ChallengesService challenges)
    {
        _challenges = challenges;
    }

    [HttpGet("next")]
    public async Task<ActionResult<ChallengeDto>> Next(
        [FromQuery] string? types,
        CancellationToken cancellationToken)
    {
        var challenge = await _challenges.GetNextAsync(CurrentUserId(), ReadTypes(types), cancellationToken);
        return Ok(challenge);
    }

    [RequestSizeLimit(8 * 1024)]
    [HttpPost("{id:int}/answer")]
    public async Task<ActionResult<AnswerResultDto>> Answer(
        int id,
        ChallengeAnswerDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _challenges.SubmitAsync(CurrentUserId(), id, dto.Answer, cancellationToken);
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

    private static IReadOnlyList<ChallengeType> ReadTypes(string? types)
    {
        if (string.IsNullOrWhiteSpace(types))
        {
            return [];
        }

        var parsed = new List<ChallengeType>();
        foreach (var part in types.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!Enum.TryParse(part, ignoreCase: false, out ChallengeType type))
            {
                throw new AppException(StatusCodes.Status400BadRequest, "Nepoznata vrsta igre.");
            }

            parsed.Add(type);
        }

        return parsed;
    }
}
