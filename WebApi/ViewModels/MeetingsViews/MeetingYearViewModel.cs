using System.Text.Json.Serialization;

namespace WebApi.ViewModels
{
    public class MeetingYearViewModel
    {
        [JsonPropertyName("year")]
        public int Year { get; set; }
    }
}
