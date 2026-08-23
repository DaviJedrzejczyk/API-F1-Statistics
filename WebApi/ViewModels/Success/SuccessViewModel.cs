using System.Text.Json.Serialization;

namespace WebApi.ViewModels.Success
{
    public class SuccessViewModel
    {
        public SuccessViewModel()
        {
            
        }

        //public SuccessViewModel(int statusCode, string message)
        //{
        //    StatusCode = statusCode;
        //    Message = message;
        //}

        [JsonPropertyName("status_code")]
        public int StatusCode { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
