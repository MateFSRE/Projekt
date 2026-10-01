using CodeGames.Models;
using CodeGames.Services;
using Xunit;

namespace CodeGames.Tests;

public class AwardCatalogTests
{
    [Theory]
    [InlineData(0, "")]
    [InlineData(4, "")]
    [InlineData(5, "5")]
    [InlineData(12, "5,10")]
    [InlineData(100, "5,10,25,50,100")]
    [InlineData(250, "5,10,25,50,100,200")]
    [InlineData(300, "5,10,25,50,100,200,300")]
    public void Crossed_FromZero_IncludesEveryReachedThreshold(int after, string expected)
    {
        var crossed = string.Join(',', AwardCatalog.Crossed(0, after));
        Assert.Equal(expected, crossed);
    }

    [Fact]
    public void Crossed_SkipsThresholdsAlreadyPassed()
    {
        Assert.Equal([10], AwardCatalog.Crossed(5, 10));
        Assert.Equal(Array.Empty<int>(), AwardCatalog.Crossed(10, 10));
        Assert.Equal(Array.Empty<int>(), AwardCatalog.Crossed(12, 8));
    }

    [Fact]
    public void Crossed_AfterOneHundred_StepsByOneHundred()
    {
        Assert.Equal([200, 300], AwardCatalog.Crossed(150, 350));
        Assert.Equal([200], AwardCatalog.Crossed(100, 200));
    }

    [Fact]
    public void VisibleThresholds_ShowTheBaseSetAndTheNextStep()
    {
        Assert.Equal([5, 10, 25, 50, 100], AwardCatalog.VisibleThresholds(0));
        Assert.Equal([5, 10, 25, 50, 100, 200], AwardCatalog.VisibleThresholds(100));
        Assert.Equal([5, 10, 25, 50, 100, 200, 300], AwardCatalog.VisibleThresholds(200));
    }

    [Fact]
    public void NewlyEarned_ReportsLevelAndStreakSeparately()
    {
        var awards = AwardCatalog.NewlyEarned(4, 10, 3, 5);

        Assert.Equal(3, awards.Count);
        Assert.Contains(awards, award => award.Kind == AwardKind.Level && award.Threshold == 5 && award.Earned);
        Assert.Contains(awards, award => award.Kind == AwardKind.Level && award.Threshold == 10 && award.Earned);
        Assert.Contains(awards, award => award.Kind == AwardKind.Streak && award.Threshold == 5 && award.Earned);
    }

    [Fact]
    public void ProfileAwards_KeepEarlierMilestonesWhenProgressDropsBelowThem()
    {
        var awards = AwardCatalog.ProfileAwards(level: 6, bestStreak: 10);

        Assert.Contains(awards, award => award.Kind == AwardKind.Level && award.Threshold == 5 && award.Earned);
        Assert.Contains(awards, award => award.Kind == AwardKind.Level && award.Threshold == 10 && !award.Earned);
        Assert.Contains(awards, award => award.Kind == AwardKind.Streak && award.Threshold == 10 && award.Earned);
        Assert.Contains(awards, award => award.Kind == AwardKind.Streak && award.Threshold == 25 && !award.Earned);
        Assert.Equal(10, awards.Count);
    }
}
