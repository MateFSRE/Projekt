namespace CodeGames.Models;

// Jedan točan dnevni završetak. Isti zadatak se može ponovno brojati sljedeći dan.
public class DailyCompletion
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateOnly Date { get; set; }

    public int ChallengeId { get; set; }

    public int AttemptId { get; set; }

    public int PointsAwarded { get; set; }

    public DateTime CompletedAt { get; set; }

    public User User { get; set; } = null!;

    public Challenge Challenge { get; set; } = null!;

    public Attempt Attempt { get; set; } = null!;
}
