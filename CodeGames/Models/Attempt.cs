namespace CodeGames.Models;

public class Attempt
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ChallengeId { get; set; }

    public string AnswerJson { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }

    public double ScoreFraction { get; set; }

    public int PointsAwarded { get; set; }

    public DateTime CreatedAt { get; set; }

    // Pokušaj pripada točno jednom korisniku.
    public User User { get; set; } = null!;

    // Pokušaj pripada točno jednom zadatku.
    public Challenge Challenge { get; set; } = null!;
}
