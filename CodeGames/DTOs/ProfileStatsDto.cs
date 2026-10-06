using CodeGames.Models;

namespace CodeGames.DTOs;

public class ProfileStatsDto
{
    public string Username { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public ChallengeType? FavoriteType { get; set; }

    public int FavoritePlays { get; set; }

    public List<AwardDto> Awards { get; set; } = [];

    public ChallengerProgressDto Challenger { get; set; } = new();

    public int Level { get; set; }

    public int ProgressPercent { get; set; }

    public int TotalPoints { get; set; }

    public double CorrectPercent { get; set; }

    public int CurrentStreak { get; set; }

    public int BestStreak { get; set; }

    public int SolvedCount { get; set; }

    public List<SolvedTypeDto> SolvedByType { get; set; } = [];

    public List<RecentAttemptDto> RecentAttempts { get; set; } = [];

    public List<DailyPointsDto> DailyPoints { get; set; } = [];
}

public class SolvedTypeDto
{
    public ChallengeType Type { get; set; }

    public int Solved { get; set; }
}

public class RecentAttemptDto
{
    public int Id { get; set; }

    public string ChallengeTitle { get; set; } = string.Empty;

    public ChallengeType Type { get; set; }

    public bool IsCorrect { get; set; }

    public int PointsAwarded { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class DailyPointsDto
{
    public DateOnly Date { get; set; }

    public int Points { get; set; }
}
