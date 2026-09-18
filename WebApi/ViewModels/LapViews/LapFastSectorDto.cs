using System.Text.Json.Serialization;

namespace WebApi.ViewModels
{
    public class LapFastSectorDto
    {
        [JsonPropertyName("session_key")]
        public int SessionKey { get; set; }

        [JsonPropertyName("driver_number")]
        public int DriverNumber { get; set; }
        
        [JsonPropertyName("duration")]
        public double Duration { get; set; }
        
        [JsonPropertyName("driver_name")]
        public string? DriverName { get; set; }

        [JsonPropertyName("sector")]
        public int Sector { get; set; }
    }
}
