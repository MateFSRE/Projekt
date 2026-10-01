using CodeGames.Services;
using Xunit;

namespace CodeGames.Tests;

public class GamificationServiceTests
{
    private readonly GamificationService _service = new();
    private static readonly DateOnly Today = new(2026, 9, 30);

    [Fact]
    public void DayChange_IncreasesStreakByOne()
    {
        var result = _service.Apply(Base() with
        {
            LastActiveDate = Today.AddDays(-1),
            CurrentStreak = 3,
            BestStreak = 3
        });

        Assert.Equal(4, result.CurrentStreak);
        Assert.Equal(4, result.BestStreak);
        Assert.Equal(Today, result.LastActiveDate);
    }

    [Fact]
    public void MissedDay_ResetsStreakToOneAndKeepsBest()
    {
        var result = _service.Apply(Base() with
        {
            LastActiveDate = Today.AddDays(-2),
            CurrentStreak = 5,
            BestStreak = 5,
            IsCorrect = false,
            ScoreFraction = 0
        });

        Assert.Equal(1, result.CurrentStreak);
        Assert.Equal(5, result.BestStreak);
        Assert.Equal(0, result.PointsAwarded);
    }

    [Fact]
    public void SameDay_DoesNotChangeStreak()
    {
        var result = _service.Apply(Base() with
        {
            LastActiveDate = Today,
            CurrentStreak = 4,
            BestStreak = 6
        });

        Assert.Equal(4, result.CurrentStreak);
        Assert.Equal(6, result.BestStreak);
    }

    [Fact]
    public void LevelCrossing_RaisesLevelWhenXpReachesThreshold()
    {
        var result = _service.Apply(Base() with
        {
            ChallengePoints = 20,
            ScoreFraction = 1,
            IsCorrect = true,
            Xp = 90,
            Level = 1
        });

        Assert.Equal(20 + GamificationService.BonusPerCorrectInRow, result.PointsAwarded);
        Assert.Equal(112, result.NewXp);
        Assert.Equal(2, result.NewLevel);
        Assert.True(result.LevelUp);
    }

    [Fact]
    public void AlreadySolved_AwardsNoPoints()
    {
        var result = _service.Apply(Base() with
        {
            AlreadySolved = true,
            IsCorrect = true,
            ScoreFraction = 1,
            ConsecutiveCorrectBefore = 4,
            ChallengePoints = 30,
            Xp = 500,
            Level = 4
        });

        Assert.Equal(0, result.PointsAwarded);
        Assert.Equal(500, result.NewXp);
        Assert.Equal(4, result.NewLevel);
        Assert.False(result.LevelUp);
    }

    [Fact]
    public void PartialScore_AwardsOnlyTheImprovement()
    {
        var result = _service.Apply(Base() with
        {
            ChallengePoints = 20,
            ScoreFraction = 0.75,
            BestPreviousFraction = 0.25,
            IsCorrect = false,
            Xp = 40,
            Level = 1
        });

        Assert.Equal(GamificationService.PartialCreditPoints, result.PointsAwarded);
        Assert.Equal(42, result.NewXp);
        Assert.False(result.LevelUp);
    }

    [Fact]
    public void IncorrectAnswer_AwardsNothingWhenItDoesNotImprove()
    {
        var result = _service.Apply(Base() with
        {
            ChallengePoints = 20,
            ScoreFraction = 0,
            BestPreviousFraction = 0.5,
            IsCorrect = false,
            LastActiveDate = Today,
            CurrentStreak = 2,
            Xp = 40,
            Level = 1
        });

        Assert.Equal(0, result.PointsAwarded);
        Assert.Equal(40, result.NewXp);
        Assert.Equal(1, result.NewLevel);
        Assert.False(result.LevelUp);
        Assert.Equal(2, result.CurrentStreak);
    }

    [Fact]
    public void CorrectAnswer_AddsStreakBonusOnTopOfFullPoints()
    {
        var result = _service.Apply(Base() with
        {
            ChallengePoints = 10,
            ScoreFraction = 1,
            IsCorrect = true,
            ConsecutiveCorrectBefore = 2
        });

        Assert.Equal(10 + (3 * GamificationService.BonusPerCorrectInRow), result.PointsAwarded);
    }

    [Fact]
    public void CorrectAnswer_KeepsFullPointsAfterAnEarlierPartialMiss()
    {
        var result = _service.Apply(Base() with
        {
            ChallengePoints = 10,
            ScoreFraction = 1,
            BestPreviousFraction = 0.5,
            IsCorrect = true
        });

        Assert.Equal(10 + GamificationService.BonusPerCorrectInRow, result.PointsAwarded);
    }

    [Fact]
    public void StarBonus_AddsToACorrectAnswer()
    {
        var result = _service.Apply(Base() with
        {
            ChallengePoints = 10,
            ScoreFraction = 1,
            IsCorrect = true,
            BonusPoints = 4
        });

        Assert.Equal(10 + GamificationService.BonusPerCorrectInRow + 4, result.PointsAwarded);
    }

    private static GamificationInput Base()
    {
        return new GamificationInput
        {
            ChallengePoints = 10,
            ScoreFraction = 1,
            IsCorrect = true,
            AlreadySolved = false,
            BestPreviousFraction = 0,
            ConsecutiveCorrectBefore = 0,
            Xp = 0,
            Level = 1,
            CurrentStreak = 1,
            BestStreak = 1,
            LastActiveDate = null,
            Today = Today
        };
    }
}
