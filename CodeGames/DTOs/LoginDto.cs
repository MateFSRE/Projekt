using System.ComponentModel.DataAnnotations;

namespace CodeGames.DTOs;

public class LoginDto
{
    [Required(ErrorMessage = "E-mail je obavezan.")]
    [EmailAddress(ErrorMessage = "E-mail nije ispravan.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lozinka je obavezna.")]
    public string Password { get; set; } = string.Empty;
}
