using System.Text.Json.Serialization;

namespace ExternalApi.ViewModels
{
    public class CarDataDto
    {
        [JsonPropertyName("brake")]
        public int? Brake { get; set; }

        [JsonPropertyName("date")]
        public DateTime? Date { get; set; }

        [JsonPropertyName("driver_number")]
        public int? DriverNumber { get; set; }

        [JsonPropertyName("drs")]
        public int? Drs { get; set; }

        [JsonPropertyName("meeting_key")]
        public int? MeetingKey { get; set; }

        [JsonPropertyName("n_gear")]
        public int? Gear { get; set; }

        [JsonPropertyName("rpm")]
        public int? Rpm { get; set; }

        [JsonPropertyName("session_key")]
        public int? SessionKey { get; set; }

        [JsonPropertyName("speed")]
        public int? Speed { get; set; }

        [JsonPropertyName("throttle")]
        public int? Throttle { get; set; }

    }
}
