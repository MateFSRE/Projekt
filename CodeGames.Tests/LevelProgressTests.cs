using CodeGames.Services;
using Xunit;

namespace CodeGames.Tests;

public class LevelProgressTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(50, 50)]
    [InlineData(99, 99)]
    [InlineData(100, 0)]
    [InlineData(250, 0)]
    [InlineData(8000, 100)]
    [InlineData(9000, 100)]
    public void ProgressPercent_FollowsTheThresholds(int xp, int percent)
    {
        Assert.Equal(percent, GamificationService.ProgressPercent(xp));
    }
}
