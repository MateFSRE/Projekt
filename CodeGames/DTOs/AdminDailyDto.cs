namespace CodeGames.DTOs;

public class AdminDailyDto
{
    public int PoolCount { get; set; }

    public int PoolTarget { get; set; }

    public int MinimumPool { get; set; }

    public string? Message { get; set; }

    public List<AdminDailySlotDto> Selection { get; set; } = [];
}

public class AdminDailySlotDto
{
    public int Slot { get; set; }

    public int ChallengeId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;
}
