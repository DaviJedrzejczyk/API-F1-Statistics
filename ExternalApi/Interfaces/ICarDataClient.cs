using Entities;
using Shared.Responses;

namespace ExternalApi.Interfaces
{
    public interface ICarDataClient
    {
        Task<DataResponse<CarData>> GetHighSpeedsDriverSession(int sessionKey, int driverNumber, int minimunSpeed); 
    }
}
