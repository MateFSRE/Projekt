using System.Text.Json;
using CodeGames.Data;
using CodeGames.DTOs;
using CodeGames.Exceptions;
using CodeGames.Models;
using CodeGames.Services.Challenges;
using Microsoft.EntityFrameworkCore;

namespace CodeGames.Services;

public class ChallengesService
{
    private const int MaxAnswerLength = 4000;

    private readonly AppDbContext _db;
    private readonly GamificationService _gamification;
    private readonly TimeProvider _clock;
    private readonly IReadOnlyDictionary<ChallengeType, IChallengeHandler> _handlers;

    public ChallengesService(
        AppDbContext db,
        GamificationService gamification,
        TimeProvider clock,
        IEnumerable<IChallengeHandler> handlers)
    {
        _db = db;
        _gamification = gamification;
        _clock = clock;
        _handlers = handlers.ToDictionary(handler => handler.Type);
    }

    public async Task<ChallengeDto> GetNextAsync(
        int userId,
        IReadOnlyCollection<ChallengeType> types,
        CancellationToken cancellationToken)
    {
        var (start, end) = TodayWindow();
        var solvedIds = await _db.Attempts
            .AsNoTracking()
            .Where(attempt => attempt.UserId == userId
                && attempt.IsCorrect
                && !attempt.IsDaily
                && attempt.CreatedAt >= start
                && attempt.CreatedAt < end)
            .Select(attempt => attempt.ChallengeId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var allowed = types.Count == 0
            ? _handlers.Keys.ToHashSet()
            : types.Where(type => _handlers.ContainsKey(type)).ToHashSet();

        var pool = await _db.Challenges
            .AsNoTracking()
            .Include(challenge => challenge.Options)
            .Where(challenge => challenge.IsActive)
            .ToListAsync(cancellationToken);

        var remaining = pool
            .Where(challenge => allowed.Contains(challenge.Type) && !solvedIds.Contains(challenge.Id))
            .ToList();

        if (remaining.Count == 0)
        {
            var message = types.Count > 0
                ? "Nema više zadataka u odabranim igrama. Sutra se ponovno otvaraju."
                : "Nema više zadataka za danas. Sutra se ponovno otvaraju.";
            throw new AppException(StatusCodes.Status404NotFound, message);
        }

        var challenge = remaining[Random.Shared.Next(remaining.Count)];
        var handler = _handlers[challenge.Type];
        handler.ValidateConfig(challenge);

        return new ChallengeDto
        {
            Id = challenge.Id,
            Type = challenge.Type,
            Language = challenge.Language,
            Difficulty = challenge.Difficulty,
            Title = challenge.Title,
            Points = challenge.Points,
            Payload = handler.BuildPublicView(challenge)
        };
    }

    public async Task<AnswerResultDto> SubmitAsync(
        int userId,
        int challengeId,
        JsonElement answer,
        CancellationToken cancellationToken)
    {
        if (answer.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Odgovor je obavezan.");
        }

        var rawAnswer = answer.GetRawText();
        if (rawAnswer.Length > MaxAnswerLength)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Odgovor je predug.");
        }

        var challenge = await _db.Challenges
            .Include(item => item.Options)
            .FirstOrDefaultAsync(
                item => item.Id == challengeId && item.IsActive,
                cancellationToken);

        if (challenge is null)
        {
            throw new AppException(StatusCodes.Status404NotFound, "Zadatak nije pronađen.");
        }

        if (!_handlers.TryGetValue(challenge.Type, out var handler))
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Ova vrsta zadatka još nije dostupna.");
        }

        handler.ValidateConfig(challenge);
        var evaluation = handler.Evaluate(challenge, answer);
        var fraction = ClampFraction(evaluation.ScoreFraction);

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        var user = await _db.Users.FirstOrDefaultAsync(item => item.Id == userId, cancellationToken);
        if (user is null)
        {
            throw new AppException(StatusCodes.Status401Unauthorized, "Niste prijavljeni.");
        }

