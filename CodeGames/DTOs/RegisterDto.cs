using System.ComponentModel.DataAnnotations;

namespace CodeGames.DTOs;

public class RegisterDto
{
    [Required(ErrorMessage = "E-mail je obavezan.")]
    [EmailAddress(ErrorMessage = "E-mail nije ispravan.")]
    [MaxLength(256, ErrorMessage = "E-mail je predug.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Korisničko ime je obavezno.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Korisničko ime mora imati od 3 do 50 znakova.")]
    [RegularExpression("^[a-zA-Z0-9_]+$", ErrorMessage = "Korisničko ime smije sadržavati samo slova, brojeve i podvlaku.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lozinka je obavezna.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Lozinka mora imati najmanje 8 znakova.")]
    public string Password { get; set; } = string.Empty;
}
