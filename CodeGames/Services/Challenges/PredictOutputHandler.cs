using System.Text.Json;
using System.Text.RegularExpressions;
using CodeGames.Exceptions;
using CodeGames.Models;

namespace CodeGames.Services.Challenges;

public class PredictOutputHandler : IChallengeHandler
{
    private static readonly Regex RepeatedSpaces = new("[ \t]{2,}", RegexOptions.Compiled);

    public ChallengeType Type => ChallengeType.PredictOutput;

    public object BuildPublicView(Challenge challenge)
    {
        return new
        {
            question = challenge.Question,
            codeSnippet = challenge.CodeSnippet
        };
    }

    public EvaluationResult Evaluate(Challenge challenge, JsonElement answer)
    {
        if (answer.ValueKind != JsonValueKind.String)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Odgovor mora biti tekst.");
        }

        var submitted = answer.GetString() ?? string.Empty;
        var normalizedSubmitted = Normalize(submitted);
        var normalizedExpected = Normalize(challenge.CorrectAnswer);
        var correct = normalizedSubmitted == normalizedExpected;

        return new EvaluationResult
        {
            IsCorrect = correct,
            ScoreFraction = correct ? 1d : 0d,
            Details = new { normalizedAnswer = normalizedSubmitted },
            RevealedSolution = new { correctAnswer = challenge.CorrectAnswer }
        };
    }

    public void ValidateConfig(Challenge challenge)
    {
        if (string.IsNullOrWhiteSpace(challenge.CodeSnippet))
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Predviđanje ispisa treba isječak koda.");
        }

        if (string.IsNullOrWhiteSpace(challenge.Question))
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Predviđanje ispisa treba pitanje.");
        }

        if (string.IsNullOrWhiteSpace(challenge.CorrectAnswer))
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Predviđanje ispisa treba točan ispis.");
        }
    }

    // Spaja završetke redaka, reže razmake na krajevima linija i odbacuje prazne retke na kraju.
    internal static string Normalize(string value)
    {
        var unified = value.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').Trim();
        if (unified.Length == 0)
        {
            return string.Empty;
        }

        var lines = unified
            .Split('\n')
            .Select(line => RepeatedSpaces.Replace(line.Trim(), " "));

        return string.Join('\n', lines);
    }
}
