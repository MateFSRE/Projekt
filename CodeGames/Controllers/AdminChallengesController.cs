using CodeGames.DTOs;
using CodeGames.Models;
using CodeGames.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeGames.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/challenges")]
public class AdminChallengesController : ControllerBase
{
    private readonly AdminChallengeService _challenges;

    public AdminChallengesController(AdminChallengeService challenges)
    {
        _challenges = challenges;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<AdminChallengeListItemDto>>> List(
        [FromQuery] ChallengeType? type,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _challenges.ListAsync(type, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AdminChallengeDto>> Get(int id, CancellationToken cancellationToken)
    {
        var challenge = await _challenges.GetAsync(id, cancellationToken);
        return Ok(challenge);
    }

    [HttpPost]
    public async Task<ActionResult<AdminChallengeDto>> Create(
        AdminChallengeWriteDto dto,
        CancellationToken cancellationToken)
    {
        var challenge = await _challenges.CreateAsync(dto, cancellationToken);
        return Ok(challenge);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AdminChallengeDto>> Update(
        int id,
        AdminChallengeWriteDto dto,
        CancellationToken cancellationToken)
    {
        var challenge = await _challenges.UpdateAsync(id, dto, cancellationToken);
        return Ok(challenge);
    }

    [HttpPost("{id:int}/deactivate")]
    public async Task<ActionResult<MessageDto>> Deactivate(int id, CancellationToken cancellationToken)
    {
        await _challenges.DeactivateAsync(id, cancellationToken);
        return Ok(new MessageDto { Message = "Zadatak je deaktiviran." });
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<MessageDto>> Delete(int id, CancellationToken cancellationToken)
    {
        await _challenges.DeleteAsync(id, cancellationToken);
        return Ok(new MessageDto { Message = "Zadatak je obrisan." });
    }
}
