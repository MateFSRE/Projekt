using System.Text.Json;
using CodeGames.Exceptions;
using CodeGames.Models;

namespace CodeGames.Services.Challenges;

public class FixBugHandler : IChallengeHandler
{
    public ChallengeType Type => ChallengeType.FixBug;

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
            throw new AppException(StatusCodes.Status400BadRequest, "Odaberi ispravak.");
        }

        var selected = challenge.Options.FirstOrDefault(option => option.Id == optionId);
        if (selected is null)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Odabrani ispravak ne pripada zadatku.");
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
        if (string.IsNullOrWhiteSpace(challenge.CodeSnippet))
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Ispravljanje greške treba isječak koda.");
        }

        if (string.IsNullOrWhiteSpace(challenge.Question))
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Ispravljanje greške treba pitanje.");
        }

        if (challenge.Options.Count < 2)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Ispravljanje greške treba barem dva ispravka.");
        }

        if (challenge.Options.Any(option => string.IsNullOrWhiteSpace(option.Text)))
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Svaki ispravak treba tekst.");
        }

        if (challenge.Options.Count(option => option.IsCorrect) != 1)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Ispravljanje greške treba točno jedan točan ispravak.");
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
