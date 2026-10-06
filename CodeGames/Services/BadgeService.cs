using CodeGames.Data;
using CodeGames.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeGames.Services;

public class BadgeService
{
    private readonly AppDbContext _db;

    public BadgeService(AppDbContext db)
    {
        _db = db;
    }

    // Vraća true samo kad ukupan broj pređe novi prag od 10.
    public async Task<bool> SyncChallengerAsync(
        int userId,
        int totalCompleted,
        DateTime awardedAt,
        CancellationToken cancellationToken)
    {
        var badge = await _db.UserBadges
            .FirstOrDefaultAsync(
                item => item.UserId == userId && item.BadgeKey == BadgeCatalog.Challenger.Key,
                cancellationToken);

        var previousTier = badge?.Tier ?? 0;
        if (!ChallengerRules.TierIncreased(previousTier, totalCompleted))
        {
            return false;
        }

        var tier = ChallengerRules.TierFor(totalCompleted);
        if (badge is null)
        {
            _db.UserBadges.Add(new UserBadge
            {
                UserId = userId,
                BadgeKey = BadgeCatalog.Challenger.Key,
                Tier = tier,
                AwardedAt = awardedAt
            });
        }
        else
        {
            badge.Tier = tier;
            badge.AwardedAt = awardedAt;
        }

        return true;
    }
}
