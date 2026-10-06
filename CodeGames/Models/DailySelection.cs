namespace CodeGames.Models;

// Tri zadatka odabrana za jedan UTC datum. Isti dan svi igrači dobiju isti odabir.
public class DailySelection
{
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    public int ChallengeId { get; set; }

    public int Slot { get; set; }

    public Challenge Challenge { get; set; } = null!;
}
