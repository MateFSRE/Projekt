namespace CodeGames.Services;

public sealed record BadgeDefinition(string Key, string Name, string Description);

// Definicije bedževa žive u kodu. Novi bedž je novi zapis ovdje i pravilo u BadgeService.
public static class BadgeCatalog
{
    public static readonly BadgeDefinition Challenger = new(
        "challenger",
        "Challenger",
        "Svakih 10 riješenih dnevnih izazova otvara sljedeći tier.");

    public static IReadOnlyList<BadgeDefinition> All { get; } = [Challenger];
}
