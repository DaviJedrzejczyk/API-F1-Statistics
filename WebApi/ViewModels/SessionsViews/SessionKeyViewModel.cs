using System.Text.Json.Serialization;

namespace WebApi.ViewModels
{
    public class SessionKeyViewModel
    {
        [JsonPropertyName("session_key")]
        public int SessionKey { get; set; }
    }
}
