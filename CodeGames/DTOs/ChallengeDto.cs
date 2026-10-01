using CodeGames.Models;

namespace CodeGames.DTOs;

public class ChallengeDto
{
    public int Id { get; set; }

    public ChallengeType Type { get; set; }

    public string Language { get; set; } = string.Empty;

    public int Difficulty { get; set; }

    public string Title { get; set; } = string.Empty;

    public int Points { get; set; }

    public object Payload { get; set; } = new { };
}
