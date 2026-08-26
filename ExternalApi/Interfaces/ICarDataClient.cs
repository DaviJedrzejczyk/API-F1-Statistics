using Entities;
using Shared.Responses;

namespace ExternalApi.Interfaces
{
    public interface ICarDataClient
    {
        Task<DataResponse<CarData>> GetHighSpeedsSession(int sessionKey, int minimunSpeed); 
    }
}
