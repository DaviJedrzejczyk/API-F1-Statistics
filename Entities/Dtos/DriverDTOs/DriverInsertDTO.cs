using System.Text.Json.Serialization;

namespace Entities.Dtos.DriverDTOs
{
    public class DriverInsertDTO
    {
        [JsonPropertyName("session_key")]
        public int SessionKey { get; set; }

        public DriverInsertDTO()
        {
            
        }

        public DriverInsertDTO(int sessionKey)
        {
            SessionKey = sessionKey;
        }
    }
}
