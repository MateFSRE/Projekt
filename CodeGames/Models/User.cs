namespace CodeGames.Models;

public class User
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public int Xp { get; set; }

    public int Level { get; set; }

    public int CurrentStreak { get; set; }

    public int BestStreak { get; set; }

    public DateTime? LastActiveDate { get; set; }

    public DateTime CreatedAt { get; set; }

    // Jedan korisnik ima više pokušaja.
    public ICollection<Attempt> Attempts { get; set; } = new List<Attempt>();
}
