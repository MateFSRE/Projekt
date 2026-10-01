using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;

namespace CodeGames.Tests;

[CollectionDefinition("api")]
public class ApiCollection : ICollectionFixture<ApiFactory>;

[Collection("api")]
public class ApiRouteTests
{
    private static readonly string[] SecretMarkers =
    [
        "hasBug",
        "bugLine",
        "correctOrders",
        "\"expected\"",
        "configJson",
        "correctAnswer",
        "isCorrect"
    ];

    private readonly ApiFactory _factory;

    public ApiRouteTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task RegisterAndLogin_ReturnTheUserAndKeepTheCookie()
    {
        using var client = _factory.CreateCookieClient();
        var stamp = Guid.NewGuid().ToString("N")[..10];

        var registered = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = $"{stamp}@example.com",
            username = $"u_{stamp}",
            password = "lozinka123"
        });
        Assert.Equal(HttpStatusCode.OK, registered.StatusCode);

        var me = await client.GetAsync("/api/auth/me");
        var body = await me.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
        Assert.Contains($"u_{stamp}", body);

        var logout = await client.PostAsync("/api/auth/logout", null);
        Assert.Equal(HttpStatusCode.OK, logout.StatusCode);

        var loggedIn = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = $"{stamp}@example.com",
            password = "lozinka123"
        });
        Assert.Equal(HttpStatusCode.OK, loggedIn.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/auth/me")).StatusCode);
    }

    [Fact]
    public async Task ProtectedRoutes_Return401WithoutACookieAnd403ForAPlayer()
    {
        using var guest = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/challenges/next");
        request.Headers.Add("Origin", "http://localhost:5173");
        var next = await guest.SendAsync(request);
        var admin = await guest.GetAsync("/api/admin/users");
        Assert.Equal(HttpStatusCode.Unauthorized, next.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, admin.StatusCode);
        Assert.Contains("Niste prijavljeni.", await next.Content.ReadAsStringAsync());
        Assert.Equal("nosniff", next.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("http://localhost:5173", next.Headers.GetValues("Access-Control-Allow-Origin").Single());

        using var player = _factory.CreateCookieClient();
        var stamp = Guid.NewGuid().ToString("N")[..10];
        var registered = await player.PostAsJsonAsync("/api/auth/register", new
        {
            email = $"{stamp}@example.com",
            username = $"p_{stamp}",
            password = "lozinka123"
        });
        Assert.Equal(HttpStatusCode.OK, registered.StatusCode);

        var forbidden = await player.GetAsync("/api/admin/users");
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
        Assert.Contains("Nemate dozvolu", await forbidden.Content.ReadAsStringAsync());

        var profile = await player.GetAsync("/api/profile/stats");
        var profileBody = await profile.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, profile.StatusCode);
        using (var profileJson = JsonDocument.Parse(profileBody))
        {
            Assert.Equal(10, profileJson.RootElement.GetProperty("awards").GetArrayLength());
            Assert.Equal(0, profileJson.RootElement.GetProperty("favoritePlays").GetInt32());
            Assert.Equal(JsonValueKind.Null, profileJson.RootElement.GetProperty("favoriteType").ValueKind);
        }
    }

    [Fact]
    public async Task Admin_CanOpenThePingRoute()
    {
        using var client = _factory.CreateCookieClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@codegames.local",
            password = ApiFactory.AdminPassword
        });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var ping = await client.GetAsync("/api/admin/ping");
        Assert.Equal(HttpStatusCode.OK, ping.StatusCode);
        Assert.Contains("Admin pristup je dozvoljen.", await ping.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task NextAndDemo_HideSecretsUntilTheAnswer()
    {
        using var player = _factory.CreateCookieClient();
        var stamp = Guid.NewGuid().ToString("N")[..10];
        Assert.Equal(HttpStatusCode.OK, (await player.PostAsJsonAsync("/api/auth/register", new
        {
            email = $"{stamp}@example.com",
            username = $"n_{stamp}",
            password = "lozinka123"
        })).StatusCode);

        var next = await player.GetStringAsync("/api/challenges/next");
        AssertNoSecrets(next);

        using var guest = _factory.CreateClient();
        var demo = await guest.GetStringAsync("/api/demo/challenge");
        AssertNoSecrets(demo);

        var revealed = await guest.PostAsJsonAsync("/api/demo/answer", new
        {
            answer = new { hasBug = true }
        });
        var revealedBody = await revealed.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, revealed.StatusCode);
        Assert.Contains("hasBug", revealedBody, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("explanation", revealedBody, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Answer_ScoresAKnownChallengeAndRejectsAHugeBody()
    {
        using var client = _factory.CreateCookieClient();
        var stamp = Guid.NewGuid().ToString("N")[..10];
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = $"{stamp}@example.com",
            username = $"a_{stamp}",
            password = "lozinka123"
        })).StatusCode);

        var correct = await client.PostAsJsonAsync("/api/challenges/1/answer", new { answer = "5" });
        var body = await correct.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, correct.StatusCode);
        Assert.Contains("\"isCorrect\":true", body, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("correctAnswer", body, StringComparison.OrdinalIgnoreCase);
        using (var answerJson = JsonDocument.Parse(body))
        {
            Assert.Equal(0, answerJson.RootElement.GetProperty("newAwards").GetArrayLength());
        }

        var longAnswer = new string('x', 4001);
        var tooLong = await client.PostAsJsonAsync("/api/challenges/1/answer", new { answer = longAnswer });
        Assert.Equal(HttpStatusCode.BadRequest, tooLong.StatusCode);
        Assert.Contains("predug", await tooLong.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);

        var huge = new StringContent(
            "{\"answer\":\"" + new string('y', 20_000) + "\"}",
            Encoding.UTF8,
            "application/json");
        var rejected = await client.PostAsync("/api/challenges/1/answer", huge);
        Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
        Assert.Contains("predug", await rejected.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertNoSecrets(string json)
    {
        foreach (var marker in SecretMarkers)
        {
            Assert.DoesNotContain(marker, json, StringComparison.OrdinalIgnoreCase);
        }
    }
}
