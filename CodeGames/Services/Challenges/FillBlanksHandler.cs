using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using CodeGames.Exceptions;
using CodeGames.Models;

namespace CodeGames.Services.Challenges;

public class FillBlanksHandler : IChallengeHandler
{
    private static readonly Regex Placeholder = new(@"\{\{([A-Za-z0-9]+)\}\}", RegexOptions.Compiled);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ChallengeType Type => ChallengeType.FillBlanks;

    public object BuildPublicView(Challenge challenge)
    {
        var config = ReadConfig(challenge);
        return new
        {
            language = config.Language,
            template = config.Template,
            wordBank = Shuffle(config.WordBank)
        };
    }

    public EvaluationResult Evaluate(Challenge challenge, JsonElement answer)
    {
        var config = ReadConfig(challenge);
        if (answer.ValueKind != JsonValueKind.Object)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Odgovor mora biti mapa rupa i riječi.");
        }

        var filled = ReadAnswers(answer);
        var checks = config.Blanks.Select(blank =>
        {
            var given = filled.TryGetValue(blank.Id, out var word) ? word.Trim() : string.Empty;
            return new BlankCheck(blank.Id, given.Length > 0 && given == blank.Answer.Trim());
        }).ToList();

        var correctCount = checks.Count(check => check.Correct);
        var fraction = config.Blanks.Count == 0 ? 0d : (double)correctCount / config.Blanks.Count;

        return new EvaluationResult
        {
            IsCorrect = correctCount == config.Blanks.Count,
            ScoreFraction = fraction,
            Details = new
            {
                blanks = checks.Select(check => new { id = check.Id, correct = check.Correct }).ToList()
            },
            RevealedSolution = new
            {
                blanks = config.Blanks.Select(blank => new { id = blank.Id, answer = blank.Answer }).ToList()
            }
        };
    }

    public void ValidateConfig(Challenge challenge)
    {
        ReadConfig(challenge);
    }

    private static Dictionary<string, string> ReadAnswers(JsonElement answer)
    {
        var filled = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var property in answer.EnumerateObject())
        {
            filled[property.Name] = property.Value.ValueKind == JsonValueKind.String
                ? property.Value.GetString() ?? string.Empty
                : string.Empty;
        }

        return filled;
    }

    private static List<string> Shuffle(IReadOnlyList<string> words)
    {
        var copy = words.ToList();
        for (var index = copy.Count - 1; index > 0; index--)
        {
            var swap = Random.Shared.Next(index + 1);
            (copy[index], copy[swap]) = (copy[swap], copy[index]);
        }

        return copy;
    }

    private static FillConfig ReadConfig(Challenge challenge)
    {
        if (string.IsNullOrWhiteSpace(challenge.ConfigJson))
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Dopuni rupe treba ConfigJson.");
        }

        FillConfig? config;
        try
        {
            config = JsonSerializer.Deserialize<FillConfig>(challenge.ConfigJson, JsonOptions);
        }
        catch (JsonException)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "ConfigJson za dopunu rupa nije ispravan.");
        }

        if (config is null)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "ConfigJson za dopunu rupa nije ispravan.");
        }

        if (config.Language is not ("Python" or "JavaScript"))
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Dopuni rupe jezik mora biti Python ili JavaScript.");
        }

        if (string.IsNullOrWhiteSpace(config.Template))
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Dopuni rupe treba predložak.");
        }

        if (config.Blanks is null || config.Blanks.Count < 2)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Dopuni rupe treba barem dvije rupe.");
        }

        if (config.Blanks.Any(blank => string.IsNullOrWhiteSpace(blank.Id) || string.IsNullOrWhiteSpace(blank.Answer)))
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Svaka rupa treba id i točnu riječ.");
        }

        if (config.Blanks.Select(blank => blank.Id).Distinct(StringComparer.Ordinal).Count() != config.Blanks.Count)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Id rupe mora biti jedinstven.");
        }

        var placeholders = Placeholder.Matches(config.Template).Select(match => match.Groups[1].Value).ToList();
        var blankIds = config.Blanks.Select(blank => blank.Id).ToHashSet(StringComparer.Ordinal);
        if (placeholders.Count != blankIds.Count || placeholders.ToHashSet(StringComparer.Ordinal).SetEquals(blankIds) == false)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Oznake {{id}} u predlošku moraju odgovarati rupama.");
        }

        if (config.WordBank is null || config.WordBank.Any(string.IsNullOrWhiteSpace))
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Banka riječi treba neprazne riječi.");
        }

        var needed = config.Blanks
            .GroupBy(blank => blank.Answer, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
        var available = config.WordBank
            .GroupBy(word => word, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);

        foreach (var pair in needed)
        {
            if (!available.TryGetValue(pair.Key, out var count) || count < pair.Value)
            {
                throw new AppException(StatusCodes.Status400BadRequest, "Banka mora sadržavati svaku točnu riječ.");
            }
        }

        var extras = config.WordBank.Count - config.Blanks.Count;
        if (extras is < 2 or > 3)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Banka treba dvije ili tri zamke uz točne riječi.");
        }

        return config;
    }

    private sealed record BlankCheck(string Id, bool Correct);

    private sealed class FillConfig
    {
        [JsonPropertyName("language")]
        public string Language { get; set; } = string.Empty;

        [JsonPropertyName("template")]
        public string Template { get; set; } = string.Empty;

        [JsonPropertyName("blanks")]
        public List<FillBlank> Blanks { get; set; } = [];

        [JsonPropertyName("wordBank")]
        public List<string> WordBank { get; set; } = [];
    }

    private sealed class FillBlank
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("answer")]
        public string Answer { get; set; } = string.Empty;
    }
}
