using System.Text.Json.Serialization;

namespace WebApi.ViewModels
{
    public class SessionResultFastLapViewModel
    {
        [JsonPropertyName("duration")]
        public double LapDuration { get; set; }

        [JsonPropertyName("is_fast_lap")]
        public bool IsFastLap { get; set; } = false;
    }
}
