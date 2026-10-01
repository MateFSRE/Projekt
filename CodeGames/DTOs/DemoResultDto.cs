namespace CodeGames.DTOs;

// Rezultat demo pokušaja. Nema bodova, XP-a ni ranga jer se ništa ne sprema.
public class DemoResultDto
{
    public bool IsCorrect { get; set; }

    public double ScoreFraction { get; set; }

    public object Details { get; set; } = new { };

    public string Explanation { get; set; } = string.Empty;

    public object RevealedSolution { get; set; } = new { };
}
