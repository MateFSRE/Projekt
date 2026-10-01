using System.Text.Json;
using CodeGames.Data;
using CodeGames.Exceptions;
using CodeGames.Services.Challenges;
using Xunit;

namespace CodeGames.Tests;

public class RobotMazeHandlerTests
{
    private readonly RobotMazeHandler _handler = new();

    [Fact]
    public void PublicView_SendsTheMapWithoutAStoredSolution()
    {
        var challenge = ChallengeSeed.MazeChallenges[0];
        var json = JsonSerializer.Serialize(_handler.BuildPublicView(challenge));
        using var document = JsonDocument.Parse(json);
        var names = document.RootElement.EnumerateObject().Select(property => property.Name).OrderBy(name => name).ToArray();

        Assert.Equal(
            ["allowedBlocks", "goal", "height", "maxBlocks", "maxSteps", "optimalBlocks", "stars", "start", "walls", "width"],
            names);
        Assert.DoesNotContain("solution", json);
    }

    [Fact]
    public void Seed_HasSixIncreasingLevels()
    {
        Assert.Equal(6, ChallengeSeed.MazeChallenges.Count);
        Assert.Equal([1, 1, 2, 2, 3, 3], ChallengeSeed.MazeChallenges.Select(challenge => challenge.Difficulty).ToArray());

        foreach (var challenge in ChallengeSeed.MazeChallenges)
        {
            _handler.ValidateConfig(challenge);
            var json = JsonSerializer.Serialize(_handler.BuildPublicView(challenge));
            using var document = JsonDocument.Parse(json);
            Assert.True(document.RootElement.GetProperty("stars").GetArrayLength() > 0);
        }
    }

    [Fact]
    public void Evaluate_ForwardIntoAWall_Fails()
    {
        var challenge = ChallengeSeed.MazeChallenges.Single(item => item.Id == 42);
        using var answer = JsonDocument.Parse("""[{"type":"forward"},{"type":"forward"}]""");

        var result = _handler.Evaluate(challenge, answer.RootElement);
        var details = JsonSerializer.Serialize(result.Details);

        Assert.False(result.IsCorrect);
        Assert.Equal(0d, result.ScoreFraction);
        Assert.Contains("\"outcome\":\"hitWall\"", details);
    }

    [Fact]
    public void Evaluate_StraightLine_ReachesTheGoal()
    {
        var challenge = ChallengeSeed.MazeChallenges.Single(item => item.Id == 40);
        using var answer = JsonDocument.Parse("""[{"type":"forward"},{"type":"forward"},{"type":"forward"}]""");

        var result = _handler.Evaluate(challenge, answer.RootElement);
        var details = JsonSerializer.Serialize(result.Details);

        Assert.True(result.IsCorrect);
        Assert.Equal(1d, result.ScoreFraction);
        Assert.Equal(RobotMazeHandler.PointsPerStar, result.BonusPoints);
        Assert.Contains("\"outcome\":\"reachedGoal\"", details);
        Assert.Contains("\"starsCollected\":1", details);
        Assert.Contains("\"blocksUsed\":3", details);
    }

    [Fact]
    public void Evaluate_Repeat_ReachesTheGoalWithTwoBlocks()
    {
        var challenge = ChallengeSeed.MazeChallenges.Single(item => item.Id == 43);
        using var answer = JsonDocument.Parse("""[{"type":"repeat","times":3,"body":[{"type":"forward"}]}]""");

        var result = _handler.Evaluate(challenge, answer.RootElement);
        var details = JsonSerializer.Serialize(result.Details);

        Assert.True(result.IsCorrect);
        Assert.Equal(1d, result.ScoreFraction);
        Assert.Contains("\"blocksUsed\":2", details);
    }

    [Fact]
    public void Evaluate_StarWithoutTheGoal_StaysAPartialMiss()
    {
        var challenge = ChallengeSeed.MazeChallenges.Single(item => item.Id == 40);
        using var answer = JsonDocument.Parse("""[{"type":"forward"}]""");

        var result = _handler.Evaluate(challenge, answer.RootElement);

        Assert.False(result.IsCorrect);
        Assert.Equal(0.99d, result.ScoreFraction);
        Assert.Equal(0, result.BonusPoints);
    }

    [Fact]
    public void Evaluate_TooManySteps_StopsInsteadOfLoopingForever()
    {
        var challenge = ChallengeSeed.MazeChallenges.Single(item => item.Id == 44);
        using var answer = JsonDocument.Parse("""
            [{"type":"repeat","times":10,"body":[{"type":"repeat","times":10,"body":[{"type":"turnLeft"}]}]}]
            """);

        var result = _handler.Evaluate(challenge, answer.RootElement);
        var details = JsonSerializer.Serialize(result.Details);

        Assert.False(result.IsCorrect);
        Assert.Contains("\"outcome\":\"outOfSteps\"", details);
    }

    [Fact]
    public void Evaluate_TooManyBlocks_IsRejected()
    {
        var challenge = ChallengeSeed.MazeChallenges.Single(item => item.Id == 40);
        using var answer = JsonDocument.Parse("""
            [{"type":"forward"},{"type":"forward"},{"type":"forward"},{"type":"forward"},{"type":"forward"}]
            """);

        var error = Assert.Throws<AppException>(() => _handler.Evaluate(challenge, answer.RootElement));

        Assert.Equal(400, error.StatusCode);
    }

    [Fact]
    public void Evaluate_StarsOnTheWay_CountTowardTheFullScore()
    {
        var challenge = ChallengeSeed.MazeChallenges.Single(item => item.Id == 45);
        using var answer = JsonDocument.Parse("""
            [
              {"type":"turnRight"},
              {"type":"forward"},
              {"type":"turnLeft"},
              {"type":"repeat","times":4,"body":[{"type":"forward"}]},
              {"type":"turnLeft"},
              {"type":"forward"}
            ]
            """);

        var result = _handler.Evaluate(challenge, answer.RootElement);
        var details = JsonSerializer.Serialize(result.Details);

        Assert.True(result.IsCorrect);
        Assert.Equal(1d, result.ScoreFraction);
        Assert.Equal(2 * RobotMazeHandler.PointsPerStar, result.BonusPoints);
        Assert.Contains("\"starsCollected\":2", details);
        Assert.Contains("\"blocksUsed\":7", details);
    }
}
