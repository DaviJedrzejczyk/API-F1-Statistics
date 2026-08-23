using Entities;
using Shared.Responses;

namespace Dao.Interface
{
    public interface ICarDataDao
    {
        Task<Response> InsertHighSpeedSessionDriver(CarData carData);
    }
}
