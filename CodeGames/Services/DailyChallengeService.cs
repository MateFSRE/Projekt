using System.Text.Json;
using CodeGames.Data;
using CodeGames.DTOs;
using CodeGames.Exceptions;
using CodeGames.Models;
using CodeGames.Services.Challenges;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CodeGames.Services;

public class DailyChallengeService
{
    private const int MaxAnswerLength = 4000;

    private readonly AppDbContext _db;
    private readonly DailySelectionService _selection;
    private readonly GamificationService _gamification;
    private readonly BadgeService _badges;
    private readonly TimeProvider _clock;
    private readonly IReadOnlyDictionary<ChallengeType, IChallengeHandler> _handlers;

    public DailyChallengeService(
        AppDbContext db,
        DailySelectionService selection,
        GamificationService gamification,
        BadgeService badges,
        TimeProvider clock,
        IEnumerable<IChallengeHandler> handlers)
    {
        _db = db;
        _selection = selection;
        _gamification = gamification;
        _badges = badges;
        _clock = clock;
        _handlers = handlers.ToDictionary(handler => handler.Type);
    }

    public async Task<DailyBoardDto> GetBoardAsync(int userId, CancellationToken cancellationToken)
    {
        var today = _selection.Today();
        var slots = await _selection.EnsureTodayAsync(cancellationToken);
        var ids = slots.Select(slot => slot.ChallengeId).ToList();
        var challenges = await _db.Challenges
            .AsNoTracking()
            .Include(challenge => challenge.Options)
            .Where(challenge => ids.Contains(challenge.Id) && challenge.IsActive)
            .ToListAsync(cancellationToken);

        var completedIds = await _db.DailyCompletions
            .AsNoTracking()
            .Where(completion => completion.UserId == userId && completion.Date == today)
            .Select(completion => completion.ChallengeId)
            .ToListAsync(cancellationToken);

        var total = await _db.DailyCompletions.CountAsync(
            completion => completion.UserId == userId,
            cancellationToken);

        var items = new List<DailySlotDto>();
        foreach (var slot in slots.OrderBy(item => item.Slot))
        {
            var challenge = challenges.FirstOrDefault(item => item.Id == slot.ChallengeId);
            if (challenge is null || !_handlers.TryGetValue(challenge.Type, out var handler))
            {
                continue;
            }

            handler.ValidateConfig(challenge);
            items.Add(new DailySlotDto
            {
                Slot = slot.Slot,
                Completed = completedIds.Contains(challenge.Id),
                Challenge = new ChallengeDto
                {
                    Id = challenge.Id,
                    Type = challenge.Type,
                    Language = challenge.Language,
                    Difficulty = challenge.Difficulty,
                    Title = challenge.Title,
                    Points = challenge.Points,
                    Payload = handler.BuildPublicView(challenge)
                }
            });
        }

        return new DailyBoardDto
        {
            Challenges = items,
            CompletedCount = completedIds.Count,
            SecondsUntilUtcMidnight = _selection.SecondsUntilUtcMidnight(),
            Challenger = ChallengerRules.Describe(total)
        };
    }

