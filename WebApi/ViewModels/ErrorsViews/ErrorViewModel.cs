using System.Text.Json.Serialization;

namespace WebApi.ViewModels
{
    public class ErrorViewModel
    {
        [JsonPropertyName("status_code")]
        public int StatusCode { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; }

        public ErrorViewModel()
        {
            
        }

        public ErrorViewModel(int statusCode, string message)
        {
            StatusCode = statusCode;
            Message = message;
        }
    }
}
