using CodeGames.DTOs;
using CodeGames.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CodeGames.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto dto, CancellationToken cancellationToken)
    {
        var user = await _authService.RegisterAsync(dto, HttpContext, cancellationToken);
        return Ok(user);
    }

    [AllowAnonymous]
    [EnableRateLimiting("login")]
    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto dto, CancellationToken cancellationToken)
    {
        var user = await _authService.LoginAsync(dto, HttpContext, cancellationToken);
        return Ok(user);
    }

    [Authorize]
    [HttpPost("logout")]
    public ActionResult<MessageDto> Logout()
    {
        _authService.Logout(HttpContext);
        return Ok(new MessageDto { Message = "Odjavljeni ste." });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me(CancellationToken cancellationToken)
    {
        var idValue = User.FindFirst("id")?.Value;
        if (!int.TryParse(idValue, out var userId))
        {
            return Unauthorized(new MessageDto { Message = "Niste prijavljeni." });
        }

        var user = await _authService.GetCurrentAsync(userId, cancellationToken);
        return Ok(user);
    }
}
