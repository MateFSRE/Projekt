using System.Text.Json;
using CodeGames.Exceptions;
using CodeGames.Models;

namespace CodeGames.Services.Challenges;

public class MultipleChoiceHandler : IChallengeHandler
{
    public ChallengeType Type => ChallengeType.MultipleChoice;

    public object BuildPublicView(Challenge challenge)
    {
        return new
        {
            question = challenge.Question,
            codeSnippet = challenge.CodeSnippet,
            options = challenge.Options
                .OrderBy(option => option.Id)
                .Select(option => new { id = option.Id, text = option.Text })
                .ToList()
        };
    }

    public EvaluationResult Evaluate(Challenge challenge, JsonElement answer)
    {
        if (!TryReadOptionId(answer, out var optionId))
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Odaberi jedan odgovor.");
        }

        var selected = challenge.Options.FirstOrDefault(option => option.Id == optionId);
        if (selected is null)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Odabrana opcija ne pripada zadatku.");
        }

        var correctOption = challenge.Options.First(option => option.IsCorrect);

        return new EvaluationResult
        {
            IsCorrect = selected.IsCorrect,
            ScoreFraction = selected.IsCorrect ? 1d : 0d,
            Details = new { selectedOptionId = selected.Id, selectedText = selected.Text },
            RevealedSolution = new { optionId = correctOption.Id, text = correctOption.Text }
        };
    }

    public void ValidateConfig(Challenge challenge)
    {
        if (string.IsNullOrWhiteSpace(challenge.Question))
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Odabir odgovora treba pitanje.");
        }

        if (challenge.Options.Count < 2)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Odabir odgovora treba barem dvije opcije.");
        }

        if (challenge.Options.Any(option => string.IsNullOrWhiteSpace(option.Text)))
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Svaka opcija treba tekst.");
        }

        if (challenge.Options.Count(option => option.IsCorrect) != 1)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Odabir odgovora treba točno jednu točnu opciju.");
        }
    }

    private static bool TryReadOptionId(JsonElement answer, out int optionId)
    {
        optionId = 0;
        if (answer.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (!answer.TryGetProperty("optionId", out var property))
        {
            return false;
        }

        return property.TryGetInt32(out optionId);
    }
}
