using CodeGames.Exceptions;
using CodeGames.Services;
using Xunit;

namespace CodeGames.Tests;

public class ChallengerRulesTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(9, 0)]
    [InlineData(10, 1)]
    [InlineData(19, 1)]
    [InlineData(20, 2)]
    [InlineData(100, 10)]
    public void Tier_FollowsStepsOfTen(int total, int tier)
    {
        Assert.Equal(tier, ChallengerRules.TierFor(total));
    }

    [Fact]
    public void TierUp_IsTrueOnlyWhenTheThresholdIsCrossed()
    {
        Assert.False(ChallengerRules.TierIncreased(0, 9));
        Assert.True(ChallengerRules.TierIncreased(0, 10));
        Assert.False(ChallengerRules.TierIncreased(1, 19));
        Assert.True(ChallengerRules.TierIncreased(1, 20));
        Assert.Equal(10, ChallengerRules.TierFor(100));
        Assert.Equal("X", ChallengerRules.Roman(10));
    }

    [Fact]
    public void Progress_CountsTowardTheNextTen()
    {
        var atNine = ChallengerRules.Describe(9);
        Assert.Equal(0, atNine.Tier);
        Assert.Equal(9, atNine.Progress);
        Assert.Equal(10, atNine.NextThreshold);

        var atTen = ChallengerRules.Describe(10);
        Assert.Equal(1, atTen.Tier);
        Assert.Equal(0, atTen.Progress);
        Assert.Equal(20, atTen.NextThreshold);
    }
}

public class DailyPickerTests
{
    [Fact]
    public void Pick_RequiresAtLeastThree()
    {
        var error = Assert.Throws<AppException>(() => DailyPicker.Pick([1, 2], [], Random.Shared));
        Assert.Contains("barem 3", error.Message);
    }

    [Fact]
    public void Pick_WithExactlyThree_ReturnsThoseIds()
    {
        var picked = DailyPicker.Pick([7, 8, 9], [7, 8, 9], new Random(1));
        Assert.Equal(3, picked.Distinct().Count());
        Assert.Equal([7, 8, 9], picked.OrderBy(id => id).ToArray());
    }

    [Fact]
    public void Pick_DoesNotRepeatYesterdaysSetWhenAnotherTaskExists()
    {
        var yesterday = new[] { 1, 2, 3 };
        for (var seed = 0; seed < 20; seed++)
        {
            var picked = DailyPicker.Pick([1, 2, 3, 4], yesterday, new Random(seed));
            Assert.Equal(3, picked.Distinct().Count());
            Assert.NotEqual(yesterday.OrderBy(id => id), picked.OrderBy(id => id));
        }
    }
}
