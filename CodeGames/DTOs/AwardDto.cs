using CodeGames.Models;

namespace CodeGames.DTOs;

public class AwardDto
{
    public AwardKind Kind { get; set; }

    public int Threshold { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool Earned { get; set; }
}
