using Entities.Class;
using Entities.Dtos;

namespace Services.Models;

// Durable historical snapshots use ImportWindows storage, but never its five-minute TTL.
// A complete source response is distinct from the availability of individual measurements.
internal sealed class LapCatalogSnapshot
{
    public int Version { get; set; } = 1;
    public int SessionKey { get; set; }
    public int DriverNumber { get; set; }
    public bool Complete { get; set; }
    public string? IncompleteReason { get; set; }
    public List<LapListDto> Laps { get; set; } = [];
}

internal sealed class LapCatalogEnrichment
{
    public List<Stint>? Stints { get; set; }
    public List<Pit>? Pits { get; set; }
    public List<RaceControl>? RaceControls { get; set; }
    public List<string> UnavailableSources { get; set; } = [];
}
