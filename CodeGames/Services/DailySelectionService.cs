using CodeGames.Data;
using CodeGames.Exceptions;
using CodeGames.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CodeGames.Services;

public class DailySelectionService
{
    private readonly AppDbContext _db;
    private readonly TimeProvider _clock;

    public DailySelectionService(AppDbContext db, TimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public DateOnly Today()
    {
        return DateOnly.FromDateTime(_clock.GetUtcNow().UtcDateTime);
    }

    public int SecondsUntilUtcMidnight()
    {
        var now = _clock.GetUtcNow().UtcDateTime;
        var nextMidnight = now.Date.AddDays(1);
        var seconds = (int)Math.Floor((nextMidnight - now).TotalSeconds);
        return seconds < 0 ? 0 : seconds;
    }

    public async Task<IReadOnlyList<DailySelection>> EnsureTodayAsync(CancellationToken cancellationToken)
    {
        var today = Today();
        var existing = await LoadAsync(today, cancellationToken);
        if (existing.Count > 0)
        {
            return existing;
        }

        var pool = await _db.Challenges
            .AsNoTracking()
            .Where(challenge => challenge.IsActive && challenge.IsInDailyPool)
            .Select(challenge => challenge.Id)
            .ToListAsync(cancellationToken);

        var yesterday = await _db.DailySelections
            .AsNoTracking()
            .Where(selection => selection.Date == today.AddDays(-1))
            .Select(selection => selection.ChallengeId)
            .ToListAsync(cancellationToken);

        var picked = DailyPicker.Pick(pool, yesterday, Random.Shared);
        var rows = picked.Select((id, index) => new DailySelection
        {
            Date = today,
            ChallengeId = id,
            Slot = index + 1
        }).ToList();

        _db.DailySelections.AddRange(rows);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            return rows;
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            _db.ChangeTracker.Clear();
            var stored = await LoadAsync(today, cancellationToken);
            if (stored.Count == 0)
            {
                throw new AppException(
                    StatusCodes.Status409Conflict,
                    "Dnevni odabir se upravo sprema. Pokušaj ponovno.");
            }

            return stored;
        }
    }

    public async Task<int> PoolCountAsync(CancellationToken cancellationToken)
    {
        return await _db.Challenges.CountAsync(
            challenge => challenge.IsActive && challenge.IsInDailyPool,
            cancellationToken);
    }

    private async Task<List<DailySelection>> LoadAsync(DateOnly date, CancellationToken cancellationToken)
    {
        return await _db.DailySelections
            .AsNoTracking()
            .Where(selection => selection.Date == date)
            .OrderBy(selection => selection.Slot)
            .ToListAsync(cancellationToken);
    }

    private static bool IsUniqueViolation(DbUpdateException exception)
    {
        return exception.InnerException is SqlException sql && sql.Number is 2601 or 2627;
    }
}
