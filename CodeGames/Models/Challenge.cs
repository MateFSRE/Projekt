namespace CodeGames.Models;

public class Challenge
{
    public int Id { get; set; }

    public ChallengeType Type { get; set; }

    public string Language { get; set; } = string.Empty;

    public int Difficulty { get; set; }

    public string Title { get; set; } = string.Empty;

    public string CodeSnippet { get; set; } = string.Empty;

    public string Question { get; set; } = string.Empty;

    public string CorrectAnswer { get; set; } = string.Empty;

    public string Explanation { get; set; } = string.Empty;

    // JSON pravila tipa. SwipeBug ovdje drži kod, jezik, hasBug i bugLine.
    public string? ConfigJson { get; set; }

    public int Points { get; set; }

    public bool IsActive { get; set; }

    // Jedan zadatak ima više opcija kod odabira i ispravka greške.
    public ICollection<ChallengeOption> Options { get; set; } = new List<ChallengeOption>();

    // Jedan zadatak prima više pokušaja.
    public ICollection<Attempt> Attempts { get; set; } = new List<Attempt>();
}
