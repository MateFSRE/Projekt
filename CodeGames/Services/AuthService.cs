using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CodeGames.Data;
using CodeGames.DTOs;
using CodeGames.Exceptions;
using CodeGames.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CodeGames.Services;

public class AuthService
{
    public const string CookieName = "access_token";

    private readonly AppDbContext _db;
    private readonly JwtSettings _jwt;
    private readonly IHostEnvironment _environment;

    public AuthService(AppDbContext db, IOptions<JwtSettings> jwt, IHostEnvironment environment)
    {
        _db = db;
        _jwt = jwt.Value;
        _environment = environment;
    }

    public async Task<UserDto> RegisterAsync(RegisterDto dto, HttpContext httpContext, CancellationToken cancellationToken)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var username = dto.Username.Trim();

        if (await _db.Users.AnyAsync(user => user.Email == email, cancellationToken))
        {
            throw new AppException(StatusCodes.Status409Conflict, "E-mail je već registriran.");
        }

        if (await _db.Users.AnyAsync(
                user => user.Username.ToLower() == username.ToLower(),
                cancellationToken))
        {
            throw new AppException(StatusCodes.Status409Conflict, "Korisničko ime je zauzeto.");
        }

        var user = new User
        {
            Email = email,
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = UserRole.User,
            Xp = 0,
            Level = 1,
            CurrentStreak = 0,
            BestStreak = 0,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        SetCookie(httpContext, CreateToken(user));
        return Map(user);
    }

    public async Task<UserDto> LoginAsync(LoginDto dto, HttpContext httpContext, CancellationToken cancellationToken)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(item => item.Email == email, cancellationToken);

        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            throw new AppException(StatusCodes.Status401Unauthorized, "E-mail ili lozinka nisu ispravni.");
        }

        SetCookie(httpContext, CreateToken(user));
        return Map(user);
    }

    public void Logout(HttpContext httpContext)
    {
        httpContext.Response.Cookies.Delete(CookieName, BuildCookieOptions());
    }

    public async Task<UserDto> GetCurrentAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new AppException(StatusCodes.Status401Unauthorized, "Prijava više ne vrijedi.");
        }

        return Map(user);
    }

    // Claims koje uloga i kontroler čitaju: id, role, username.
    public string CreateToken(User user)
    {
        var claims = new[]
        {
            new Claim("id", user.Id.ToString()),
            new Claim("role", user.Role.ToString()),
            new Claim("username", user.Username)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.ExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private void SetCookie(HttpContext httpContext, string token)
    {
        var options = BuildCookieOptions();
        options.Expires = DateTimeOffset.UtcNow.AddMinutes(_jwt.ExpirationMinutes);
        httpContext.Response.Cookies.Append(CookieName, token, options);
    }

    private CookieOptions BuildCookieOptions()
    {
        return new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = _environment.IsProduction(),
            Path = "/"
        };
    }

    private static UserDto Map(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            Role = user.Role,
            Xp = user.Xp,
            Level = user.Level
        };
    }
}
