using CodeGames.Data;
using CodeGames.DTOs;
using CodeGames.Exceptions;
using CodeGames.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeGames.Services;

public class AdminScoreService
{
    private readonly AppDbContext _db;

    public AdminScoreService(AppDbContext db)
    {
        _db = db;
    }

    // Pokušaji se brišu. Nema arhivske tablice, a tjedna lista čita Attempts,
    // pa bi arhiva i dalje brojala bodove dok se upit ne promijeni.
    public async Task<MessageDto> ResetUserAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(item => item.Id == userId, cancellationToken);
        if (user is null)
        {
            throw new AppException(StatusCodes.Status404NotFound, "Korisnik nije pronađen.");
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        var removed = await _db.Attempts
            .Where(attempt => attempt.UserId == user.Id)
            .ExecuteDeleteAsync(cancellationToken);
        Clear(user);
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new MessageDto
        {
            Message = $"Bodovi korisnika {user.Username} su vraćeni na nulu. Obrisano pokušaja: {removed}."
        };
    }

    public async Task<MessageDto> ResetAllAsync(CancellationToken cancellationToken)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        var removed = await _db.Attempts.ExecuteDeleteAsync(cancellationToken);
        var users = await _db.Users.ToListAsync(cancellationToken);
        foreach (var user in users)
        {
            Clear(user);
        }

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new MessageDto
        {
            Message = $"Bodovi svih korisnika su vraćeni na nulu. Obrisano pokušaja: {removed}."
        };
    }

    private static void Clear(User user)
    {
        user.Xp = 0;
        user.Level = 1;
        user.CurrentStreak = 0;
        user.BestStreak = 0;
        user.LastActiveDate = null;
    }
}
