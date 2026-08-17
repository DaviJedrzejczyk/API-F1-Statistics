using System.Text.Json.Serialization;

namespace WebApi.ViewModels
{
    public class SessionViewModel
    {
        [JsonPropertyName("session_key")]
        public int SessionKey { get; set; }

        [JsonPropertyName("circuit_key")]
        public int CircuitKey { get; set; }

        [JsonPropertyName("circuit_short_name")]
        public string CircuitShortName { get; set; } = string.Empty;

        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; } = string.Empty;

        [JsonPropertyName("country_key")]
        public int CountryKey { get; set; }

        [JsonPropertyName("country_name")]
        public string CountryName { get; set; } = string.Empty;

        [JsonPropertyName("date_start")]
        public DateTimeOffset DateStart { get; set; }

        [JsonPropertyName("date_end")]
        public DateTimeOffset DateEnd { get; set; }

        [JsonPropertyName("gmt_offset")]
        public TimeSpan GmtOffset { get; set; }

        [JsonPropertyName("is_cancelled")]
        public bool IsCancelled { get; set; }

        [JsonPropertyName("location")]
        public string Location { get; set; } = string.Empty;

        [JsonPropertyName("meeting_key")]
        public int MeetingKey { get; set; }

        [JsonPropertyName("session_name")]
        public string SessionName { get; set; } = string.Empty;

        [JsonPropertyName("session_type")]
        public string SessionType { get; set; } = string.Empty;

        [JsonPropertyName("year")]
        public int Year { get; set; }
    }
}

