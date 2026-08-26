using Entities;
using Entities.Dtos;
using Shared.Responses;

namespace Services.Interfaces
{
    public interface ICarDataService
    {
        Task<DataResponse<CarData>> GetHighSpeedsSession(int sessionKey, int minimunSpeed);
        Task<Response> SaveCarDatas(List<CarData> data);
        Task<DataResponse<SessionDriverSpeedDTO>> GetSortedHighSpeedsSession(int sessionKey, int minimunSpeed);
    }
}
