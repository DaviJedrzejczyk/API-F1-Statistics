namespace Entities
{
    public class RaceControl
    {
        public string Category { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int? DriverNumber { get; set; }
        public string Flag { get; set; } = string.Empty;
        public int LapNumber { get; set; }
        public int MeetingKey { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? QualifyingPhase { get; set; }
        public string Scope { get; set; } = string.Empty;
        public int? Sector { get; set; }
        public int SessionKey { get; set; }
    }
}
