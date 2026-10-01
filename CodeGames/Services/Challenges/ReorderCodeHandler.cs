using System.Text.Json;
using System.Text.Json.Serialization;
using CodeGames.Exceptions;
using CodeGames.Models;

namespace CodeGames.Services.Challenges;

public class ReorderCodeHandler : IChallengeHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ChallengeType Type => ChallengeType.ReorderCode;

    public object BuildPublicView(Challenge challenge)
    {
        var config = ReadConfig(challenge);
        var lines = Shuffle(config.Lines, config.CorrectOrders);
        return new
        {
            language = config.Language,
            goal = config.Goal,
            lines = lines.Select(line => new { id = line.Id, text = line.Text, indent = line.Indent }).ToList()
        };
    }

    public EvaluationResult Evaluate(Challenge challenge, JsonElement answer)
    {
        var config = ReadConfig(challenge);
        if (answer.ValueKind != JsonValueKind.Array)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Odgovor mora biti niz id-eva linija.");
        }

        var submitted = answer.EnumerateArray()
            .Select(item => item.ValueKind == JsonValueKind.String ? item.GetString() ?? string.Empty : string.Empty)
            .ToList();

        var closest = Closest(submitted, config.CorrectOrders);
        var matches = MatchCount(submitted, closest);
        var isCorrect = config.CorrectOrders.Any(order => order.SequenceEqual(submitted));
        var fraction = config.Lines.Count == 0 ? 0d : (double)matches / config.Lines.Count;
        if (!isCorrect)
        {
            fraction = Math.Min(fraction, (config.Lines.Count - 1) / (double)Math.Max(config.Lines.Count, 1));
        }

        var wrong = new List<string>();
        for (var index = 0; index < submitted.Count; index++)
        {
            var id = submitted[index];
            if (id.Length == 0)
            {
                continue;
            }

            var inPlace = index < closest.Count && closest[index] == id;
            if (!inPlace)
            {
                wrong.Add(id);
            }
        }

        return new EvaluationResult
        {
            IsCorrect = isCorrect,
            ScoreFraction = fraction,
            Details = new { wrongLineIds = wrong },
            RevealedSolution = new
            {
                lineIds = closest.ToList(),
                text = string.Join("\n", closest.Select(id =>
                {
                    var line = config.Lines.First(item => item.Id == id);
                    return new string(' ', line.Indent) + line.Text;
                }))
            }
        };
    }

    public void ValidateConfig(Challenge challenge)
    {
        ReadConfig(challenge);
    }

    private static List<string> Closest(IReadOnlyList<string> submitted, IReadOnlyList<List<string>> orders)
    {
        foreach (var order in orders)
        {
            if (order.SequenceEqual(submitted))
            {
                return order;
            }
        }

        List<string>? best = null;
        var bestScore = -1;
        foreach (var order in orders)
        {
            var score = MatchCount(submitted, order);
            if (score > bestScore)
            {
                bestScore = score;
                best = order;
            }
        }

        return best ?? orders[0];
    }

    private static int MatchCount(IReadOnlyList<string> submitted, IReadOnlyList<string> order)
    {
        var count = 0;
        var limit = Math.Min(submitted.Count, order.Count);
        for (var index = 0; index < limit; index++)
        {
            if (submitted[index] == order[index])
            {
                count++;
            }
        }

        return count;
    }

    private static List<ReorderLine> Shuffle(IReadOnlyList<ReorderLine> lines, IReadOnlyList<List<string>> orders)
    {
        var copy = lines.ToList();
        for (var attempt = 0; attempt < 8; attempt++)
        {
            for (var index = copy.Count - 1; index > 0; index--)
            {
                var swap = Random.Shared.Next(index + 1);
                (copy[index], copy[swap]) = (copy[swap], copy[index]);
            }

            var ids = copy.Select(line => line.Id).ToArray();
            if (!orders.Any(order => order.SequenceEqual(ids)))
            {
                return copy;
            }
        }

        return copy;
    }

    private static ReorderConfig ReadConfig(Challenge challenge)
    {
        if (string.IsNullOrWhiteSpace(challenge.ConfigJson))
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Složi kod treba ConfigJson.");
        }

        ReorderConfig? config;
        try
        {
            config = JsonSerializer.Deserialize<ReorderConfig>(challenge.ConfigJson, JsonOptions);
        }
        catch (JsonException)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "ConfigJson za slaganje koda nije ispravan.");
        }

        if (config is null)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "ConfigJson za slaganje koda nije ispravan.");
        }

        if (config.Language is not ("Python" or "JavaScript"))
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Složi kod jezik mora biti Python ili JavaScript.");
        }

        if (string.IsNullOrWhiteSpace(config.Goal))
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Složi kod treba opis cilja.");
        }

        if (config.Lines is null || config.Lines.Count is < 3 or > 8)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Složi kod treba od 3 do 8 linija.");
        }

        if (config.Lines.Any(line => string.IsNullOrWhiteSpace(line.Id) || string.IsNullOrWhiteSpace(line.Text) || line.Indent < 0))
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Svaka linija treba id, tekst i uvlaku od nula razmaka naviše.");
        }

        if (config.Lines.Select(line => line.Id).Distinct(StringComparer.Ordinal).Count() != config.Lines.Count)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Id linije mora biti jedinstven.");
        }

        if (config.CorrectOrders is null || config.CorrectOrders.Count == 0)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Složi kod treba barem jedan ispravan redoslijed.");
        }

        var ids = config.Lines.Select(line => line.Id).ToHashSet(StringComparer.Ordinal);
        foreach (var order in config.CorrectOrders)
        {
            if (order.Count != ids.Count
                || order.Distinct(StringComparer.Ordinal).Count() != ids.Count
                || order.Any(id => !ids.Contains(id)))
            {
                throw new AppException(StatusCodes.Status400BadRequest, "Ispravan redoslijed mora sadržavati svaku liniju točno jednom.");
            }
        }

        return config;
    }

    private sealed class ReorderConfig
    {
        [JsonPropertyName("language")]
        public string Language { get; set; } = string.Empty;

        [JsonPropertyName("goal")]
        public string Goal { get; set; } = string.Empty;

        [JsonPropertyName("lines")]
        public List<ReorderLine> Lines { get; set; } = [];

        [JsonPropertyName("correctOrders")]
        public List<List<string>> CorrectOrders { get; set; } = [];
    }

    private sealed class ReorderLine
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("indent")]
        public int Indent { get; set; }
    }
}
