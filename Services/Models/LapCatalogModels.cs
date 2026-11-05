namespace Services.Models;

public sealed class LapCatalogException : Exception
{
    public int StatusCode { get; }
    public LapCatalogException(int statusCode, string message) : base(message) => StatusCode = statusCode;
}

public sealed record LapCatalogRequest(int[] DriverNumbers);
public sealed record LapSelectionItem(int DriverNumber, int LapNumber);
public sealed record LapSelectionRequest(List<LapSelectionItem> Selections);
public sealed record LapAlert(string Code, string Message);
public sealed record CatalogLap(int DriverNumber, int LapNumber, double? Duration, string? Compound,
    bool IsPitOutLap, IReadOnlyList<LapAlert> Alerts, IReadOnlyList<string> Unavailable,
    bool SuggestedEligible);
public sealed record DriverLapCatalog(int DriverNumber, IReadOnlyList<CatalogLap> Laps,
    int? SuggestedLapNumber, string? NoSuggestionReason, bool ManualSelectionAllowed);
public sealed record LapCatalogResponse(int SessionKey, int MeetingKey, IReadOnlyList<DriverLapCatalog> Drivers,
    bool ComparisonAvailable, string? ComparisonUnavailableReason);
public sealed record LapSelectionResponse(int SessionKey, IReadOnlyList<CatalogLap> Selections,
    bool Confirmed, bool ComparisonAvailable, string? ComparisonUnavailableReason);
