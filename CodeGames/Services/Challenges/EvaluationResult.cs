namespace CodeGames.Services.Challenges;

// Ishod provjere. Šalje se igraču tek nakon što je odgovor poslan.
public class EvaluationResult
{
    public bool IsCorrect { get; init; }

    // Udio bodova zadatka, od 0 do 1. Točan odgovor ga množi s bodovima.
    public double ScoreFraction { get; init; }

    // Dodatni bodovi uz točan odgovor, npr. po pokupljenoj zvjezdici.
    public int BonusPoints { get; init; }

    public object Details { get; init; } = new { };

    public object RevealedSolution { get; init; } = new { };
}
