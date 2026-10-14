using System.Text.Json.Serialization;

namespace WebApi.ViewModels
{
    public class SessionMeetingKeyViewModel
    {
        [JsonPropertyName("meeting_key")]
        public int MeetingKey { get; set; }
    }
}
