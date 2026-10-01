using System.Text.Json;
using CodeGames.Data;
using CodeGames.Exceptions;
using CodeGames.Services.Challenges;
using Xunit;

namespace CodeGames.Tests;

public class ReorderCodeHandlerTests
{
    private readonly ReorderCodeHandler _handler = new();

    [Fact]
    public void PublicView_ShufflesLinesAndHidesCorrectOrders()
    {
        var challenge = ChallengeSeed.ReorderChallenges[0];
        var json = JsonSerializer.Serialize(_handler.BuildPublicView(challenge));
        using var document = JsonDocument.Parse(json);
        var names = document.RootElement.EnumerateObject().Select(property => property.Name).OrderBy(name => name).ToArray();

        Assert.Equal(["goal", "language", "lines"], names);
        Assert.DoesNotContain("correctOrders", names);
        Assert.Equal("Ispiši brojeve od 1 do 3.", document.RootElement.GetProperty("goal").GetString());

        var shown = document.RootElement.GetProperty("lines").EnumerateArray().Select(line => line.GetProperty("id").GetString()!).OrderBy(id => id).ToArray();
        Assert.Equal(["1", "2", "3"], shown);
        Assert.Equal(4, document.RootElement.GetProperty("lines").EnumerateArray().Single(line => line.GetProperty("id").GetString() == "3").GetProperty("indent").GetInt32());
    }

    [Fact]
    public void Seed_HasSixValidExamples()
    {
        Assert.Equal(6, ChallengeSeed.ReorderChallenges.Count);
        Assert.Equal([3, 4, 5, 6, 7, 8], ChallengeSeed.ReorderChallenges.Select(challenge =>
        {
            using var document = JsonDocument.Parse(challenge.ConfigJson!);
            return document.RootElement.GetProperty("lines").GetArrayLength();
        }).ToArray());

        foreach (var challenge in ChallengeSeed.ReorderChallenges)
        {
            _handler.ValidateConfig(challenge);
            var json = JsonSerializer.Serialize(_handler.BuildPublicView(challenge));
            using var document = JsonDocument.Parse(json);
            var names = document.RootElement.EnumerateObject().Select(property => property.Name);
            Assert.DoesNotContain("correctOrders", names);
        }
    }

    [Fact]
    public void Evaluate_CorrectOrder_IsFullyCorrect()
    {
        var challenge = ChallengeSeed.ReorderChallenges.Single(item => item.Id == 30);
        using var answer = JsonDocument.Parse("[\"1\",\"2\",\"3\"]");

        var result = _handler.Evaluate(challenge, answer.RootElement);
        var revealed = JsonSerializer.Serialize(result.RevealedSolution);

        Assert.True(result.IsCorrect);
        Assert.Equal(1d, result.ScoreFraction);
        Assert.Contains("    print(broj)", revealed);
        Assert.DoesNotContain("\"wrongLineIds\":[\"", JsonSerializer.Serialize(result.Details));
    }

    [Fact]
    public void Evaluate_AlternativeOrder_IsAccepted()
    {
        var challenge = ChallengeSeed.ReorderChallenges.Single(item => item.Id == 31);
        using var answer = JsonDocument.Parse("[\"2\",\"1\",\"3\",\"4\"]");

        var result = _handler.Evaluate(challenge, answer.RootElement);
        var revealed = JsonSerializer.Serialize(result.RevealedSolution);

        Assert.True(result.IsCorrect);
        Assert.Equal(1d, result.ScoreFraction);
        Assert.Contains("const b = 3;", revealed);
    }

    [Fact]
    public void Evaluate_WrongOrder_MarksLinesThatAreNotInTheClosestSolution()
    {
        var challenge = ChallengeSeed.ReorderChallenges.Single(item => item.Id == 31);
        using var answer = JsonDocument.Parse("[\"1\",\"2\",\"4\",\"3\"]");

        var result = _handler.Evaluate(challenge, answer.RootElement);
        var details = JsonSerializer.Serialize(result.Details);
        var revealed = JsonSerializer.Serialize(result.RevealedSolution);

        Assert.False(result.IsCorrect);
        Assert.Equal(0.5d, result.ScoreFraction);
        Assert.Contains("\"4\"", details);
        Assert.Contains("\"3\"", details);
        Assert.DoesNotContain("\"1\"", details);
        Assert.Contains("\"lineIds\":[\"1\",\"2\",\"3\",\"4\"]", revealed);
    }

    [Fact]
    public void Evaluate_RejectsAnswerThatIsNotAList()
    {
        var challenge = ChallengeSeed.ReorderChallenges[0];
        using var answer = JsonDocument.Parse("\"1\"");

        var error = Assert.Throws<AppException>(() => _handler.Evaluate(challenge, answer.RootElement));

        Assert.Equal(400, error.StatusCode);
    }
}
