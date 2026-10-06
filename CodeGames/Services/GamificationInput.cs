namespace CodeGames.Services;

// Ulaz za jedan pokušaj. Klasa ne čita bazu; pozivatelj donese stanje.
public sealed record GamificationInput
{
    public int ChallengePoints { get; init; }

    public double ScoreFraction { get; init; }

    public int BonusPoints { get; init; }

    public bool IsCorrect { get; init; }

    public bool AlreadySolved { get; init; }

    // Dnevni izazov daje pune bodove zadatka, bez niza točnih odgovora.
    public bool AwardFullPoints { get; init; }

    public double BestPreviousFraction { get; init; }

    // Koliko je zadnjih pokušaja zaredom bilo točno, prije ovog.
    public int ConsecutiveCorrectBefore { get; init; }

    public int Xp { get; init; }

    public int Level { get; init; }

    public int CurrentStreak { get; init; }

    public int BestStreak { get; init; }

    public DateOnly? LastActiveDate { get; init; }

    public DateOnly Today { get; init; }
}
