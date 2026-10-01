using System.Security.Claims;
using CodeGames.Exceptions;

namespace CodeGames.Controllers;

internal static class AdminCaller
{
    public static int Id(ClaimsPrincipal user)
    {
        var idValue = user.FindFirst("id")?.Value;
        if (!int.TryParse(idValue, out var userId))
        {
            throw new AppException(StatusCodes.Status401Unauthorized, "Niste prijavljeni.");
        }

        return userId;
    }
}
