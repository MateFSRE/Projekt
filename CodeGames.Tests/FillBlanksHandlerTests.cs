using System.Text.Json;
using CodeGames.Data;
using CodeGames.Exceptions;
using CodeGames.Services.Challenges;
using Xunit;

namespace CodeGames.Tests;

public class FillBlanksHandlerTests
{
    private readonly FillBlanksHandler _handler = new();

    [Fact]
    public void PublicView_HidesBlankAnswersAndShufflesTheSameWords()
    {
        var challenge = ChallengeSeed.FillChallenges[0];
        var json = JsonSerializer.Serialize(_handler.BuildPublicView(challenge));
        using var document = JsonDocument.Parse(json);
        var names = document.RootElement.EnumerateObject().Select(property => property.Name).OrderBy(name => name).ToArray();

        Assert.Equal(["language", "template", "wordBank"], names);

        var shown = document.RootElement.GetProperty("wordBank").EnumerateArray().Select(item => item.GetString()).OrderBy(word => word).ToArray();
        var expected = new[] { "for", "in", "while", "if", "def" }.OrderBy(word => word).ToArray();
        Assert.Equal(expected, shown);
    }

    [Fact]
    public void Seed_HasSixValidExamplesWithoutLeakingAnswers()
    {
        Assert.Equal(6, ChallengeSeed.FillChallenges.Count);

        foreach (var challenge in ChallengeSeed.FillChallenges)
        {
            _handler.ValidateConfig(challenge);
            var json = JsonSerializer.Serialize(_handler.BuildPublicView(challenge));
            using var document = JsonDocument.Parse(json);
            var names = document.RootElement.EnumerateObject().Select(property => property.Name);
            Assert.DoesNotContain("blanks", names);
            Assert.DoesNotContain("answer", names);
        }
    }

    [Fact]
    public void Evaluate_AllCorrect_IsFullyCorrect()
    {
        var challenge = ChallengeSeed.FillChallenges.Single(item => item.Id == 24);
        using var answer = JsonDocument.Parse("{\"1\":\"for\",\"2\":\"in\"}");

        var result = _handler.Evaluate(challenge, answer.RootElement);
        var revealed = JsonSerializer.Serialize(result.RevealedSolution);

        Assert.True(result.IsCorrect);
        Assert.Equal(1d, result.ScoreFraction);
        Assert.Contains("\"answer\":\"for\"", revealed);
        Assert.Contains("\"answer\":\"in\"", revealed);
    }

    [Fact]
    public void Evaluate_Partial_ScoresOnlyTheCorrectBlanks()
    {
        var challenge = ChallengeSeed.FillChallenges.Single(item => item.Id == 24);
        using var answer = JsonDocument.Parse("{\"1\":\"for\",\"2\":\"while\"}");

        var result = _handler.Evaluate(challenge, answer.RootElement);
        var details = JsonSerializer.Serialize(result.Details);

        Assert.False(result.IsCorrect);
        Assert.Equal(0.5d, result.ScoreFraction);
        Assert.Contains("\"id\":\"1\",\"correct\":true", details);
        Assert.Contains("\"id\":\"2\",\"correct\":false", details);
    }

    [Fact]
    public void Evaluate_EmptyMap_ScoresZeroAndMarksEveryBlankWrong()
    {
        var challenge = ChallengeSeed.FillChallenges.Single(item => item.Id == 26);
        using var answer = JsonDocument.Parse("{}");

        var result = _handler.Evaluate(challenge, answer.RootElement);
        var details = JsonSerializer.Serialize(result.Details);

        Assert.False(result.IsCorrect);
        Assert.Equal(0d, result.ScoreFraction);
        Assert.DoesNotContain("\"correct\":true", details);
        Assert.Equal(2, details.Split("\"correct\":false").Length - 1);
    }

    [Fact]
    public void Evaluate_RejectsAnswerThatIsNotAMap()
    {
        var challenge = ChallengeSeed.FillChallenges[0];
        using var answer = JsonDocument.Parse("\"for\"");

        var error = Assert.Throws<AppException>(() => _handler.Evaluate(challenge, answer.RootElement));

        Assert.Equal(400, error.StatusCode);
    }
}
