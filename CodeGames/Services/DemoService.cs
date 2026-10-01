using System.Text.Json;
using CodeGames.Data;
using CodeGames.DTOs;
using CodeGames.Exceptions;
using CodeGames.Models;
using CodeGames.Services.Challenges;
using Microsoft.EntityFrameworkCore;

namespace CodeGames.Services;

public class DemoService
{
    public const string ChallengeIdKey = "DemoChallengeId";

    private const int MaxAnswerLength = 4000;

    private static readonly HashSet<ChallengeType> AllowedTypes =
    [
        ChallengeType.SwipeBug,
        ChallengeType.FillBlanks
    ];

    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly IReadOnlyDictionary<ChallengeType, IChallengeHandler> _handlers;

    public DemoService(
        AppDbContext db,
        IConfiguration configuration,
        IEnumerable<IChallengeHandler> handlers)
    {
        _db = db;
        _configuration = configuration;
        _handlers = handlers.ToDictionary(handler => handler.Type);
    }

    public async Task<ChallengeDto> GetChallengeAsync(CancellationToken cancellationToken)
    {
        var challenge = await LoadAsync(cancellationToken);
        var handler = HandlerFor(challenge);
        handler.ValidateConfig(challenge);

        return new ChallengeDto
        {
            Id = challenge.Id,
            Type = challenge.Type,
            Language = challenge.Language,
            Difficulty = challenge.Difficulty,
            Title = challenge.Title,
            Points = challenge.Points,
            Payload = handler.BuildPublicView(challenge)
        };
    }

    public async Task<DemoResultDto> CheckAnswerAsync(JsonElement answer, CancellationToken cancellationToken)
    {
        if (answer.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Odgovor je obavezan.");
        }

        var rawAnswer = answer.GetRawText();
        if (rawAnswer.Length > MaxAnswerLength)
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Odgovor je predug.");
        }

        var challenge = await LoadAsync(cancellationToken);
        var handler = HandlerFor(challenge);
        handler.ValidateConfig(challenge);
        var evaluation = handler.Evaluate(challenge, answer);

        return new DemoResultDto
        {
            IsCorrect = evaluation.IsCorrect,
            ScoreFraction = ClampFraction(evaluation.ScoreFraction),
            Details = evaluation.Details,
            Explanation = challenge.Explanation,
            RevealedSolution = evaluation.RevealedSolution
        };
    }

    private async Task<Challenge> LoadAsync(CancellationToken cancellationToken)
    {
        var challengeId = _configuration.GetValue<int>(ChallengeIdKey);
        if (challengeId <= 0)
        {
            throw new AppException(StatusCodes.Status404NotFound, "Demo zadatak nije postavljen.");
        }

        var challenge = await _db.Challenges
            .AsNoTracking()
            .FirstOrDefaultAsync(
                item => item.Id == challengeId && item.IsActive,
                cancellationToken);

        if (challenge is null)
        {
            throw new AppException(StatusCodes.Status404NotFound, "Demo zadatak nije pronađen.");
        }

        if (!AllowedTypes.Contains(challenge.Type))
        {
            throw new AppException(
                StatusCodes.Status400BadRequest,
                "Demo smije biti samo SwipeBug ili FillBlanks.");
        }

        return challenge;
    }

    private IChallengeHandler HandlerFor(Challenge challenge)
    {
        if (!_handlers.TryGetValue(challenge.Type, out var handler))
        {
            throw new AppException(StatusCodes.Status400BadRequest, "Ova vrsta zadatka još nije dostupna.");
        }

        return handler;
    }

    private static double ClampFraction(double fraction)
    {
        if (double.IsNaN(fraction) || double.IsInfinity(fraction) || fraction < 0d)
        {
            return 0d;
        }

        return fraction > 1d ? 1d : fraction;
    }
}
