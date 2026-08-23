using System.Text.Json.Serialization;

namespace WebApi.ViewModels.Meetings
{
    public class MeetingYearViewModel
    {
        [JsonPropertyName("year")]
        public int Year { get; set; }
    }
}
