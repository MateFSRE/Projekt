using CodeGames.Models;

namespace CodeGames.DTOs;

public class UserDto
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public int Xp { get; set; }

    public int Level { get; set; }
}
