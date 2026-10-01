namespace CodeGames.Models;

public class ChallengeOption
{
    public int Id { get; set; }

    public int ChallengeId { get; set; }

    public string Text { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }

    // Opcija pripada točno jednom zadatku.
    public Challenge Challenge { get; set; } = null!;
}