        var (start, end) = TodayWindow();
        var previous = await _db.Attempts
            .Where(attempt => attempt.UserId == userId
                && attempt.ChallengeId == challenge.Id
                && !attempt.IsDaily
                && attempt.CreatedAt >= start
                && attempt.CreatedAt < end)
            .Select(attempt => new { attempt.IsCorrect, attempt.ScoreFraction })
            .ToListAsync(cancellationToken);

        var correctFlags = await _db.Attempts
            .Where(attempt => attempt.UserId == userId && !attempt.IsDaily)
            .OrderByDescending(attempt => attempt.CreatedAt)
            .ThenByDescending(attempt => attempt.Id)
            .Select(attempt => attempt.IsCorrect)
            .ToListAsync(cancellationToken);

        var consecutive = 0;
        foreach (var correct in correctFlags)
        {
            if (!correct)
            {
                break;
            }

            consecutive++;
        }

        var today = DateOnly.FromDateTime(_clock.GetUtcNow().UtcDateTime);
        var now = _clock.GetUtcNow().UtcDateTime;
        var previousLevel = user.Level;
        var previousBestStreak = user.BestStreak;
        var outcome = _gamification.Apply(new GamificationInput
        {
            ChallengePoints = challenge.Points,
            ScoreFraction = fraction,
            BonusPoints = evaluation.BonusPoints,
            IsCorrect = evaluation.IsCorrect,
            AlreadySolved = previous.Any(attempt => attempt.IsCorrect),
            BestPreviousFraction = previous.Count == 0 ? 0d : previous.Max(attempt => attempt.ScoreFraction),
            ConsecutiveCorrectBefore = consecutive,
            Xp = user.Xp,
            Level = user.Level,
            CurrentStreak = user.CurrentStreak,
            BestStreak = user.BestStreak,
            LastActiveDate = user.LastActiveDate is null
                ? null
                : DateOnly.FromDateTime(user.LastActiveDate.Value),
            Today = today
        });

        user.Xp = outcome.NewXp;
        user.Level = outcome.NewLevel;
        user.CurrentStreak = outcome.CurrentStreak;
        user.BestStreak = outcome.BestStreak;
        user.LastActiveDate = DateTime.SpecifyKind(
            outcome.LastActiveDate.ToDateTime(TimeOnly.MinValue),
            DateTimeKind.Utc);

        _db.Attempts.Add(new Attempt
        {
            UserId = userId,
            ChallengeId = challenge.Id,
            AnswerJson = rawAnswer,
            IsCorrect = evaluation.IsCorrect,
            ScoreFraction = fraction,
            PointsAwarded = outcome.PointsAwarded,
            CreatedAt = now
        });

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new AnswerResultDto
        {
            IsCorrect = evaluation.IsCorrect,
            ScoreFraction = fraction,
            Details = evaluation.Details,
            Explanation = challenge.Explanation,
            RevealedSolution = evaluation.RevealedSolution,
            PointsAwarded = outcome.PointsAwarded,
            NewXp = outcome.NewXp,
            NewLevel = outcome.NewLevel,
            LevelUp = outcome.LevelUp,
            CurrentStreak = outcome.CurrentStreak,
            NewAwards = AwardCatalog.NewlyEarned(
                previousLevel,
                outcome.NewLevel,
                previousBestStreak,
                outcome.BestStreak)
        };
    }

    private (DateTime Start, DateTime End) TodayWindow()
    {
        var today = DateOnly.FromDateTime(_clock.GetUtcNow().UtcDateTime);
        var start = today.ToDateTime(TimeOnly.MinValue);
        return (start, start.AddDays(1));
    }

    private static double ClampFraction(double fraction)
    {
        if (double.IsNaN(fraction) || double.IsInfinity(fraction) || fraction < 0d)
        {
            return 0d;
        }

        return fraction > 1d ? 1d : fraction;
    }
}
