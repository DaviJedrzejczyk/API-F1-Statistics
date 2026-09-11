using System.Text.Json.Serialization;

namespace Entities.Dtos
{
    public class OvertakeDto
    {
        [JsonPropertyName("meeting_key")]
        public int MeetingKey { get; set; }

        [JsonPropertyName("session_key")]
        public int SessionKey { get; set; }

        [JsonPropertyName("overtaking_driver_number")]
        public int OvertakingDriverNumber { get; set; }

        [JsonPropertyName("overtaken_driver_number")]
        public int OvertakedDriverNumber { get; set; }

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("position")]
        public int Position { get; set; }
    }
}
