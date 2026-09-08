using Shared.Converters;
using System.Text.Json.Serialization;

namespace Entities.Dtos.SessionResultDTOs
{
    public class SessionResultDto
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
        [JsonConverter(typeof(ListDoubleNullToZeroConverter))]
        public List<double> Duration { get; set; } = [];

        [JsonPropertyName("gap_to_leader")]
        [JsonConverter(typeof(GapToLeaderConverter))]
        public string GapToLeader { get; set; } = string.Empty;

        [JsonPropertyName("number_of_laps")]
        public int NumberOfLaps { get; set; }

        [JsonPropertyName("points")]
        [JsonConverter(typeof(DoubleNullToZeroConverter))]
        public double Points { get; set; }

        [JsonPropertyName("meeting_key")]
        public int MeetingKey { get; set; }

        [JsonPropertyName("position")]
        [JsonConverter(typeof(IntNullToZeroConverter))]
        public int Position { get; set; }

        [JsonPropertyName("session_key")]
        public int SessionKey { get; set; }
    }
}
