namespace CodeGames.DTOs;

public class DailyBoardDto
{
    public List<DailySlotDto> Challenges { get; set; } = [];

    public int CompletedCount { get; set; }

    public int SecondsUntilUtcMidnight { get; set; }

    public ChallengerProgressDto Challenger { get; set; } = new();
}

public class DailySlotDto
{
    public int Slot { get; set; }

    public bool Completed { get; set; }

    public ChallengeDto Challenge { get; set; } = new();
}

public class DailyAnswerResultDto : AnswerResultDto
{
    public bool DailyCompleted { get; set; }

    public int CompletedCount { get; set; }

    public int ChallengerTier { get; set; }

    public bool ChallengerTierUp { get; set; }

    public int TotalDailyCompleted { get; set; }

    public int AllClearBonus { get; set; }
}
