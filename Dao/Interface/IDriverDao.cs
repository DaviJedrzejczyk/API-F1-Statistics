using Entities;
using Shared.Responses;
namespace Dao.Interface
{
    public interface IDriverDao
    {
        Task<Response> InsertDrivers(List<Driver> drivers);
        Task<Response> UpdateDriver(Driver driver);
        Task<Response> DeleteDriver(Driver driver);
        Task<SingleResponse<Driver>> GetDriverById(int id);
    }
}
