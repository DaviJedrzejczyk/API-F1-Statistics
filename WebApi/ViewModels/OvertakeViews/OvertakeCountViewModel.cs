using System.Text.Json.Serialization;

namespace WebApi.ViewModels.OvertakeViews
{
    public class OvertakeCountViewModel
    {
        [JsonPropertyName("meeting_key")]
        public int MeetingKey { get; set; }

        [JsonPropertyName("session_key")]
        public int SessionKey { get; set; }

        [JsonPropertyName("numbers_of_overtakes")]
        public int NumbersOfOvertakes { get; set; }
        public OvertakeCountViewModel(int meetingKey, int sessionKey, int numberOfOvertakes)
        {
            NumbersOfOvertakes = numberOfOvertakes;
            MeetingKey = meetingKey;
            SessionKey = sessionKey;
        }
    }
}
