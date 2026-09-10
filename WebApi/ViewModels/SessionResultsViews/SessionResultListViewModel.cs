using System.Text.Json.Serialization;

namespace WebApi.ViewModels.SessionResultsViews
{
    public class SessionResultListViewModel
    {
        [JsonPropertyName("dnf")]
        public bool Dnf { get; set; }

        [JsonPropertyName("dns")]
        public bool Dns { get; set; }

        [JsonPropertyName("dsq")]
        public bool Dsq { get; set; }

        [JsonPropertyName("driver_number")]
        public int DriverNumber { get; set; }

        [JsonPropertyName("duration")]
        public double Duration { get; set; }

        [JsonPropertyName("gap_to_leader")]
        public string GapToLeader { get; set; } = string.Empty;

        [JsonPropertyName("number_of_laps")]
        public int NumberOfLaps { get; set; }

        [JsonPropertyName("points")]
        public double Points { get; set; }

        [JsonPropertyName("meeting_key")]
        public int MeetingKey { get; set; }

        [JsonPropertyName("position")]
        public int Position { get; set; }

        [JsonPropertyName("session_key")]
        public int SessionKey { get; set; }

        [JsonPropertyName("compound")]
        public string Compound { get; set; } = string.Empty;
    }
}
