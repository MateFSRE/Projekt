using System.Text.Json;
using System.Text.Json.Serialization;
using CodeGames.Exceptions;
using CodeGames.Models;

namespace CodeGames.Services.Challenges;

public class TraceExecutionHandler : IChallengeHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ChallengeType Type => ChallengeType.TraceExecution;

    public object BuildPublicView(Challenge challenge)
    {
        var config = ReadConfig(challenge);
        return new
        {
            language = config.Language,
            code = config.Code,
            variables = config.Variables,
            steps = config.Steps.Select(step => new { line = step.Line }).ToList(),
            askAt = config.AskAt
        };
    }

    public EvaluationResult Evaluate(Challenge challenge, JsonElement answer)
    {
        var config = ReadConfig(challenge);
        if (answer.ValueKind != JsonValueKind.Object)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Odgovor mora biti mapa koraka i vrijednosti.");
        }

        var cells = new List<CellCheck>();
        foreach (var stepIndex in config.AskAt)
        {
            var given = ReadStep(answer, stepIndex);
            var expected = config.Steps[stepIndex].Expected;
            foreach (var name in config.Variables)
            {
                var value = given.TryGetValue(name, out var typed) ? typed.Trim() : string.Empty;
                cells.Add(new CellCheck(stepIndex, name, value == expected[name].Trim()));
            }
        }

        var correctCount = cells.Count(cell => cell.Correct);
        var fraction = cells.Count == 0 ? 0d : (double)correctCount / cells.Count;

        return new EvaluationResult
        {
            IsCorrect = correctCount == cells.Count,
            ScoreFraction = fraction,
            Details = new
            {
                cells = cells.Select(cell => new { stepIndex = cell.StepIndex, name = cell.Name, correct = cell.Correct }).ToList()
            },
            RevealedSolution = new
            {
                text = string.Join("\n", config.AskAt.Select(index =>
                {
                    var pairs = config.Variables.Select(name => $"{name}={config.Steps[index].Expected[name]}");
                    return $"korak {index + 1}: {string.Join(", ", pairs)}";
                })),
                cells = config.AskAt.SelectMany(index => config.Variables.Select(name => new
                {
                    stepIndex = index,
                    name,
                    value = config.Steps[index].Expected[name]
                })).ToList()
            }
        };
    }

    public void ValidateConfig(Challenge challenge)
    {
        ReadConfig(challenge);
    }

    private static Dictionary<string, string> ReadStep(JsonElement answer, int stepIndex)
    {
        if (!answer.TryGetProperty(stepIndex.ToString(), out var step) || step.ValueKind != JsonValueKind.Object)
        {
            return [];
        }

        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var property in step.EnumerateObject())
        {
            values[property.Name] = property.Value.ValueKind == JsonValueKind.String
                ? property.Value.GetString() ?? string.Empty
                : string.Empty;
        }

        return values;
    }

    private static TraceConfig ReadConfig(Challenge challenge)
    {
        if (string.IsNullOrWhiteSpace(challenge.ConfigJson))
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Prati izvršavanje treba ConfigJson.");
        }

        TraceConfig? config;
        try
        {
            config = JsonSerializer.Deserialize<TraceConfig>(challenge.ConfigJson, JsonOptions);
        }
        catch (JsonException)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "ConfigJson za praćenje izvršavanja nije ispravan.");
        }

        if (config is null)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "ConfigJson za praćenje izvršavanja nije ispravan.");
        }

        if (config.Language is not ("Python" or "JavaScript"))
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Prati izvršavanje jezik mora biti Python ili JavaScript.");
        }

        if (string.IsNullOrWhiteSpace(config.Code))
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Prati izvršavanje treba kod.");
        }

        if (config.Variables is null || config.Variables.Count is < 1 or > 6
            || config.Variables.Any(name => string.IsNullOrWhiteSpace(name))
            || config.Variables.Distinct(StringComparer.Ordinal).Count() != config.Variables.Count)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Varijable moraju imati jedinstvena imena.");
        }

        if (config.Steps is null || config.Steps.Count < 2)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Prati izvršavanje treba barem dva koraka.");
        }

        var lineCount = config.Code.Replace("\r\n", "\n").Split('\n').Length;
        foreach (var step in config.Steps)
        {
            if (step.Line < 1 || step.Line > lineCount)
            {
                throw new AppException(StatusCodes.Status400BadRequest, "Broj linije u koraku nije u kodu.");
            }

            if (step.Expected is null
                || step.Expected.Count != config.Variables.Count
                || config.Variables.Any(name => !step.Expected.ContainsKey(name)))
            {
                throw new AppException(StatusCodes.Status400BadRequest, "Svaki korak treba očekivanu vrijednost za svaku varijablu.");
            }
        }

        if (config.AskAt is null || config.AskAt.Count == 0
            || config.AskAt.Distinct().Count() != config.AskAt.Count
            || config.AskAt.Any(index => index < 0 || index >= config.Steps.Count))
        {
            throw new AppException(StatusCodes.Status400BadRequest, "askAt mora pokazivati na postojeće korake.");
        }

        return config;
    }

    private sealed record CellCheck(int StepIndex, string Name, bool Correct);

    private sealed class TraceConfig
    {
        [JsonPropertyName("language")]
        public string Language { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("variables")]
        public List<string> Variables { get; set; } = [];

        [JsonPropertyName("steps")]
        public List<TraceStep> Steps { get; set; } = [];

        [JsonPropertyName("askAt")]
        public List<int> AskAt { get; set; } = [];
    }

    private sealed class TraceStep
    {
        [JsonPropertyName("line")]
        public int Line { get; set; }

        [JsonPropertyName("expected")]
        public Dictionary<string, string> Expected { get; set; } = [];
    }
}
