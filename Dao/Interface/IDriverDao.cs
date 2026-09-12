using Entities.Class;
using Shared.Common.Atrributes;
using Shared.Responses;
namespace Dao.Interface
{
    [IncludeDependencyInjection]
    public interface IDriverDao
    {
        Task<Response> InsertDrivers(List<Driver> drivers);
        Task<Response> DeleteDriver(Driver driver);
        Task<SingleResponse<Driver>> GetDriverById(int id);
        Task<DataResponse<Driver>> GetAllDriversSession(int sessionKey);
    }
}
