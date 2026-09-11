using System.Text.Json.Serialization;

namespace Entities.Dtos
{
    public class DriverDto
    {
        [JsonIgnore]
        public int DriverKey { get; set; }

        [JsonPropertyName("driver_number")]
        public int DriverNumber { get; set; }

        [JsonPropertyName("broadcast_name")]
        public string BroadcastName { get; set; }

        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }

        [JsonPropertyName("full_name")]
        public string FullName { get; set; }

        [JsonPropertyName("headshot_url")]
        public string HeadshotUrl { get; set; }

        [JsonPropertyName("last_name")]
        public string LastName { get; set; }

        [JsonPropertyName("meeting_key")]
        public int MeetingKey { get; set; }

        [JsonPropertyName("name_acronym")]
        public string NameAcronym { get; set; }

        [JsonPropertyName("session_key")]
        public int SessionKey { get; set; }

        [JsonPropertyName("team_colour")]
        public string TeamColour { get; set; }

        [JsonPropertyName("team_name")]
        public string TeamName { get; set; }
    }
}
