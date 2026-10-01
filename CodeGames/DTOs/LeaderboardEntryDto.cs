namespace CodeGames.DTOs;

public class LeaderboardEntryDto
{
    public int Rank { get; set; }

    public string Username { get; set; } = string.Empty;

    public int Level { get; set; }

    public int Xp { get; set; }

    // Bodovi po kojima je red poredan: ukupni XP ili zbroj ovog tjedna.
    public int Points { get; set; }
}
