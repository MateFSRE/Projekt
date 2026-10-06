using System.Text.Json;
using CodeGames.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CodeGames.Tests;

public sealed class ApiFactory : Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program>
{
    public const string AdminPassword = "AdminTest123!";

    public FixedTimeProvider Clock { get; } = new(new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero));

    static ApiFactory()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("ConnectionStrings__CodeGamesDb", TestConnectionString());
        Environment.SetEnvironmentVariable("Jwt__Secret", "integration-test-secret-key-32chars!!");
        Environment.SetEnvironmentVariable("Seed__AdminPassword", AdminPassword);
    }

    public HttpClient CreateCookieClient()
    {
        return CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            HandleCookies = true,
            AllowAutoRedirect = false
        });
    }

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var existing = services.SingleOrDefault(service => service.ServiceType == typeof(TimeProvider));
            if (existing is not null)
            {
                services.Remove(existing);
            }

            services.AddSingleton<TimeProvider>(Clock);
        });
    }

    private static string TestConnectionString()
    {
        var fromEnv = Environment.GetEnvironmentVariable("CODEGAMES_TEST_CONNECTION");
        if (!string.IsNullOrWhiteSpace(fromEnv))
        {
            return fromEnv;
        }

        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "CodeGames", "appsettings.Development.json");
            if (File.Exists(candidate))
            {
                using var document = JsonDocument.Parse(File.ReadAllText(candidate));
                var value = document.RootElement
                    .GetProperty("ConnectionStrings")
                    .GetProperty("CodeGamesDb")
                    .GetString();
                if (string.IsNullOrWhiteSpace(value) || !value.Contains("Database=CodeGames", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        "Connection string u appsettings.Development.json nema Database=CodeGames.");
                }

                return value.Replace("Database=CodeGames", "Database=CodeGamesTests", StringComparison.OrdinalIgnoreCase);
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException(
            "Integracijski testovi trebaju CodeGames/appsettings.Development.json ili varijablu CODEGAMES_TEST_CONNECTION.");
    }
}
