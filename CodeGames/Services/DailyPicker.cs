using CodeGames.Exceptions;

namespace CodeGames.Services;

public static class DailyPicker
{
    public const int SlotCount = 3;
    public const int TargetPool = 10;
    public const int AllClearBonus = 25;

    public static IReadOnlyList<int> Pick(IReadOnlyList<int> pool, IReadOnlyCollection<int> yesterday, Random random)
    {
        var distinct = pool.Distinct().ToList();
        if (distinct.Count < SlotCount)
        {
            throw new AppException(
                StatusCodes.Status400BadRequest,
                "Dnevni bazen treba barem 3 aktivna zadatka.");
        }

        var yesterdaySet = yesterday.Distinct().ToHashSet();
        var outsideYesterday = distinct.Where(id => !yesterdaySet.Contains(id)).ToList();
        var source = outsideYesterday.Count >= SlotCount ? outsideYesterday : distinct;
        var chosen = source.OrderBy(_ => random.Next()).Take(SlotCount).ToList();

        if (yesterdaySet.Count >= SlotCount
            && chosen.All(yesterdaySet.Contains)
            && distinct.Count > SlotCount)
        {
            var replacement = distinct.First(id => !chosen.Contains(id));
            chosen[0] = replacement;
        }

        return chosen;
    }
}
