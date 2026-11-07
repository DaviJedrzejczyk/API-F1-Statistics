namespace Services.Models;

public sealed class LapCatalogException : Exception
{
    public int StatusCode { get; }

    public LapCatalogException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }
}
