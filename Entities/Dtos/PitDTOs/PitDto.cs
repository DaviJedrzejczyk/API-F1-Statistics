using System.Text.Json.Serialization;

namespace Entities.Dtos.PitDTOs
{
    public class PitDto
    {
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("driver_number")]
        public int DriverNumber { get; set; }

        [JsonPropertyName("lane_duration")]
        public double LaneDuration { get; set; }

        [JsonPropertyName("lap_number")]
        public int LapNumber { get; set; }

        [JsonPropertyName("meeting_key")]
        public int MeetingKey { get; set; }

        [JsonPropertyName("pit_duration")]
        public double PitDuration { get; set; }

        [JsonPropertyName("session_key")]
        public int SessionKey { get; set; }

        [JsonPropertyName("stop_duration")]
        public double? StopDuration { get; set; }
    }
}
