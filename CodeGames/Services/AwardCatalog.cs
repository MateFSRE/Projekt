using CodeGames.DTOs;
using CodeGames.Models;

namespace CodeGames.Services;

// Pragovi nagrada: 5, 10, 25, 50, 100, a zatim svakih 100.
public static class AwardCatalog
{
    private static readonly int[] EarlyThresholds = [5, 10, 25, 50, 100];

    public static List<AwardDto> ProfileAwards(int level, int bestStreak)
    {
        var awards = new List<AwardDto>();
        foreach (var threshold in VisibleThresholds(level))
        {
            awards.Add(Describe(AwardKind.Level, threshold, level >= threshold));
        }

        foreach (var threshold in VisibleThresholds(bestStreak))
        {
            awards.Add(Describe(AwardKind.Streak, threshold, bestStreak >= threshold));
        }

        return awards;
    }

    public static List<AwardDto> NewlyEarned(int previousLevel, int newLevel, int previousBestStreak, int newBestStreak)
    {
        var awards = new List<AwardDto>();
        foreach (var threshold in Crossed(previousLevel, newLevel))
        {
            awards.Add(Describe(AwardKind.Level, threshold, true));
        }

        foreach (var threshold in Crossed(previousBestStreak, newBestStreak))
        {
            awards.Add(Describe(AwardKind.Streak, threshold, true));
        }

        return awards;
    }

    public static IReadOnlyList<int> VisibleThresholds(int progress)
    {
        var thresholds = new List<int>(EarlyThresholds);
        if (progress < 100)
        {
            return thresholds;
        }

        for (var threshold = 200; threshold <= progress; threshold += 100)
        {
            thresholds.Add(threshold);
        }

        var next = ((progress / 100) + 1) * 100;
        if (!thresholds.Contains(next))
        {
            thresholds.Add(next);
        }

        return thresholds;
    }

    public static IReadOnlyList<int> Crossed(int before, int after)
    {
        if (after <= before)
        {
            return [];
        }

        var crossed = new List<int>();
        foreach (var threshold in EarlyThresholds)
        {
            if (before < threshold && after >= threshold)
            {
                crossed.Add(threshold);
            }
        }

        var start = Math.Max(200, ((before / 100) + 1) * 100);
        for (var threshold = start; threshold <= after; threshold += 100)
        {
            if (before < threshold)
            {
                crossed.Add(threshold);
            }
        }

        return crossed;
    }

    private static AwardDto Describe(AwardKind kind, int threshold, bool earned)
    {
        if (kind == AwardKind.Streak)
        {
            return new AwardDto
            {
                Kind = kind,
                Threshold = threshold,
                Title = $"Niz od {threshold} dana",
                Description = $"Niz igranja dosegao je {threshold} dana.",
                Earned = earned
            };
        }

        return new AwardDto
        {
            Kind = kind,
            Threshold = threshold,
            Title = $"Razina {threshold}",
            Description = $"Dosegnuta je razina {threshold}.",
            Earned = earned
        };
    }
}
