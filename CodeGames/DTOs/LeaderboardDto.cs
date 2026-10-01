namespace CodeGames.DTOs;

public class LeaderboardDto
{
    public string Period { get; set; } = "total";

    public List<LeaderboardEntryDto> Entries { get; set; } = [];

    // Postavljen samo kad je prijavljeni korisnik izvan prvih 50.
    public LeaderboardEntryDto? CurrentUser { get; set; }
}
