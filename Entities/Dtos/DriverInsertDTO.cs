using System.Text.Json.Serialization;

namespace Entities.Dtos
{
    public class DriverInsertDTO
    {
        [JsonPropertyName("meeting_key")]
        public int MeetingKey { get; set; }

        [JsonPropertyName("session_key")]
        public int SessionKey { get; set; }

        public DriverInsertDTO()
        {
            
        }

        public DriverInsertDTO(int meetingKey, int sessionKey)
        {
            MeetingKey = meetingKey;
            SessionKey = sessionKey;
        }
    }
}
