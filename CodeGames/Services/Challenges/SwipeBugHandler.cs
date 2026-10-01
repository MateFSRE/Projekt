using System.Text.Json;
using System.Text.Json.Serialization;
using CodeGames.Exceptions;
using CodeGames.Models;

namespace CodeGames.Services.Challenges;

public class SwipeBugHandler : IChallengeHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ChallengeType Type => ChallengeType.SwipeBug;

    public object BuildPublicView(Challenge challenge)
    {
        var config = ReadConfig(challenge);
        return new
        {
            code = config.Code,
            language = config.Language
        };
    }

    public EvaluationResult Evaluate(Challenge challenge, JsonElement answer)
    {
        var config = ReadConfig(challenge);
        if (!TryReadHasBug(answer, out var hasBug))
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Odgovor mora biti ima bug ili ispravno.");
        }

        var correct = hasBug == config.HasBug;
        return new EvaluationResult
        {
            IsCorrect = correct,
            ScoreFraction = correct ? 1d : 0d,
            Details = new { hasBug },
            RevealedSolution = new { hasBug = config.HasBug, bugLine = config.BugLine }
        };
    }

    public void ValidateConfig(Challenge challenge)
    {
        ReadConfig(challenge);
    }

    private static bool TryReadHasBug(JsonElement answer, out bool hasBug)
    {
        hasBug = false;
        if (answer.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (!answer.TryGetProperty("hasBug", out var property))
        {
            return false;
        }

        if (property.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
        {
            return false;
        }

        hasBug = property.GetBoolean();
        return true;
    }

    private static SwipeBugConfig ReadConfig(Challenge challenge)
    {
        if (string.IsNullOrWhiteSpace(challenge.ConfigJson))
        {
            throw new AppException(StatusCodes.Status400BadRequest, "SwipeBug treba ConfigJson.");
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(challenge.ConfigJson);
        }
        catch (JsonException)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "ConfigJson za SwipeBug nije ispravan.");
        }

        using (document)
        {
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object
                || !root.TryGetProperty("hasBug", out var hasBugProperty)
                || hasBugProperty.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
            {
                throw new AppException(StatusCodes.Status400BadRequest, "SwipeBug ConfigJson mora imati hasBug.");
            }
        }

        SwipeBugConfig? config;
        try
        {
            config = JsonSerializer.Deserialize<SwipeBugConfig>(challenge.ConfigJson, JsonOptions);
        }
        catch (JsonException)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "ConfigJson za SwipeBug nije ispravan.");
        }

        if (config is null || string.IsNullOrWhiteSpace(config.Code))
        {
            throw new AppException(StatusCodes.Status400BadRequest, "SwipeBug treba kod.");
        }

        if (config.Language is not ("Python" or "JavaScript"))
        {
            throw new AppException(StatusCodes.Status400BadRequest, "SwipeBug jezik mora biti Python ili JavaScript.");
        }

        var lineCount = CountLines(config.Code);
        if (lineCount is < 4 or > 8)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "SwipeBug kod mora imati od 4 do 8 linija.");
        }

        if (config.HasBug)
        {
            if (config.BugLine is null || config.BugLine < 1 || config.BugLine > lineCount)
            {
                throw new AppException(StatusCodes.Status400BadRequest, "Ako kod ima bug, bugLine mora biti broj postojeće linije.");
            }
        }
        else if (config.BugLine is not null)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Ispravan kod nema bugLine.");
        }

        return config;
    }

    private static int CountLines(string code)
    {
        var normalized = code.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').TrimEnd('\n');
        return normalized.Length == 0 ? 0 : normalized.Split('\n').Length;
    }

    private sealed class SwipeBugConfig
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("language")]
        public string Language { get; set; } = string.Empty;

        [JsonPropertyName("hasBug")]
        public bool HasBug { get; set; }

        [JsonPropertyName("bugLine")]
        public int? BugLine { get; set; }
    }
}
