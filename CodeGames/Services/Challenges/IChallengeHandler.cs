using System.Text.Json;
using CodeGames.Models;

namespace CodeGames.Services.Challenges;

// Jedan handler po tipu. Novi tip igre dodaje novu klasu i registraciju u DI.
public interface IChallengeHandler
{
    ChallengeType Type { get; }

    // Samo ono što igrač smije vidjeti prije odgovora.
    object BuildPublicView(Challenge challenge);

    EvaluationResult Evaluate(Challenge challenge, JsonElement answer);

    // Provjera da zadatak ima sve što tip traži. Admin panel je zove pri spremanju.
    void ValidateConfig(Challenge challenge);
}
