using System.Text.Json.Serialization;

namespace WebApi.ViewModels.SuccessViews
{
    public class SuccessViewModel
    {
        [JsonPropertyName("status_code")]
        public int StatusCode { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; }

        public SuccessViewModel()
        {

        }

        public SuccessViewModel(int statusCode, string message)
        {
            StatusCode = statusCode;
            Message = message;
        }
    }
}
