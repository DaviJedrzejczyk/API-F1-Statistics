using System.Text.Json.Serialization;

namespace Entities.Dtos
{
    public class StintListDTO
    {
        [JsonPropertyName("compound")]
        public string Compound { get; set; } = string.Empty;
        
        [JsonPropertyName("driver_number")]
        public int DriverNumber { get; set; }
        
        [JsonPropertyName("lap_end")]
        public int? LapEnd { get; set; }
        
        [JsonPropertyName("lap_start")]
        public int? LapStart { get; set; }
        
        [JsonPropertyName("meeting_key")] 
        public int MeetingKey { get; set; }
        
        [JsonPropertyName("session_key")]
        public int SessionKey { get; set; }

        [JsonPropertyName("stint_number")]
        public int StintNumber { get; set; }
        
        [JsonPropertyName("tyre_age_at_start")]
        public int TyreAgeAtStart { get; set; }
    }
}
