using CodeGames.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeGames.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Challenge> Challenges => Set<Challenge>();

    public DbSet<ChallengeOption> ChallengeOptions => Set<ChallengeOption>();

    public DbSet<Attempt> Attempts => Set<Attempt>();

    public DbSet<UserBadge> UserBadges => Set<UserBadge>();

    public DbSet<DailySelection> DailySelections => Set<DailySelection>();

    public DbSet<DailyCompletion> DailyCompletions => Set<DailyCompletion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureUser(modelBuilder);
        ConfigureChallenge(modelBuilder);
        ConfigureChallengeOption(modelBuilder);
        ConfigureAttempt(modelBuilder);
        ConfigureUserBadge(modelBuilder);
        ConfigureDailySelection(modelBuilder);
        ConfigureDailyCompletion(modelBuilder);
        ChallengeSeed.Apply(modelBuilder);
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<User>();

        entity.ToTable("Users");
        entity.Property(user => user.Email).HasMaxLength(256).IsRequired();
        entity.Property(user => user.Username).HasMaxLength(50).IsRequired();
        entity.Property(user => user.PasswordHash).HasMaxLength(200).IsRequired();
        entity.Property(user => user.Role).HasConversion<string>().HasMaxLength(20).IsRequired();
        entity.Property(user => user.CreatedAt).IsRequired();

        entity.HasIndex(user => user.Email).IsUnique();
        entity.HasIndex(user => user.Username).IsUnique();
        entity.HasIndex(user => user.Xp).IsDescending(true);

        // Restrict, ne Cascade: Attempts već imaju vezu i prema zadatku.
        entity.HasMany(user => user.Attempts)
            .WithOne(attempt => attempt.User)
            .HasForeignKey(attempt => attempt.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureChallenge(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Challenge>();

        entity.ToTable("Challenges", table =>
            table.HasCheckConstraint(
                "CK_Challenge_Difficulty",
                "[Difficulty] >= 1 AND [Difficulty] <= 3"));

        entity.Property(challenge => challenge.Type).HasConversion<string>().HasMaxLength(30).IsRequired();
        entity.Property(challenge => challenge.Language).HasMaxLength(30).IsRequired();
        entity.Property(challenge => challenge.Title).HasMaxLength(200).IsRequired();
        entity.Property(challenge => challenge.CodeSnippet).HasMaxLength(4000).IsRequired();
        entity.Property(challenge => challenge.Question).HasMaxLength(1000).IsRequired();
        entity.Property(challenge => challenge.CorrectAnswer).HasMaxLength(500).IsRequired();
        entity.Property(challenge => challenge.Explanation).HasMaxLength(2000).IsRequired();
        entity.Property(challenge => challenge.ConfigJson).HasColumnType("nvarchar(max)");

        // Opcije žive samo uz zadatak, pa se brišu zajedno s njim.
        entity.HasMany(challenge => challenge.Options)
            .WithOne(option => option.Challenge)
            .HasForeignKey(option => option.ChallengeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Pokušaj se ne briše kaskadno: inače SQL Server vidi dva CASCADE puta do Attempts.
        entity.HasMany(challenge => challenge.Attempts)
            .WithOne(attempt => attempt.Challenge)
            .HasForeignKey(attempt => attempt.ChallengeId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureChallengeOption(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ChallengeOption>();

        entity.ToTable("ChallengeOptions");
        entity.Property(option => option.Text).HasMaxLength(500).IsRequired();
        entity.HasIndex(option => option.ChallengeId);
    }

    private static void ConfigureAttempt(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Attempt>();

        entity.ToTable("Attempts");
        entity.Property(attempt => attempt.AnswerJson).HasMaxLength(4000).IsRequired();
        entity.Property(attempt => attempt.ScoreFraction).IsRequired();
        entity.Property(attempt => attempt.CreatedAt).IsRequired();
        entity.HasIndex(attempt => attempt.UserId);
    }

    private static void ConfigureUserBadge(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<UserBadge>();

        entity.ToTable("UserBadges");
        entity.Property(badge => badge.BadgeKey).HasMaxLength(40).IsRequired();
        entity.Property(badge => badge.AwardedAt).IsRequired();
        entity.HasIndex(badge => new { badge.UserId, badge.BadgeKey }).IsUnique();

        entity.HasOne(badge => badge.User)
            .WithMany()
            .HasForeignKey(badge => badge.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureDailySelection(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<DailySelection>();

        entity.ToTable("DailySelections", table =>
            table.HasCheckConstraint("CK_DailySelection_Slot", "[Slot] >= 1 AND [Slot] <= 3"));
        entity.Property(selection => selection.Date).IsRequired();
        entity.HasIndex(selection => new { selection.Date, selection.Slot }).IsUnique();
        entity.HasIndex(selection => new { selection.Date, selection.ChallengeId }).IsUnique();

        entity.HasOne(selection => selection.Challenge)
            .WithMany()
            .HasForeignKey(selection => selection.ChallengeId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureDailyCompletion(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<DailyCompletion>();

        entity.ToTable("DailyCompletions");
        entity.Property(completion => completion.Date).IsRequired();
        entity.Property(completion => completion.CompletedAt).IsRequired();
        entity.HasIndex(completion => new { completion.UserId, completion.Date, completion.ChallengeId }).IsUnique();

        entity.HasOne(completion => completion.User)
            .WithMany()
            .HasForeignKey(completion => completion.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(completion => completion.Challenge)
            .WithMany()
            .HasForeignKey(completion => completion.ChallengeId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(completion => completion.Attempt)
            .WithMany()
            .HasForeignKey(completion => completion.AttemptId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
