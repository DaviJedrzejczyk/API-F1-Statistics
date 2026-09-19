using Entities.Class;
using Entities.Dtos;
using Shared.Responses;
using Shared.Common.Atrributes;

namespace Services.Interfaces
{
    public interface ICarDataService
    {
        Task<DataResponse<CarData>> GetHighSpeedsSessionApi(int sessionKey, int minimunSpeed);
        Task<Response> SaveCarDatas(List<CarData> data);
        Task<DataResponse<SessionDriverSpeedDTO>> GetSortedHighSpeedsSession(int sessionKey, int minimunSpeed);
        Task<DataResponse<CarData>> GetHighSpeedSessionDatabase(int sessionKey, int minimunSpeed);

    }
}
