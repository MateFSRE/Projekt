namespace CodeGames.DTOs;

public class ChallengerProgressDto
{
    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int TotalCompleted { get; set; }

    public int Tier { get; set; }

    public string TierLabel { get; set; } = string.Empty;

    public int Progress { get; set; }

    public int Step { get; set; }

    public int NextThreshold { get; set; }
}
