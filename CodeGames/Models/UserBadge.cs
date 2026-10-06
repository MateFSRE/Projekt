namespace CodeGames.Models;

// Osvojeni tier jednog bedža. Definicija bedža nije redak u bazi.
public class UserBadge
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string BadgeKey { get; set; } = string.Empty;

    public int Tier { get; set; }

    public DateTime AwardedAt { get; set; }

    public User User { get; set; } = null!;
}
