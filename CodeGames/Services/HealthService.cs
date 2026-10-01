using CodeGames.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeGames.Services;

public class HealthService
{
    private readonly AppDbContext _db;

    public HealthService(AppDbContext db)
    {
        _db = db;
    }

    // Otvara vezu prema SQL Serveru. Ne izvršava upit nad tablicama.
    public async Task<bool> CanConnectAsync(CancellationToken cancellationToken)
    {
        return await _db.Database.CanConnectAsync(cancellationToken);
    }
}
