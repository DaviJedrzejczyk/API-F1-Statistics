using Entities.Class;
using System.Text.Json.Serialization;

namespace Entities.Dtos
{
    public class LapFastLapDto
    {
        [JsonPropertyName("driver_number")]
        public int DriverNumber { get; set; }
        
        [JsonPropertyName("duration")]
        public double LapDuration { get; set; }
        
        [JsonPropertyName("session_key")]
        public int SessionKey { get; set; }
        
        [JsonPropertyName("meeting_key")]
        public int MeetingKey { get; set; }

        [JsonPropertyName("is_fast_lap")]
        public bool IsFastLap { get; set; }
    }
}
