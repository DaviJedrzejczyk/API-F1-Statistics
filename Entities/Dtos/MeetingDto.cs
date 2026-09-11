using System.Text.Json.Serialization;

namespace Entities.Dtos
{
    public class MeetingDto
    {
        [JsonPropertyName("circuit_key")]
        public int CircuitKey { get; set; }

        [JsonPropertyName("circuit_info_url")]
        public string CircuitInfoUrl { get; set; } = string.Empty;

        [JsonPropertyName("circuit_image")]
        public string CircuitImage { get; set; } = string.Empty;

        [JsonPropertyName("circuit_short_name")]
        public string CircuitShortName { get; set; } = string.Empty;

        [JsonPropertyName("circuit_type")]
        public string CircuitType { get; set; } = string.Empty;

        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; } = string.Empty;

        [JsonPropertyName("country_flag")]
        public string CountryFlag { get; set; } = string.Empty;

        [JsonPropertyName("country_key")]
        public int CountryKey { get; set; }

        [JsonPropertyName("country_name")]
        public string CountryName { get; set; } = string.Empty;

        [JsonPropertyName("date_end")]
        public DateTimeOffset DateEnd { get; set; }

        [JsonPropertyName("date_start")]
        public DateTimeOffset DateStart { get; set; }

        [JsonPropertyName("gmt_offset")]
        public string GmtOffset { get; set; } = string.Empty;

        [JsonPropertyName("is_cancelled")]
        public bool IsCancelled { get; set; }

        [JsonPropertyName("location")]
        public string Location { get; set; } = string.Empty;

        [JsonPropertyName("meeting_key")]
        public int MeetingKey { get; set; }

        [JsonPropertyName("meeting_name")]
        public string MeetingName { get; set; } = string.Empty;

        [JsonPropertyName("meeting_official_name")]
        public string MeetingOfficialName { get; set; } = string.Empty;

        [JsonPropertyName("year")]
        public int Year { get; set; }
    }
}
