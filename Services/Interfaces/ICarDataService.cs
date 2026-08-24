using Entities;
using Entities.Dtos;
using Shared.Responses;

namespace Services.Interfaces
{
    public interface ICarDataService
    {
        Task<SingleResponse<CarData>> GetHighSpeedDriverSession(int sessionKey, int driverNumber, int minimunSpeed);
        Task<Response> SaveCarData(CarData data);
        Task<DataResponse<SessionDriverSpeedDTO>> GetSortedHighSpeedsSession(int sessionKey, int meetingKey, int minimunSpeed);
    }
}
