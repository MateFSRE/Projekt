using CodeGames.Data;
using CodeGames.DTOs;
using CodeGames.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CodeGames.Services;

public class LeaderboardService
{
    public const int TopCount = 50;

    private readonly AppDbContext _db;

    public LeaderboardService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<LeaderboardDto> GetAsync(string? period, int? userId, CancellationToken cancellationToken)
    {
        var mode = Normalize(period);
        return mode == "weekly"
            ? await WeeklyAsync(userId, cancellationToken)
            : await TotalAsync(userId, cancellationToken);
    }

    private async Task<LeaderboardDto> TotalAsync(int? userId, CancellationToken cancellationToken)
    {
        var rows = await _db.Users
            .AsNoTracking()
            .OrderByDescending(user => user.Xp)
            .ThenBy(user => user.Username)
            .Take(TopCount)
            .Select(user => new LeaderboardEntryDto
            {
                Username = user.Username,
                Level = user.Level,
                Xp = user.Xp,
                Points = user.Xp
            })
            .ToListAsync(cancellationToken);

        AssignRanks(rows);

        LeaderboardEntryDto? current = null;
        if (userId is int id)
        {
            var me = await _db.Users
                .AsNoTracking()
                .Where(user => user.Id == id)
                .Select(user => new { user.Username, user.Level, user.Xp })
                .FirstOrDefaultAsync(cancellationToken);

            if (me is not null)
            {
                var ahead = await _db.Users.CountAsync(
                    user => user.Xp > me.Xp || (user.Xp == me.Xp && user.Username.CompareTo(me.Username) < 0),
                    cancellationToken);
                var rank = ahead + 1;
                if (rank > TopCount)
                {
                    current = new LeaderboardEntryDto
                    {
                        Rank = rank,
                        Username = me.Username,
                        Level = me.Level,
                        Xp = me.Xp,
                        Points = me.Xp
                    };
                }
            }
        }

        return new LeaderboardDto
        {
            Period = "total",
            Entries = rows,
            CurrentUser = current
        };
    }

    private async Task<LeaderboardDto> WeeklyAsync(int? userId, CancellationToken cancellationToken)
    {
        var start = WeekStartUtc(DateTime.UtcNow);
        var end = start.AddDays(7);
        var scores = _db.Attempts
            .AsNoTracking()
            .Where(attempt => attempt.CreatedAt >= start && attempt.CreatedAt < end)
            .GroupBy(attempt => attempt.UserId)
            .Select(group => new
            {
                UserId = group.Key,
                Points = group.Sum(attempt => attempt.PointsAwarded)
            })
            .Where(row => row.Points > 0)
            .Join(
                _db.Users.AsNoTracking(),
                row => row.UserId,
                user => user.Id,
                (row, user) => new
                {
                    user.Id,
                    user.Username,
                    user.Level,
                    user.Xp,
                    row.Points
                });

        var rows = await scores
            .OrderByDescending(row => row.Points)
            .ThenBy(row => row.Username)
            .Take(TopCount)
            .Select(row => new LeaderboardEntryDto
            {
                Username = row.Username,
                Level = row.Level,
                Xp = row.Xp,
                Points = row.Points
            })
            .ToListAsync(cancellationToken);

        AssignRanks(rows);

        LeaderboardEntryDto? current = null;
        if (userId is int id)
        {
            var me = await scores.FirstOrDefaultAsync(row => row.Id == id, cancellationToken);
            if (me is not null)
            {
                var ahead = await scores.CountAsync(
                    row => row.Points > me.Points || (row.Points == me.Points && row.Username.CompareTo(me.Username) < 0),
                    cancellationToken);
                var rank = ahead + 1;
                if (rank > TopCount)
                {
                    current = new LeaderboardEntryDto
                    {
                        Rank = rank,
                        Username = me.Username,
                        Level = me.Level,
                        Xp = me.Xp,
                        Points = me.Points
                    };
                }
            }
        }

        return new LeaderboardDto
        {
            Period = "weekly",
            Entries = rows,
            CurrentUser = current
        };
    }

    private static void AssignRanks(List<LeaderboardEntryDto> rows)
    {
        for (var index = 0; index < rows.Count; index++)
        {
            rows[index].Rank = index + 1;
        }
    }

    private static string Normalize(string? period)
    {
        if (string.IsNullOrWhiteSpace(period) || period.Equals("total", StringComparison.OrdinalIgnoreCase))
        {
            return "total";
        }

        if (period.Equals("weekly", StringComparison.OrdinalIgnoreCase))
        {
            return "weekly";
        }

        throw new AppException(StatusCodes.Status400BadRequest, "Razdoblje može biti total ili weekly.");
    }

    // Tjedan počinje u ponedjeljak 00:00 UTC, isto kao ostali datumi u aplikaciji.
    private static DateTime WeekStartUtc(DateTime utcNow)
    {
        var date = DateOnly.FromDateTime(utcNow);
        var daysFromMonday = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-daysFromMonday).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
    }
}
