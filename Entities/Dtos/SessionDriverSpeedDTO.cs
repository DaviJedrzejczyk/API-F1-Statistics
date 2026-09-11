using System.Text.Json.Serialization;

namespace Entities.Dtos
{
    public class SessionDriverSpeedDTO
    {
        [JsonPropertyName("session_key")]
        public int SessionKey { get; set; }
        
        [JsonPropertyName("driver_number")]
        public int DriverNumber { get; set; }

        [JsonPropertyName("driver_name")]
        public string DriverName { get; set; }

        [JsonPropertyName("speed")]
        public int Speed { get; set; }

        [JsonPropertyName("headshot_url")]
        public string HeadshotUrl { get; set; }

        [JsonPropertyName("team_colour")]
        public string TeamColour { get; set; }

        [JsonPropertyName("team_name")]
        public string TeamName { get; set; }
    }
}
