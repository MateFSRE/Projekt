using CodeGames.Data;
using CodeGames.DTOs;
using CodeGames.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CodeGames.Services;

public class ProfileService
{
    private const int ChartDays = 30;
    private const int RecentCount = 10;

    private readonly AppDbContext _db;

    public ProfileService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ProfileStatsDto> GetStatsAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await _db.Users
            .AsNoTracking()
            .Where(item => item.Id == userId)
            .Select(item => new
            {
                item.Username,
                item.CreatedAt,
                item.Xp,
                item.CurrentStreak,
                item.BestStreak,
                item.LastActiveDate
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            throw new AppException(StatusCodes.Status401Unauthorized, "Niste prijavljeni.");
        }

        var attemptCount = await _db.Attempts.CountAsync(attempt => attempt.UserId == userId, cancellationToken);
        var correctCount = await _db.Attempts.CountAsync(
            attempt => attempt.UserId == userId && attempt.IsCorrect,
            cancellationToken);

        var solved = await _db.Attempts
            .AsNoTracking()
            .Where(attempt => attempt.UserId == userId && attempt.IsCorrect && !attempt.IsDaily)
            .Select(attempt => new { attempt.ChallengeId, attempt.Challenge.Type })
            .Distinct()
            .ToListAsync(cancellationToken);

        var solvedByType = solved
            .GroupBy(item => item.Type)
            .Select(group => new SolvedTypeDto
            {
                Type = group.Key,
                Solved = group.Count()
            })
            .OrderByDescending(item => item.Solved)
            .ThenBy(item => item.Type.ToString())
            .ToList();

        var recent = await _db.Attempts
            .AsNoTracking()
            .Where(attempt => attempt.UserId == userId)
            .OrderByDescending(attempt => attempt.CreatedAt)
            .ThenByDescending(attempt => attempt.Id)
            .Take(RecentCount)
            .Select(attempt => new RecentAttemptDto
            {
                Id = attempt.Id,
                ChallengeTitle = attempt.Challenge.Title,
                Type = attempt.Challenge.Type,
                IsCorrect = attempt.IsCorrect,
                PointsAwarded = attempt.PointsAwarded,
                CreatedAt = attempt.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var chartStart = today.AddDays(-(ChartDays - 1));
        var chartStartUtc = chartStart.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var daily = await _db.Attempts
            .AsNoTracking()
            .Where(attempt => attempt.UserId == userId && attempt.CreatedAt >= chartStartUtc)
            .GroupBy(attempt => attempt.CreatedAt.Date)
            .Select(group => new
            {
                Day = group.Key,
                Points = group.Sum(attempt => attempt.PointsAwarded)
            })
            .ToListAsync(cancellationToken);

        var byDay = daily.ToDictionary(item => DateOnly.FromDateTime(item.Day), item => item.Points);
        var series = new List<DailyPointsDto>(ChartDays);
        for (var offset = 0; offset < ChartDays; offset++)
        {
            var date = chartStart.AddDays(offset);
            series.Add(new DailyPointsDto
            {
                Date = date,
                Points = byDay.GetValueOrDefault(date)
            });
        }

        var plays = await _db.Attempts
            .AsNoTracking()
            .Where(attempt => attempt.UserId == userId)
            .GroupBy(attempt => attempt.Challenge.Type)
            .Select(group => new
            {
                Type = group.Key,
                Plays = group.Count(),
                Last = group.Max(attempt => attempt.CreatedAt)
            })
            .ToListAsync(cancellationToken);

        var favorite = plays
            .OrderByDescending(item => item.Plays)
            .ThenByDescending(item => item.Last)
            .ThenBy(item => item.Type.ToString())
            .FirstOrDefault();

        var level = GamificationService.LevelForXp(user.Xp);
        var correctPercent = attemptCount == 0
            ? 0d
            : Math.Round(correctCount * 100d / attemptCount, 1, MidpointRounding.AwayFromZero);
        var dailyCompleted = await _db.DailyCompletions.CountAsync(
            completion => completion.UserId == userId,
            cancellationToken);

        return new ProfileStatsDto
        {
            Username = user.Username,
            CreatedAt = user.CreatedAt,
            FavoriteType = favorite?.Type,
            FavoritePlays = favorite?.Plays ?? 0,
            Awards = AwardCatalog.ProfileAwards(level, user.BestStreak),
            Challenger = ChallengerRules.Describe(dailyCompleted),
            Level = level,
            ProgressPercent = GamificationService.ProgressPercent(user.Xp),
            TotalPoints = user.Xp,
            CorrectPercent = correctPercent,
            CurrentStreak = VisibleStreak(user.CurrentStreak, user.LastActiveDate, today),
            BestStreak = user.BestStreak,
            SolvedCount = solved.Count,
            SolvedByType = solvedByType,
            RecentAttempts = recent,
            DailyPoints = series
        };
    }

    // Spremljeni niz vrijedi samo ako je zadnja aktivnost danas ili jučer.
    private static int VisibleStreak(int currentStreak, DateTime? lastActive, DateOnly today)
    {
        if (lastActive is null || currentStreak <= 0)
        {
            return 0;
        }

        var last = DateOnly.FromDateTime(lastActive.Value);
        if (last == today || last == today.AddDays(-1))
        {
            return currentStreak;
        }

        return 0;
    }
}
