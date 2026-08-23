using Shared.Responses;

namespace ExternalApi.Interfaces
{
    public interface ICarDataClient
    {
        Task<SingleResponse<string>> GetHighSpeedsDriverSession(int meetingKey, int sessionKey, int driverKey, int minimunSpeed); 
    }
}
