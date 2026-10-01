using CodeGames.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeGames.Data;

// Lozinke se čitaju iz konfiguracije (user-secrets). U bazu ide samo hash.
public static class AdminSeeder
{
    public const string Email = "admin@codegames.local";
    public const string Username = "admin";

    public const string PlayerEmail = "igrac1@codegames.local";
    public const string PlayerUsername = "igrac1";

    public static async Task SeedAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("AdminSeeder");

        await EnsureUserAsync(
            db,
            logger,
            configuration["Seed:AdminPassword"],
            Email,
            Username,
            UserRole.Admin,
            "Admin nije stvoren. U user-secrets postavi ključ Seed:AdminPassword.");

        await EnsureUserAsync(
            db,
            logger,
            configuration["Seed:PlayerPassword"],
            PlayerEmail,
            PlayerUsername,
            UserRole.User,
            "Igrač nije stvoren. U user-secrets postavi ključ Seed:PlayerPassword.");
    }

    private static async Task EnsureUserAsync(
        AppDbContext db,
        ILogger logger,
        string? password,
        string email,
        string username,
        UserRole role,
        string missingPasswordMessage)
    {
        if (await db.Users.AnyAsync(user => user.Email == email || user.Username == username))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("{Message}", missingPasswordMessage);
            return;
        }

        db.Users.Add(new User
        {
            Email = email,
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role,
            Xp = 0,
            Level = 1,
            CurrentStreak = 0,
            BestStreak = 0,
            LastActiveDate = null,
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
        logger.LogInformation("Korisnik {Username} je spremljen.", username);
    }
}
