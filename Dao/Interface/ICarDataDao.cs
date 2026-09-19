using Entities.Class;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Dao.Interface
{
    public interface ICarDataDao
    {
        Task<Response> SaveCarDatas(List<CarData> data);
        Task<DataResponse<CarData>> GetHighSpeedSessionDatabase(int sessionKey, int minimunSpeed);
    }
}
