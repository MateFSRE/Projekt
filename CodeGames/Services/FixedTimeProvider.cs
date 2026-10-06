namespace CodeGames.Services;

// Vrijeme koje test ili razvojna konfiguracija mogu pomaknuti.
public sealed class FixedTimeProvider : TimeProvider
{
    private readonly object _gate = new();
    private DateTimeOffset _utcNow;

    public FixedTimeProvider(DateTimeOffset utcNow)
    {
        _utcNow = utcNow;
    }

    public override DateTimeOffset GetUtcNow()
    {
        lock (_gate)
        {
            return _utcNow;
        }
    }

    public void SetUtcNow(DateTimeOffset utcNow)
    {
        lock (_gate)
        {
            _utcNow = utcNow;
        }
    }
}
