using System.Text.Json;
using CodeGames.Data;
using CodeGames.Exceptions;
using CodeGames.Services.Challenges;
using Xunit;

namespace CodeGames.Tests;

public class TraceExecutionHandlerTests
{
    private readonly TraceExecutionHandler _handler = new();

    [Fact]
    public void PublicView_HidesExpectedValues()
    {
        var challenge = ChallengeSeed.TraceChallenges[0];
        var json = JsonSerializer.Serialize(_handler.BuildPublicView(challenge));
        using var document = JsonDocument.Parse(json);
        var names = document.RootElement.EnumerateObject().Select(property => property.Name).OrderBy(name => name).ToArray();

        Assert.Equal(["askAt", "code", "language", "steps", "variables"], names);
        Assert.DoesNotContain("expected", json);

        var stepNames = document.RootElement.GetProperty("steps")[0].EnumerateObject().Select(property => property.Name).ToArray();
        Assert.Equal(["line"], stepNames);
        Assert.Equal(["x", "i"], document.RootElement.GetProperty("variables").EnumerateArray().Select(item => item.GetString()!).ToArray());
    }

    [Fact]
    public void Seed_HasFourValidExamples()
    {
        Assert.Equal(4, ChallengeSeed.TraceChallenges.Count);

        foreach (var challenge in ChallengeSeed.TraceChallenges)
        {
            _handler.ValidateConfig(challenge);
            var json = JsonSerializer.Serialize(_handler.BuildPublicView(challenge));
            Assert.DoesNotContain("expected", json);
        }
    }

    [Fact]
    public void Evaluate_AllCorrect_AcceptsTrimmedText()
    {
        var challenge = ChallengeSeed.TraceChallenges.Single(item => item.Id == 36);
        using var answer = JsonDocument.Parse("""
            {
              "3": { "x": " 1 ", "i": "1" },
              "6": { "x": "3", "i": "2" },
              "7": { "x": "3", "i": "3" }
            }
            """);

        var result = _handler.Evaluate(challenge, answer.RootElement);
        var revealed = JsonSerializer.Serialize(result.RevealedSolution);

        Assert.True(result.IsCorrect);
        Assert.Equal(1d, result.ScoreFraction);
        Assert.Contains("x=1", revealed);
        Assert.Contains("i=3", revealed);
    }

    [Fact]
    public void Evaluate_Partial_ScoresOnlyTheCorrectCells()
    {
        var challenge = ChallengeSeed.TraceChallenges.Single(item => item.Id == 36);
        using var answer = JsonDocument.Parse("""
            {
              "3": { "x": "9", "i": "1" },
              "6": { "x": "3", "i": "2" },
              "7": { "x": "3", "i": "3" }
            }
            """);

        var result = _handler.Evaluate(challenge, answer.RootElement);
        var details = JsonSerializer.Serialize(result.Details);

        Assert.False(result.IsCorrect);
        Assert.Equal(5d / 6d, result.ScoreFraction);
        Assert.Contains("\"stepIndex\":3,\"name\":\"x\",\"correct\":false", details);
        Assert.Contains("\"stepIndex\":3,\"name\":\"i\",\"correct\":true", details);
    }

    [Fact]
    public void Evaluate_EmptyMap_ScoresZero()
    {
        var challenge = ChallengeSeed.TraceChallenges.Single(item => item.Id == 37);
        using var answer = JsonDocument.Parse("{}");

        var result = _handler.Evaluate(challenge, answer.RootElement);
        var details = JsonSerializer.Serialize(result.Details);

        Assert.False(result.IsCorrect);
        Assert.Equal(0d, result.ScoreFraction);
        Assert.DoesNotContain("\"correct\":true", details);
        Assert.Equal(9, details.Split("\"correct\":false").Length - 1);
    }

    [Fact]
    public void Evaluate_RejectsAnswerThatIsNotAMap()
    {
        var challenge = ChallengeSeed.TraceChallenges[0];
        using var answer = JsonDocument.Parse("[]");

        var error = Assert.Throws<AppException>(() => _handler.Evaluate(challenge, answer.RootElement));

        Assert.Equal(400, error.StatusCode);
    }
}
