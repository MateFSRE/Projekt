using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CodeGames.Data;
using CodeGames.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CodeGames.Tests;

[Collection("api")]
public class DailyChallengeTests
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

    public DailyChallengeTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Selection_IsThreeStableTasksAndChangesOnTheNextDay()
    {
        await WithPool([1, 2, 3, 4], async () =>
        {
            _factory.Clock.SetUtcNow(new DateTimeOffset(2031, 3, 1, 8, 0, 0, TimeSpan.Zero));
            using var client = await RegisterAsync();

            var first = await BoardAsync(client);
            var again = await BoardAsync(client);
            Assert.Equal(3, first.Count);
            Assert.Equal(first.OrderBy(id => id), again.OrderBy(id => id));

            _factory.Clock.SetUtcNow(new DateTimeOffset(2031, 3, 2, 8, 0, 0, TimeSpan.Zero));
            var nextDay = await BoardAsync(client);
            Assert.Equal(3, nextDay.Count);
            Assert.NotEqual(first.OrderBy(id => id), nextDay.OrderBy(id => id));
        });
    }

    [Fact]
    public async Task Selection_WorksWhenThePoolHasExactlyThree()
    {
        await WithPool([1, 2, 3], async () =>
        {
            _factory.Clock.SetUtcNow(new DateTimeOffset(2031, 4, 1, 8, 0, 0, TimeSpan.Zero));
            using var client = await RegisterAsync();
            var ids = await BoardAsync(client);
            Assert.Equal([1, 2, 3], ids.OrderBy(id => id).ToArray());
        });
    }

    [Fact]
    public async Task Selection_FailsWhenThePoolHasFewerThanThree()
    {
        await WithPool([], async () =>
        {
            _factory.Clock.SetUtcNow(new DateTimeOffset(2031, 5, 1, 8, 0, 0, TimeSpan.Zero));
            using var client = await RegisterAsync();
            var response = await client.GetAsync("/api/daily");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("barem 3", await response.Content.ReadAsStringAsync());
        });
    }

    [Fact]
    public async Task ParallelReads_KeepASingleSelection()
    {
        await WithPool([1, 2, 3, 4, 5], async () =>
        {
            var day = new DateTimeOffset(2031, 6, 1, 9, 0, 0, TimeSpan.Zero);
            _factory.Clock.SetUtcNow(day);
            using var client = await RegisterAsync();
            var calls = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => client.GetAsync("/api/daily")));
            Assert.All(calls, response => Assert.Equal(HttpStatusCode.OK, response.StatusCode));

            await using var scope = _factory.Services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var count = await db.DailySelections.CountAsync(selection => selection.Date == DateOnly.FromDateTime(day.UtcDateTime));
            Assert.Equal(3, count);
        });
    }

    [Fact]
    public async Task Completion_PaysOncePerDay_AndAgainTheNextDay()
    {
        await WithPool([1, 2, 3], async () =>
        {
            _factory.Clock.SetUtcNow(new DateTimeOffset(2031, 7, 1, 10, 0, 0, TimeSpan.Zero));
            using var client = await RegisterAsync();

            var wrong = await AnswerAsync(client, 1, "krivo");
            Assert.False(wrong.GetProperty("isCorrect").GetBoolean());
            Assert.False(wrong.GetProperty("dailyCompleted").GetBoolean());
            Assert.Equal(0, wrong.GetProperty("pointsAwarded").GetInt32());

            var first = await AnswerAsync(client, 1, "5");
            Assert.True(first.GetProperty("isCorrect").GetBoolean());
            Assert.True(first.GetProperty("dailyCompleted").GetBoolean());
            Assert.Equal(10, first.GetProperty("pointsAwarded").GetInt32());
            var xp = first.GetProperty("newXp").GetInt32();

            var repeat = await AnswerAsync(client, 1, "5");
            Assert.Equal(0, repeat.GetProperty("pointsAwarded").GetInt32());
            Assert.Equal(xp, repeat.GetProperty("newXp").GetInt32());
            Assert.False(repeat.GetProperty("challengerTierUp").GetBoolean());

            var outside = await client.PostAsJsonAsync("/api/daily/10/answer", new { answer = "5" });
            Assert.Equal(HttpStatusCode.NotFound, outside.StatusCode);

            var regular = await AnswerAsync(client, "/api/challenges/1/answer", "5");
            Assert.True(regular.GetProperty("pointsAwarded").GetInt32() > 0);

            _factory.Clock.SetUtcNow(new DateTimeOffset(2031, 7, 2, 10, 0, 0, TimeSpan.Zero));
            var nextDay = await AnswerAsync(client, 1, "5");
            Assert.True(nextDay.GetProperty("dailyCompleted").GetBoolean());
            Assert.Equal(10, nextDay.GetProperty("pointsAwarded").GetInt32());
            Assert.Equal(2, nextDay.GetProperty("totalDailyCompleted").GetInt32());
        });
    }

    [Fact]
    public async Task Challenger_TiersUpOnlyOnTheTenthCompletion()
    {
        await WithPool([1, 2, 3], async () =>
        {
            var day = new DateTimeOffset(2031, 8, 1, 11, 0, 0, TimeSpan.Zero);
            _factory.Clock.SetUtcNow(day);
            using var client = await RegisterAsync();
            var me = await client.GetFromJsonAsync<JsonElement>("/api/auth/me");
            var userId = me.GetProperty("id").GetInt32();
            await SeedCompletionsAsync(userId, 9, DateOnly.FromDateTime(day.UtcDateTime).AddDays(-20));

            var tenth = await AnswerAsync(client, 1, "5");
            Assert.True(tenth.GetProperty("challengerTierUp").GetBoolean());
            Assert.Equal(1, tenth.GetProperty("challengerTier").GetInt32());
            Assert.Equal(10, tenth.GetProperty("totalDailyCompleted").GetInt32());

            var eleventh = await AnswerAsync(client, 2, "AnaAna");
            Assert.False(eleventh.GetProperty("challengerTierUp").GetBoolean());
            Assert.Equal(1, eleventh.GetProperty("challengerTier").GetInt32());
        });
    }

    [Fact]
    public async Task RegularGame_ReopensOnTheNextDay()
    {
        _factory.Clock.SetUtcNow(new DateTimeOffset(2032, 1, 1, 8, 0, 0, TimeSpan.Zero));
        await WithActiveOnly([1], async () =>
        {
            using var client = await RegisterAsync();
            var first = await AnswerAsync(client, "/api/challenges/1/answer", "5");
            Assert.True(first.GetProperty("pointsAwarded").GetInt32() > 0);

            var blocked = await client.GetAsync("/api/challenges/next?types=PredictOutput");
            Assert.Equal(HttpStatusCode.NotFound, blocked.StatusCode);

            var repeat = await AnswerAsync(client, "/api/challenges/1/answer", "5");
            Assert.Equal(0, repeat.GetProperty("pointsAwarded").GetInt32());

            _factory.Clock.SetUtcNow(new DateTimeOffset(2032, 1, 2, 8, 0, 0, TimeSpan.Zero));
            var next = await client.GetFromJsonAsync<JsonElement>("/api/challenges/next?types=PredictOutput");
            Assert.Equal(1, next.GetProperty("id").GetInt32());

            var again = await AnswerAsync(client, "/api/challenges/1/answer", "5");
            Assert.True(again.GetProperty("pointsAwarded").GetInt32() > 0);
        });
    }

    [Fact]
    public async Task Board_DoesNotLeakSecretsOrTheRestOfThePool()
    {
        await WithPool([1, 2, 3, 16, 24, 30, 36, 40], async () =>
        {
            _factory.Clock.SetUtcNow(new DateTimeOffset(2031, 9, 1, 12, 0, 0, TimeSpan.Zero));
            using var client = await RegisterAsync();
            var body = await client.GetStringAsync("/api/daily");
            foreach (var marker in SecretMarkers)
            {
                Assert.DoesNotContain(marker, body, StringComparison.OrdinalIgnoreCase);
            }

            using var document = JsonDocument.Parse(body);
            Assert.Equal(3, document.RootElement.GetProperty("challenges").GetArrayLength());
            Assert.False(document.RootElement.TryGetProperty("pool", out _));
        });
    }

    private async Task<HttpClient> RegisterAsync()
    {
        var client = _factory.CreateCookieClient();
        var stamp = Guid.NewGuid().ToString("N")[..10];
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email = $"{stamp}@example.com",
            username = $"d_{stamp}",
            password = "lozinka123"
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return client;
    }

    private static async Task<List<int>> BoardAsync(HttpClient client)
    {
        var document = await client.GetFromJsonAsync<JsonElement>("/api/daily");
        return document.GetProperty("challenges")
            .EnumerateArray()
            .Select(slot => slot.GetProperty("challenge").GetProperty("id").GetInt32())
            .ToList();
    }

    private static async Task<JsonElement> AnswerAsync(HttpClient client, int challengeId, string answer)
    {
        return await AnswerAsync(client, $"/api/daily/{challengeId}/answer", answer);
    }

    private static async Task<JsonElement> AnswerAsync(HttpClient client, string path, string answer)
    {
        var response = await client.PostAsJsonAsync(path, new { answer });
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, body);
        return JsonDocument.Parse(body).RootElement.Clone();
    }

    private async Task SeedCompletionsAsync(int userId, int count, DateOnly start)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        for (var index = 0; index < count; index++)
        {
            var attempt = new Attempt
            {
                UserId = userId,
                ChallengeId = 1,
                AnswerJson = "\"5\"",
                IsCorrect = true,
                ScoreFraction = 1,
                PointsAwarded = 10,
                CreatedAt = DateTime.UtcNow,
                IsDaily = true
            };
            db.Attempts.Add(attempt);
            await db.SaveChangesAsync();
            db.DailyCompletions.Add(new DailyCompletion
            {
                UserId = userId,
                Date = start.AddDays(index),
                ChallengeId = 1,
                AttemptId = attempt.Id,
                PointsAwarded = 10,
                CompletedAt = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync();
    }

    private async Task WithActiveOnly(IReadOnlyCollection<int> ids, Func<Task> action)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var challenges = await db.Challenges.ToListAsync();
        var previous = challenges.ToDictionary(challenge => challenge.Id, challenge => challenge.IsActive);
        foreach (var challenge in challenges)
        {
            challenge.IsActive = ids.Contains(challenge.Id);
        }

        await db.SaveChangesAsync();
        try
        {
            await action();
        }
        finally
        {
            foreach (var challenge in challenges)
            {
                challenge.IsActive = previous[challenge.Id];
            }

            await db.SaveChangesAsync();
        }
    }

    private async Task WithPool(IReadOnlyCollection<int> ids, Func<Task> action)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var challenges = await db.Challenges.ToListAsync();
        var previous = challenges.ToDictionary(challenge => challenge.Id, challenge => challenge.IsInDailyPool);
        foreach (var challenge in challenges)
        {
            challenge.IsInDailyPool = ids.Contains(challenge.Id);
        }

        await db.SaveChangesAsync();
        try
        {
            await action();
        }
        finally
        {
            foreach (var challenge in challenges)
            {
                challenge.IsInDailyPool = previous[challenge.Id];
            }

            await db.SaveChangesAsync();
        }
    }
}
