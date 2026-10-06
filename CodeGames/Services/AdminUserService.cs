using CodeGames.Data;
using CodeGames.DTOs;
using CodeGames.Exceptions;
using CodeGames.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeGames.Services;

public class AdminUserService
{
    private readonly AppDbContext _db;

    public AdminUserService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResultDto<AdminUserDto>> ListAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 50 ? 10 : pageSize;

        var query = _db.Users.AsNoTracking();
        var term = search?.Trim();
        if (!string.IsNullOrEmpty(term))
        {
            query = query.Where(user => user.Username.Contains(term) || user.Email.Contains(term));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(user => user.Username)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(user => new AdminUserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                Xp = user.Xp,
                Level = user.Level
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<AdminUserDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            Total = total
        };
    }

    public async Task<AdminUserDto> CreateAsync(AdminUserCreateDto dto, CancellationToken cancellationToken)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var username = dto.Username.Trim();
        await EnsureUniqueAsync(email, username, null, cancellationToken);

        var user = new User
        {
            Email = email,
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role,
            Xp = 0,
            Level = 1,
            CurrentStreak = 0,
            BestStreak = 0,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(user);
    }

    public async Task<AdminUserDto> UpdateAsync(
        int userId,
        AdminUserUpdateDto dto,
        CancellationToken cancellationToken)
    {
        var user = await FindAsync(userId, cancellationToken);
        var email = dto.Email.Trim().ToLowerInvariant();
        var username = dto.Username.Trim();
        await EnsureUniqueAsync(email, username, user.Id, cancellationToken);
        await EnsureAdminRemainsAsync(user, dto.Role, deleting: false, cancellationToken);

        user.Email = email;
        user.Username = username;
        user.Role = dto.Role;
        await _db.SaveChangesAsync(cancellationToken);
        return Map(user);
    }

    public async Task DeleteAsync(int userId, int actorId, CancellationToken cancellationToken)
    {
        if (userId == actorId)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Ne možeš obrisati vlastiti račun.");
        }

        var user = await FindAsync(userId, cancellationToken);
        await EnsureAdminRemainsAsync(user, UserRole.User, deleting: true, cancellationToken);

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        await _db.DailyCompletions.Where(completion => completion.UserId == user.Id).ExecuteDeleteAsync(cancellationToken);
        await _db.UserBadges.Where(badge => badge.UserId == user.Id).ExecuteDeleteAsync(cancellationToken);
        await _db.Attempts.Where(attempt => attempt.UserId == user.Id).ExecuteDeleteAsync(cancellationToken);
        _db.Users.Remove(user);
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task<User> FindAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(item => item.Id == userId, cancellationToken);
        if (user is null)
        {
            throw new AppException(StatusCodes.Status404NotFound, "Korisnik nije pronađen.");
        }

        return user;
    }

    private async Task EnsureUniqueAsync(
        string email,
        string username,
        int? exceptUserId,
        CancellationToken cancellationToken)
    {
        var emailTaken = exceptUserId is null
            ? await _db.Users.AnyAsync(user => user.Email == email, cancellationToken)
            : await _db.Users.AnyAsync(
                user => user.Email == email && user.Id != exceptUserId,
                cancellationToken);
        if (emailTaken)
        {
            throw new AppException(StatusCodes.Status409Conflict, "E-mail je već registriran.");
        }

        var usernameTaken = exceptUserId is null
            ? await _db.Users.AnyAsync(user => user.Username.ToLower() == username.ToLower(), cancellationToken)
            : await _db.Users.AnyAsync(
                user => user.Id != exceptUserId && user.Username.ToLower() == username.ToLower(),
                cancellationToken);
        if (usernameTaken)
        {
            throw new AppException(StatusCodes.Status409Conflict, "Korisničko ime je zauzeto.");
        }
    }

    private async Task EnsureAdminRemainsAsync(
        User user,
        UserRole nextRole,
        bool deleting,
        CancellationToken cancellationToken)
    {
        if (user.Role != UserRole.Admin)
        {
            return;
        }

        var removesAdmin = deleting || nextRole != UserRole.Admin;
        if (!removesAdmin)
        {
            return;
        }

        var adminCount = await _db.Users.CountAsync(item => item.Role == UserRole.Admin, cancellationToken);
        if (adminCount <= 1)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Posljednji admin mora ostati admin.");
        }
    }

    private static AdminUserDto Map(User user)
    {
        return new AdminUserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            Xp = user.Xp,
            Level = user.Level
        };
    }
}
