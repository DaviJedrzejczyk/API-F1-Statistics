using System.Text.Json.Serialization;

namespace Entities.Dtos;

public sealed class LapCatalogRequest
{
    [JsonPropertyName("driver_numbers")]
    public int[] DriverNumbers { get; set; } = [];
}

public sealed class LapSelectionRequest
{
    [JsonPropertyName("selections")]
    public List<LapSelectionItem?> Selections { get; set; } = [];
}

public sealed class LapSelectionItem
{
    [JsonPropertyName("driver_number")]
    public int DriverNumber { get; set; }

    [JsonPropertyName("lap_number")]
    public int LapNumber { get; set; }
}

public sealed class LapCatalogDto
{
    public int SessionKey { get; set; }
    public int MeetingKey { get; set; }
    public List<DriverLapCatalogDto> Drivers { get; set; } = [];
    public bool ComparisonAvailable { get; set; }
    public string ComparisonUnavailableReason { get; set; } = "Map and telemetry comparison are not available in this release.";
}

public sealed class DriverLapCatalogDto
{
    public int DriverNumber { get; set; }
    public bool ImportComplete { get; set; }
    public string? IncompleteReason { get; set; }
    public int TotalLaps { get; set; }
    public List<CatalogLapDto> Laps { get; set; } = [];
    public int? SuggestedLapNumber { get; set; }
    public string? NoSuggestionReason { get; set; }
    public bool ManualSelectionAllowed { get; set; } = true;
}

public sealed class CatalogLapDto
{
    public int DriverNumber { get; set; }
    public int LapNumber { get; set; }
    public double? Duration { get; set; }
    public string? Compound { get; set; }
    public bool? IsPitOutLap { get; set; }
    public List<LapAlertDto> Alerts { get; set; } = [];
    public List<string> UnavailableData { get; set; } = [];
    public bool SuggestedEligible { get; set; }
}

public sealed class LapAlertDto
{
    public string Code { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public sealed class LapSelectionDto
{
    public Guid PreviewId { get; set; }
    public int SessionKey { get; set; }
    public List<CatalogLapDto> Selections { get; set; } = [];
    public bool Confirmed { get; set; }
    public bool ComparisonAvailable { get; set; }
    public string ComparisonUnavailableReason { get; set; } = "Map and telemetry comparison are not available in this release.";
}
