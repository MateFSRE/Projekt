using System.Text.Json;
using CodeGames.Data;
using CodeGames.DTOs;
using CodeGames.Exceptions;
using CodeGames.Models;
using CodeGames.Services.Challenges;
using Microsoft.EntityFrameworkCore;

namespace CodeGames.Services;

public class AdminChallengeService
{
    private readonly AppDbContext _db;
    private readonly IReadOnlyDictionary<ChallengeType, IChallengeHandler> _handlers;

    public AdminChallengeService(AppDbContext db, IEnumerable<IChallengeHandler> handlers)
    {
        _db = db;
        _handlers = handlers.ToDictionary(handler => handler.Type);
    }

    public async Task<PagedResultDto<AdminChallengeListItemDto>> ListAsync(
        ChallengeType? type,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 50 ? 10 : pageSize;

        var query = _db.Challenges.AsNoTracking();
        if (type is not null)
        {
            query = query.Where(challenge => challenge.Type == type);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(challenge => challenge.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(challenge => new AdminChallengeListItemDto
            {
                Id = challenge.Id,
                Type = challenge.Type,
                Language = challenge.Language,
                Difficulty = challenge.Difficulty,
                Title = challenge.Title,
                Points = challenge.Points,
                IsActive = challenge.IsActive,
                IsInDailyPool = challenge.IsInDailyPool
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<AdminChallengeListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            Total = total
        };
    }

    public async Task<AdminChallengeDto> GetAsync(int id, CancellationToken cancellationToken)
    {
        var challenge = await _db.Challenges
            .AsNoTracking()
            .Include(item => item.Options)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (challenge is null)
        {
            throw new AppException(StatusCodes.Status404NotFound, "Zadatak nije pronađen.");
        }

        return Map(challenge);
    }

    public async Task<AdminChallengeDto> CreateAsync(AdminChallengeWriteDto dto, CancellationToken cancellationToken)
    {
        var challenge = new Challenge();
        Apply(challenge, dto);
        Validate(challenge);

        _db.Challenges.Add(challenge);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(challenge);
    }

    public async Task<AdminChallengeDto> UpdateAsync(
        int id,
        AdminChallengeWriteDto dto,
        CancellationToken cancellationToken)
    {
        var challenge = await _db.Challenges
            .Include(item => item.Options)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (challenge is null)
        {
            throw new AppException(StatusCodes.Status404NotFound, "Zadatak nije pronađen.");
        }

        Apply(challenge, dto);
        Validate(challenge);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(challenge);
    }

    public async Task DeactivateAsync(int id, CancellationToken cancellationToken)
    {
        var challenge = await _db.Challenges.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (challenge is null)
        {
            throw new AppException(StatusCodes.Status404NotFound, "Zadatak nije pronađen.");
        }

        challenge.IsActive = false;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var challenge = await _db.Challenges.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (challenge is null)
        {
            throw new AppException(StatusCodes.Status404NotFound, "Zadatak nije pronađen.");
        }

        var hasAttempts = await _db.Attempts.AnyAsync(attempt => attempt.ChallengeId == id, cancellationToken);
        if (hasAttempts)
        {
            throw new AppException(
                StatusCodes.Status409Conflict,
                "Zadatak ima pokušaje. Deaktiviraj ga umjesto brisanja.");
        }

        await _db.DailySelections.Where(selection => selection.ChallengeId == id).ExecuteDeleteAsync(cancellationToken);

        _db.Challenges.Remove(challenge);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private void Apply(Challenge challenge, AdminChallengeWriteDto dto)
    {
        var type = dto.Type;
        if (!_handlers.ContainsKey(type))
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Ova vrsta zadatka još nije dostupna.");
        }

        challenge.Type = type;
        challenge.Language = dto.Language.Trim();
        challenge.Difficulty = dto.Difficulty;
        challenge.Title = dto.Title.Trim();
        challenge.Explanation = dto.Explanation.Trim();
        challenge.Points = dto.Points;
        challenge.IsActive = dto.IsActive;
        challenge.IsInDailyPool = dto.IsInDailyPool;
        challenge.ConfigJson = string.IsNullOrWhiteSpace(dto.ConfigJson) ? null : dto.ConfigJson.Trim();

        var correctOption = dto.Options.FirstOrDefault(option => option.IsCorrect)?.Text;
        challenge.Question = Limit(dto.Question, 1000, DefaultQuestion(type));
        var snippet = UsesConfig(type) ? CodeFromConfig(challenge.ConfigJson) : dto.CodeSnippet;
        challenge.CodeSnippet = Limit(snippet ?? dto.CodeSnippet, 4000, dto.Title.Trim());
        challenge.CorrectAnswer = Limit(dto.CorrectAnswer ?? correctOption, 500, "config");

        ReplaceOptions(challenge, UsesOptions(type) ? dto.Options : []);
    }

    private void Validate(Challenge challenge)
    {
        if (UsesConfig(challenge.Type) && string.IsNullOrWhiteSpace(challenge.ConfigJson))
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Ovaj tip zadatka treba ConfigJson.");
        }

        _handlers[challenge.Type].ValidateConfig(challenge);
    }

    private void ReplaceOptions(Challenge challenge, List<AdminOptionWriteDto> options)
    {
        if (challenge.Options.Count > 0)
        {
            _db.ChallengeOptions.RemoveRange(challenge.Options);
            challenge.Options.Clear();
        }

        foreach (var option in options)
        {
            challenge.Options.Add(new ChallengeOption
            {
                Text = option.Text.Trim(),
                IsCorrect = option.IsCorrect
            });
        }
    }

    private static bool UsesOptions(ChallengeType type)
    {
        return type is ChallengeType.MultipleChoice or ChallengeType.FixBug;
    }

    private static bool UsesConfig(ChallengeType type)
    {
        return type is ChallengeType.SwipeBug
            or ChallengeType.FillBlanks
            or ChallengeType.ReorderCode
            or ChallengeType.TraceExecution
            or ChallengeType.RobotMaze;
    }

    private static string DefaultQuestion(ChallengeType type)
    {
        return type switch
        {
            ChallengeType.SwipeBug => "Ima li ovaj kod bug?",
            ChallengeType.FillBlanks => "Dopuni praznine.",
            ChallengeType.ReorderCode => "Složi linije u ispravan redoslijed.",
            ChallengeType.TraceExecution => "Upiši vrijednosti varijabli na traženim koracima.",
            ChallengeType.RobotMaze => "Dovedi robota do cilja.",
            _ => "Odgovori na zadatak."
        };
    }

    private static string? CodeFromConfig(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (root.TryGetProperty("code", out var code) && code.ValueKind == JsonValueKind.String)
            {
                return code.GetString();
            }

            if (root.TryGetProperty("template", out var template) && template.ValueKind == JsonValueKind.String)
            {
                return template.GetString();
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }

    private static string Limit(string? value, int max, string fallback)
    {
        var text = string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        return text.Length <= max ? text : text[..max];
    }

    private static AdminChallengeDto Map(Challenge challenge)
    {
        return new AdminChallengeDto
        {
            Id = challenge.Id,
            Type = challenge.Type,
            Language = challenge.Language,
            Difficulty = challenge.Difficulty,
            Title = challenge.Title,
            CodeSnippet = challenge.CodeSnippet,
            Question = challenge.Question,
            CorrectAnswer = challenge.CorrectAnswer,
            Explanation = challenge.Explanation,
            ConfigJson = challenge.ConfigJson,
            Points = challenge.Points,
            IsActive = challenge.IsActive,
            IsInDailyPool = challenge.IsInDailyPool,
            Options = challenge.Options
                .OrderBy(option => option.Id)
                .Select(option => new AdminOptionDto
                {
                    Id = option.Id,
                    Text = option.Text,
                    IsCorrect = option.IsCorrect
                })
                .ToList()
        };
    }
}
