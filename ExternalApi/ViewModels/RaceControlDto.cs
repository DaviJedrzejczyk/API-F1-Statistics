using System.Text.Json.Serialization;

namespace ExternalApi.ViewModels
{
    public class RaceControlDto
    {
        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("driver_number")]
        public int? DriverNumber { get; set; }

        [JsonPropertyName("flag")]
        public string Flag { get; set; } = string.Empty;

        [JsonPropertyName("lap_number")]
        public int LapNumber { get; set; }

        [JsonPropertyName("meeting_key")]
        public int MeetingKey { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("qualifying_phase")]
        public int? QualifyingPhase { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; } = string.Empty;

        [JsonPropertyName("sector")]
        public int? Sector { get; set; }

        [JsonPropertyName("session_key")]
        public int SessionKey { get; set; }
    }
}
