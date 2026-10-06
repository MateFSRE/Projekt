using CodeGames.DTOs;

namespace CodeGames.Services;

// Čista pravila Challengera. Ne čitaju bazu.
public static class ChallengerRules
{
    public const int Step = 10;

    public static int TierFor(int totalCompleted)
    {
        if (totalCompleted < Step)
        {
            return 0;
        }

        return totalCompleted / Step;
    }

    public static bool TierIncreased(int previousTier, int totalCompleted)
    {
        return TierFor(totalCompleted) > previousTier;
    }

    public static ChallengerProgressDto Describe(int totalCompleted)
    {
        var tier = TierFor(totalCompleted);
        var progress = totalCompleted % Step;
        return new ChallengerProgressDto
        {
            Key = BadgeCatalog.Challenger.Key,
            Name = BadgeCatalog.Challenger.Name,
            Description = BadgeCatalog.Challenger.Description,
            TotalCompleted = totalCompleted,
            Tier = tier,
            TierLabel = Roman(tier),
            Progress = progress,
            Step = Step,
            NextThreshold = (tier + 1) * Step
        };
    }

    public static string Roman(int tier)
    {
        if (tier <= 0)
        {
            return string.Empty;
        }

        var parts = new (int Value, string Symbol)[]
        {
            (1000, "M"), (900, "CM"), (500, "D"), (400, "CD"),
            (100, "C"), (90, "XC"), (50, "L"), (40, "XL"),
            (10, "X"), (9, "IX"), (5, "V"), (4, "IV"), (1, "I")
        };

        var remaining = tier;
        var text = string.Empty;
        foreach (var (value, symbol) in parts)
        {
            while (remaining >= value)
            {
                text += symbol;
                remaining -= value;
            }
        }

        return text;
    }
}
