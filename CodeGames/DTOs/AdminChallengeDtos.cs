using System.ComponentModel.DataAnnotations;
using CodeGames.Models;

namespace CodeGames.DTOs;

public class AdminChallengeListItemDto
{
    public int Id { get; set; }

    public ChallengeType Type { get; set; }

    public string Language { get; set; } = string.Empty;

    public int Difficulty { get; set; }

    public string Title { get; set; } = string.Empty;

    public int Points { get; set; }

    public bool IsActive { get; set; }

    public bool IsInDailyPool { get; set; }
}

public class AdminChallengeDto
{
    public int Id { get; set; }

    public ChallengeType Type { get; set; }

    public string Language { get; set; } = string.Empty;

    public int Difficulty { get; set; }

    public string Title { get; set; } = string.Empty;

    public string CodeSnippet { get; set; } = string.Empty;

    public string Question { get; set; } = string.Empty;

    public string CorrectAnswer { get; set; } = string.Empty;

    public string Explanation { get; set; } = string.Empty;

    public string? ConfigJson { get; set; }

    public int Points { get; set; }

    public bool IsActive { get; set; }

    public bool IsInDailyPool { get; set; }

    public List<AdminOptionDto> Options { get; set; } = [];
}

public class AdminOptionDto
{
    public int Id { get; set; }

    public string Text { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
}

public class AdminChallengeWriteDto
{
    [Required(ErrorMessage = "Tip zadatka je obavezan.")]
    public ChallengeType Type { get; set; }

    [Required(ErrorMessage = "Jezik je obavezan.")]
    [MaxLength(30, ErrorMessage = "Jezik je predug.")]
    public string Language { get; set; } = string.Empty;

    [Range(1, 3, ErrorMessage = "Težina mora biti od 1 do 3.")]
    public int Difficulty { get; set; }

    [Required(ErrorMessage = "Naslov je obavezan.")]
    [MaxLength(200, ErrorMessage = "Naslov je predug.")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000, ErrorMessage = "Isječak koda je predug.")]
    public string? CodeSnippet { get; set; }

    [MaxLength(1000, ErrorMessage = "Pitanje je predugo.")]
    public string? Question { get; set; }

    [MaxLength(500, ErrorMessage = "Točan odgovor je predug.")]
    public string? CorrectAnswer { get; set; }

    [Required(ErrorMessage = "Objašnjenje je obavezno.")]
    [MaxLength(2000, ErrorMessage = "Objašnjenje je predugo.")]
    public string Explanation { get; set; } = string.Empty;

    [MaxLength(20000, ErrorMessage = "ConfigJson je predug.")]
    public string? ConfigJson { get; set; }

    [Range(1, 100, ErrorMessage = "Bodovi moraju biti od 1 do 100.")]
    public int Points { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsInDailyPool { get; set; }

    public List<AdminOptionWriteDto> Options { get; set; } = [];
}

public class AdminOptionWriteDto
{
    [Required(ErrorMessage = "Tekst opcije je obavezan.")]
    [MaxLength(500, ErrorMessage = "Tekst opcije je predug.")]
    public string Text { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
}
