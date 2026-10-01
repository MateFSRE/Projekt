namespace CodeGames.Services;

public class GamificationResult
{
    public int PointsAwarded { get; init; }

    public int NewXp { get; init; }

    public int NewLevel { get; init; }

    public bool LevelUp { get; init; }

    public int CurrentStreak { get; init; }

    public int BestStreak { get; init; }

    public DateOnly LastActiveDate { get; init; }
}
