using System.Text.Json;
using CodeGames.Data;
using CodeGames.Exceptions;
using CodeGames.Services.Challenges;
using Xunit;

namespace CodeGames.Tests;

public class SwipeBugHandlerTests
{
    private readonly SwipeBugHandler _handler = new();

    [Fact]
    public void PublicView_ContainsOnlyCodeAndLanguage()
    {
        var challenge = ChallengeSeed.SwipeChallenges[0];
        var json = JsonSerializer.Serialize(_handler.BuildPublicView(challenge));

        Assert.Contains("broj = 4", json);
        Assert.Contains("Python", json);
        Assert.DoesNotContain("hasBug", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("bugLine", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Seed_HasFourBugsAndFourCleanExamplesInBothLanguages()
    {
        var examples = ChallengeSeed.SwipeChallenges;

        Assert.Equal(8, examples.Count);
        Assert.Equal(4, examples.Count(item => item.Language == "Python"));
        Assert.Equal(4, examples.Count(item => item.Language == "JavaScript"));
        Assert.Equal(4, examples.Count(item => ReadsHasBug(item.ConfigJson)));
        Assert.Equal(4, examples.Count(item => !ReadsHasBug(item.ConfigJson)));

        foreach (var challenge in examples)
        {
            _handler.ValidateConfig(challenge);
            var json = JsonSerializer.Serialize(_handler.BuildPublicView(challenge));
            Assert.DoesNotContain("hasBug", json, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("bugLine", json, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Evaluate_AcceptsMatchingBugAnswerAndRevealsLine()
    {
        var challenge = ChallengeSeed.SwipeChallenges.Single(item => item.Id == 16);
        using var answer = JsonDocument.Parse("{\"hasBug\":true}");

        var result = _handler.Evaluate(challenge, answer.RootElement);
        var revealed = JsonSerializer.Serialize(result.RevealedSolution);

        Assert.True(result.IsCorrect);
        Assert.Equal(1d, result.ScoreFraction);
        Assert.Contains("\"bugLine\":2", revealed);
    }

    [Fact]
    public void Evaluate_AcceptsCleanCodeAndRevealsNullLine()
    {
        var challenge = ChallengeSeed.SwipeChallenges.Single(item => item.Id == 22);
        using var answer = JsonDocument.Parse("{\"hasBug\":false}");

        var result = _handler.Evaluate(challenge, answer.RootElement);
        var revealed = JsonSerializer.Serialize(result.RevealedSolution);

        Assert.True(result.IsCorrect);
        Assert.Contains("\"bugLine\":null", revealed);
    }

    [Fact]
    public void Evaluate_WrongAnswerStillRevealsBugLine()
    {
        var challenge = ChallengeSeed.SwipeChallenges.Single(item => item.Id == 17);
        using var answer = JsonDocument.Parse("{\"hasBug\":false}");

        var result = _handler.Evaluate(challenge, answer.RootElement);
        var revealed = JsonSerializer.Serialize(result.RevealedSolution);

        Assert.False(result.IsCorrect);
        Assert.Equal(0d, result.ScoreFraction);
        Assert.Contains("\"hasBug\":true", revealed);
        Assert.Contains("\"bugLine\":5", revealed);
    }

    [Fact]
    public void Evaluate_RejectsAnswerThatIsNotBoolean()
    {
        var challenge = ChallengeSeed.SwipeChallenges[0];
        using var answer = JsonDocument.Parse("{\"hasBug\":\"true\"}");

        var error = Assert.Throws<AppException>(() => _handler.Evaluate(challenge, answer.RootElement));

        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public void ValidateConfig_RejectsCodeOutsideFourToEightLines()
    {
        var tooShort = Make("a = 1\nb = 2\nprint(a + b)", false, null);

        var error = Assert.Throws<AppException>(() => _handler.ValidateConfig(tooShort));

        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public void ValidateConfig_RejectsBugWithoutLineAndCleanCodeWithLine()
    {
        var missingLine = Make("a = 1\nb = 2\nc = 3\nprint(a)", true, null);
        var extraLine = Make("a = 1\nb = 2\nc = 3\nprint(a)", false, 2);

        Assert.Throws<AppException>(() => _handler.ValidateConfig(missingLine));
        Assert.Throws<AppException>(() => _handler.ValidateConfig(extraLine));
    }

    private static bool ReadsHasBug(string? configJson)
    {
        using var document = JsonDocument.Parse(configJson!);
        return document.RootElement.GetProperty("hasBug").GetBoolean();
    }

    private static CodeGames.Models.Challenge Make(string code, bool hasBug, int? bugLine)
    {
        var config = JsonSerializer.Serialize(new
        {
            code,
            language = "Python",
            hasBug,
            bugLine
        });

        return new CodeGames.Models.Challenge
        {
            Type = CodeGames.Models.ChallengeType.SwipeBug,
            ConfigJson = config
        };
    }
}
