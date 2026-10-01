using System.Text.Json;
using CodeGames.Controllers;
using CodeGames.Data;
using CodeGames.Models;
using CodeGames.Services.Challenges;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CodeGames.Tests;

public class PublicViewTests
{
    [Fact]
    public void SeededMiniGames_DoNotExposeSecretConfigKeys()
    {
        AssertHidden(new SwipeBugHandler(), ChallengeSeed.SwipeChallenges, "hasBug", "bugLine");
        AssertHidden(new FillBlanksHandler(), ChallengeSeed.FillChallenges, "blanks", "\"answer\"");
        AssertHidden(new ReorderCodeHandler(), ChallengeSeed.ReorderChallenges, "correctOrders");
        AssertHidden(new TraceExecutionHandler(), ChallengeSeed.TraceChallenges, "expected");
    }

    [Fact]
    public void AdminControllers_RequireTheAdminRole()
    {
        var controllers = typeof(Program).Assembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && type.Name.StartsWith("Admin", StringComparison.Ordinal))
            .Where(type => typeof(ControllerBase).IsAssignableFrom(type))
            .ToList();

        Assert.NotEmpty(controllers);
        foreach (var controller in controllers)
        {
            var roles = controller.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .Cast<AuthorizeAttribute>()
                .Select(attribute => attribute.Roles);
            Assert.Contains("Admin", roles);
        }

        Assert.NotNull(typeof(ChallengesController).GetMethod("Answer")!
            .GetCustomAttributes(typeof(RequestSizeLimitAttribute), inherit: false)
            .SingleOrDefault());
        Assert.NotNull(typeof(DemoController).GetMethod("Answer")!
            .GetCustomAttributes(typeof(RequestSizeLimitAttribute), inherit: false)
            .SingleOrDefault());
    }

    private static void AssertHidden(IChallengeHandler handler, IEnumerable<Challenge> challenges, params string[] secrets)
    {
        foreach (var challenge in challenges)
        {
            handler.ValidateConfig(challenge);
            var json = JsonSerializer.Serialize(handler.BuildPublicView(challenge));
            foreach (var secret in secrets)
            {
                Assert.DoesNotContain(secret, json, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
