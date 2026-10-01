namespace CodeGames.Services;

// Pravila bodova, razina i dnevnog niza. Nema veze s bazom.
public class GamificationService
{
    // Razina 1 počinje na 0. Svaki sljedeći prag je početak više razine.
    public static readonly int[] XpThresholds = [0, 100, 250, 500, 1000, 2000, 4000, 8000];

    // Bonus samo uz potpuno točan, još neriješen zadatak: 2 boda po odgovoru u nizu.
    public const int BonusPerCorrectInRow = 2;

    // Promašaj koji ipak ima nešto točno. Malo, i samo ako je bolji od prijašnjeg pokušaja.
    public const int PartialCreditPoints = 2;

    public GamificationResult Apply(GamificationInput input)
    {
        var awarded = AwardedPoints(input);
        var newXp = input.Xp + awarded;
        var newLevel = LevelForXp(newXp);
        var streak = NextStreak(input);

        return new GamificationResult
        {
            PointsAwarded = awarded,
            NewXp = newXp,
            NewLevel = newLevel,
            LevelUp = newLevel > input.Level,
            CurrentStreak = streak,
            BestStreak = Math.Max(input.BestStreak, streak),
            LastActiveDate = input.Today
        };
    }

    public static int LevelForXp(int xp)
    {
        var level = 1;
        for (var index = 0; index < XpThresholds.Length; index++)
        {
            if (xp >= XpThresholds[index])
            {
                level = index + 1;
            }
        }

        return level;
    }

    // Udio od početka trenutne razine do sljedećeg praga. Zadnja razina je 100.
    public static int ProgressPercent(int xp)
    {
        var index = LevelForXp(xp) - 1;
        if (index >= XpThresholds.Length - 1)
        {
            return 100;
        }

        var start = XpThresholds[index];
        var end = XpThresholds[index + 1];
        var span = end - start;
        if (span <= 0)
        {
            return 100;
        }

        var percent = (int)Math.Floor((xp - start) * 100d / span);
        if (percent < 0)
        {
            return 0;
        }

        return percent > 99 ? 99 : percent;
    }

    private static int AwardedPoints(GamificationInput input)
    {
        if (input.AlreadySolved)
        {
            return 0;
        }

        var fraction = Clamp(input.ScoreFraction);
        if (!input.IsCorrect)
        {
            if (fraction <= 0d || fraction <= input.BestPreviousFraction)
            {
                return 0;
            }

            return PartialCreditPoints;
        }

        var row = Math.Max(0, input.ConsecutiveCorrectBefore) + 1;
        var bonus = input.BonusPoints > 0 ? input.BonusPoints : 0;
        return PerformancePoints(input.ChallengePoints, fraction) + (row * BonusPerCorrectInRow) + bonus;
    }

    private static int PerformancePoints(int challengePoints, double fraction)
    {
        var value = challengePoints * Clamp(fraction);
        return (int)Math.Round(value, MidpointRounding.AwayFromZero);
    }

    private static double Clamp(double fraction)
    {
        if (double.IsNaN(fraction) || double.IsInfinity(fraction) || fraction < 0d)
        {
            return 0d;
        }

        return fraction > 1d ? 1d : fraction;
    }

    // UTC datum. Isti dan ne mijenja niz. Jučer ga poveća. Stariji dan ga vraća na 1.
    private static int NextStreak(GamificationInput input)
    {
        if (input.LastActiveDate is null)
        {
            return 1;
        }

        if (input.LastActiveDate.Value == input.Today)
        {
            return input.CurrentStreak;
        }

        if (input.LastActiveDate.Value == input.Today.AddDays(-1))
        {
            return input.CurrentStreak + 1;
        }

        return 1;
    }
}