    public async Task<DailyAnswerResultDto> SubmitAsync(
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

        var today = _selection.Today();
        var slots = await _selection.EnsureTodayAsync(cancellationToken);
        if (slots.All(slot => slot.ChallengeId != challengeId))
        {
            throw new AppException(StatusCodes.Status404NotFound, "Ovaj zadatak nije u današnjem izazovu.");
        }

        var challenge = await _db.Challenges
            .Include(item => item.Options)
            .FirstOrDefaultAsync(item => item.Id == challengeId && item.IsActive, cancellationToken);
        if (challenge is null || !_handlers.TryGetValue(challenge.Type, out var handler))
        {
            throw new AppException(StatusCodes.Status404NotFound, "Zadatak nije pronađen.");
        }

        handler.ValidateConfig(challenge);
        var evaluation = handler.Evaluate(challenge, answer);
        var fraction = evaluation.ScoreFraction;
        if (double.IsNaN(fraction) || double.IsInfinity(fraction) || fraction < 0d)
        {
            fraction = 0d;
        }
        else if (fraction > 1d)
        {
            fraction = 1d;
        }

        var now = _clock.GetUtcNow().UtcDateTime;
        var alreadyToday = await _db.DailyCompletions.AnyAsync(
            completion => completion.UserId == userId
                && completion.Date == today
                && completion.ChallengeId == challenge.Id,
            cancellationToken);
        var awardsPoints = evaluation.IsCorrect && !alreadyToday;
        var completedBefore = await _db.DailyCompletions.CountAsync(
            completion => completion.UserId == userId && completion.Date == today,
            cancellationToken);
        var allClearBonus = awardsPoints && completedBefore == DailyPicker.SlotCount - 1
            ? DailyPicker.AllClearBonus
            : 0;

        var savedCompletion = false;
        var tierUp = false;
        GamificationResult? outcome = null;
        User? user = null;
        var pointsAwarded = 0;

        try
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
            user = await _db.Users.FirstOrDefaultAsync(item => item.Id == userId, cancellationToken);
            if (user is null)
            {
                throw new AppException(StatusCodes.Status401Unauthorized, "Niste prijavljeni.");
            }

            if (awardsPoints)
            {
                outcome = _gamification.Apply(new GamificationInput
                {
                    ChallengePoints = challenge.Points,
                    ScoreFraction = 1d,
                    BonusPoints = allClearBonus,
                    IsCorrect = true,
                    AlreadySolved = false,
                    AwardFullPoints = true,
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
                pointsAwarded = outcome.PointsAwarded;
            }

            var attempt = new Attempt
            {
                UserId = userId,
                ChallengeId = challenge.Id,
                AnswerJson = rawAnswer,
                IsCorrect = evaluation.IsCorrect,
                ScoreFraction = fraction,
                PointsAwarded = pointsAwarded,
                CreatedAt = now,
                IsDaily = true
            };
            _db.Attempts.Add(attempt);
            await _db.SaveChangesAsync(cancellationToken);

            if (awardsPoints)
            {
                _db.DailyCompletions.Add(new DailyCompletion
                {
                    UserId = userId,
                    Date = today,
                    ChallengeId = challenge.Id,
                    AttemptId = attempt.Id,
                    PointsAwarded = pointsAwarded,
                    CompletedAt = now
                });

                var totalAfter = await _db.DailyCompletions.CountAsync(
                    completion => completion.UserId == userId,
                    cancellationToken) + 1;
                tierUp = await _badges.SyncChallengerAsync(userId, totalAfter, now, cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);
                savedCompletion = true;
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            if (_db.Database.CurrentTransaction is not null)
            {
                await _db.Database.RollbackTransactionAsync(cancellationToken);
            }

            _db.ChangeTracker.Clear();
            savedCompletion = false;
            tierUp = false;
            outcome = null;
            pointsAwarded = 0;
            allClearBonus = 0;
            _db.Attempts.Add(new Attempt
            {
                UserId = userId,
                ChallengeId = challenge.Id,
                AnswerJson = rawAnswer,
                IsCorrect = evaluation.IsCorrect,
                ScoreFraction = fraction,
                PointsAwarded = 0,
                CreatedAt = now,
                IsDaily = true
            });
            await _db.SaveChangesAsync(cancellationToken);
            user = await _db.Users.AsNoTracking().FirstAsync(item => item.Id == userId, cancellationToken);
        }

        if (user is null)
        {
            throw new AppException(StatusCodes.Status401Unauthorized, "Niste prijavljeni.");
        }

        var completedCount = await _db.DailyCompletions.CountAsync(
            completion => completion.UserId == userId && completion.Date == today,
            cancellationToken);
        var total = await _db.DailyCompletions.CountAsync(
            completion => completion.UserId == userId,
            cancellationToken);
        var progress = ChallengerRules.Describe(total);

        return new DailyAnswerResultDto
        {
            IsCorrect = evaluation.IsCorrect,
            ScoreFraction = fraction,
            Details = evaluation.Details,
            Explanation = challenge.Explanation,
            RevealedSolution = evaluation.RevealedSolution,
            PointsAwarded = savedCompletion ? pointsAwarded : 0,
            NewXp = user.Xp,
            NewLevel = user.Level,
            LevelUp = outcome?.LevelUp ?? false,
            CurrentStreak = user.CurrentStreak,
            NewAwards = [],
            DailyCompleted = evaluation.IsCorrect && (alreadyToday || savedCompletion),
            CompletedCount = completedCount,
            ChallengerTier = progress.Tier,
            ChallengerTierUp = tierUp && savedCompletion,
            TotalDailyCompleted = total,
            AllClearBonus = savedCompletion ? allClearBonus : 0
        };
    }

    public async Task<AdminDailyDto> GetAdminViewAsync(CancellationToken cancellationToken)
    {
        var poolCount = await _selection.PoolCountAsync(cancellationToken);
        var dto = new AdminDailyDto
        {
            PoolCount = poolCount,
            PoolTarget = DailyPicker.TargetPool,
            MinimumPool = DailyPicker.SlotCount
        };

        if (poolCount < DailyPicker.TargetPool)
        {
            dto.Message = poolCount < DailyPicker.SlotCount
                ? "Bazen ima manje od 3 zadatka. Dnevni izazov ne radi dok se ne dodaju."
                : "Bazen ima manje od 10 zadataka.";
        }

        if (poolCount < DailyPicker.SlotCount)
        {
            return dto;
        }

        var slots = await _selection.EnsureTodayAsync(cancellationToken);
        var ids = slots.Select(slot => slot.ChallengeId).ToList();
        var titles = await _db.Challenges
            .AsNoTracking()
            .Where(challenge => ids.Contains(challenge.Id))
            .Select(challenge => new { challenge.Id, challenge.Title, challenge.Type })
            .ToListAsync(cancellationToken);

        dto.Selection = slots
            .OrderBy(slot => slot.Slot)
            .Select(slot =>
            {
                var challenge = titles.FirstOrDefault(item => item.Id == slot.ChallengeId);
                return new AdminDailySlotDto
                {
                    Slot = slot.Slot,
                    ChallengeId = slot.ChallengeId,
                    Title = challenge?.Title ?? string.Empty,
                    Type = challenge?.Type.ToString() ?? string.Empty
                };
            })
            .ToList();

        return dto;
    }

    private static bool IsUniqueViolation(DbUpdateException exception)
    {
        return exception.InnerException is SqlException sql && sql.Number is 2601 or 2627;
    }
}
