using System.Text.Json;
using CodeGames.Exceptions;
using CodeGames.Models;
using CodeGames.Services.Challenges;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace CodeGames.Tests;

public class ClassicHandlerTests
{
    [Fact]
    public void PredictOutput_PublicView_HidesTheCorrectPrint()
    {
        var handler = new PredictOutputHandler();
        var challenge = Predict();
        var json = JsonSerializer.Serialize(handler.BuildPublicView(challenge));

        Assert.Contains("x = 2", json);
        Assert.Contains("codeSnippet", json);
        Assert.DoesNotContain("correctAnswer", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"5\"", json);
    }

    [Fact]
    public void PredictOutput_Evaluate_AcceptsWhitespaceAndRevealsAnswer()
    {
        var handler = new PredictOutputHandler();
        using var answer = JsonDocument.Parse("\"  5  \"");

        var result = handler.Evaluate(Predict(), answer.RootElement);
        var revealed = JsonSerializer.Serialize(result.RevealedSolution);

        Assert.True(result.IsCorrect);
        Assert.Equal(1d, result.ScoreFraction);
        Assert.Contains("5", revealed);
    }

    [Fact]
    public void PredictOutput_Evaluate_WrongAnswerStillRevealsPrint()
    {
        var handler = new PredictOutputHandler();
        using var answer = JsonDocument.Parse("\"9\"");

        var result = handler.Evaluate(Predict(), answer.RootElement);

        Assert.False(result.IsCorrect);
        Assert.Equal(0d, result.ScoreFraction);
        Assert.Contains("5", JsonSerializer.Serialize(result.RevealedSolution));
    }

    [Fact]
    public void PredictOutput_ValidateConfig_RequiresThePrint()
    {
        var handler = new PredictOutputHandler();
        var challenge = Predict();
        challenge.CorrectAnswer = " ";

        var error = Assert.Throws<AppException>(() => handler.ValidateConfig(challenge));

        Assert.Equal(StatusCodes.Status400BadRequest, error.StatusCode);
    }

    [Fact]
    public void MultipleChoice_PublicView_HidesWhichOptionIsCorrect()
    {
        var handler = new MultipleChoiceHandler();
        var json = JsonSerializer.Serialize(handler.BuildPublicView(Choice()));

        Assert.Contains("3", json);
        Assert.Contains("4", json);
        Assert.DoesNotContain("isCorrect", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("correctAnswer", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MultipleChoice_Evaluate_AcceptsTheCorrectOption()
    {
        var handler = new MultipleChoiceHandler();
        using var answer = JsonDocument.Parse("{\"optionId\":1}");

        var result = handler.Evaluate(Choice(), answer.RootElement);
        var revealed = JsonSerializer.Serialize(result.RevealedSolution);

        Assert.True(result.IsCorrect);
        Assert.Contains("\"optionId\":1", revealed);
    }

    [Fact]
    public void MultipleChoice_Evaluate_WrongOptionStillRevealsTheCorrectOne()
    {
        var handler = new MultipleChoiceHandler();
        using var answer = JsonDocument.Parse("{\"optionId\":2}");

        var result = handler.Evaluate(Choice(), answer.RootElement);

        Assert.False(result.IsCorrect);
        Assert.Contains("\"optionId\":1", JsonSerializer.Serialize(result.RevealedSolution));
    }

    [Fact]
    public void FixBug_PublicView_HidesWhichFixIsCorrect()
    {
        var handler = new FixBugHandler();
        var json = JsonSerializer.Serialize(handler.BuildPublicView(Bug()));

        Assert.Contains("if x == 10:", json);
        Assert.DoesNotContain("isCorrect", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("correctAnswer", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FixBug_Evaluate_AcceptsTheCorrectLine()
    {
        var handler = new FixBugHandler();
        using var answer = JsonDocument.Parse("{\"optionId\":10}");

        var result = handler.Evaluate(Bug(), answer.RootElement);

        Assert.True(result.IsCorrect);
        Assert.Equal(1d, result.ScoreFraction);
    }

    [Fact]
    public void FixBug_Evaluate_RejectsAnUnknownOption()
    {
        var handler = new FixBugHandler();
        using var answer = JsonDocument.Parse("{\"optionId\":99}");

        var error = Assert.Throws<AppException>(() => handler.Evaluate(Bug(), answer.RootElement));

        Assert.Equal(StatusCodes.Status400BadRequest, error.StatusCode);
    }

    private static Challenge Predict()
    {
        return new Challenge
        {
            Type = ChallengeType.PredictOutput,
            CodeSnippet = "x = 2\ny = 3\nprint(x + y)",
            Question = "Što program ispisuje?",
            CorrectAnswer = "5",
            Explanation = "Zbroj je 5."
        };
    }

    private static Challenge Choice()
    {
        return new Challenge
        {
            Type = ChallengeType.MultipleChoice,
            CodeSnippet = "print(len(\"kod\"))",
            Question = "Što se ispisuje?",
            CorrectAnswer = "3",
            Options =
            [
                new ChallengeOption { Id = 1, Text = "3", IsCorrect = true },
                new ChallengeOption { Id = 2, Text = "4", IsCorrect = false }
            ]
        };
    }

    private static Challenge Bug()
    {
        return new Challenge
        {
            Type = ChallengeType.FixBug,
            CodeSnippet = "x = 10\nif x = 10:\n    print(\"da\")",
            Question = "Koji redak ispravlja grešku?",
            CorrectAnswer = "if x == 10:",
            Options =
            [
                new ChallengeOption { Id = 10, Text = "if x == 10:", IsCorrect = true },
                new ChallengeOption { Id = 11, Text = "if x = 10:", IsCorrect = false }
            ]
        };
    }
}
