using CodeGames.DTOs;
using CodeGames.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeGames.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/users")]
public class AdminUsersController : ControllerBase
{
    private readonly AdminUserService _users;

    public AdminUsersController(AdminUserService users)
    {
        _users = users;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<AdminUserDto>>> List(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _users.ListAsync(search, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<AdminUserDto>> Create(AdminUserCreateDto dto, CancellationToken cancellationToken)
    {
        var user = await _users.CreateAsync(dto, cancellationToken);
        return Ok(user);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AdminUserDto>> Update(
        int id,
        AdminUserUpdateDto dto,
        CancellationToken cancellationToken)
    {
        var user = await _users.UpdateAsync(id, dto, cancellationToken);
        return Ok(user);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<MessageDto>> Delete(int id, CancellationToken cancellationToken)
    {
        await _users.DeleteAsync(id, AdminCaller.Id(User), cancellationToken);
        return Ok(new MessageDto { Message = "Korisnik je obrisan." });
    }
}
