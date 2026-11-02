namespace Dao.Cache;

/// <summary>Records the completeness and validity of an upstream query window.</summary>
public sealed class ImportWindow
{
    public string Dataset { get; set; } = string.Empty;
    public string WindowKey { get; set; } = string.Empty;
    public DateTimeOffset CompletedAtUtc { get; set; }
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public bool IsComplete { get; set; }
    public string Payload { get; set; } = string.Empty;
}
