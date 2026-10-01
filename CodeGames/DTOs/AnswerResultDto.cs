namespace CodeGames.DTOs;

public class AnswerResultDto
{
    public bool IsCorrect { get; set; }

    public double ScoreFraction { get; set; }

    public object Details { get; set; } = new { };

    public string Explanation { get; set; } = string.Empty;

    public object RevealedSolution { get; set; } = new { };

    public int PointsAwarded { get; set; }

    public int NewXp { get; set; }

    public int NewLevel { get; set; }

    public bool LevelUp { get; set; }

    public int CurrentStreak { get; set; }

    public List<AwardDto> NewAwards { get; set; } = [];
}
