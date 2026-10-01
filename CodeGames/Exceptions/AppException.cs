namespace CodeGames.Exceptions;

// Očekivana greška s HTTP statusom i porukom za klijenta.
public class AppException : Exception
{
    public int StatusCode { get; }

    public AppException(int statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
