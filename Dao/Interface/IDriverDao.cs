using Entities;
using Shared.Responses;
namespace Dao.Interface
{
    public interface IDriverDao
    {
        Task<Response> InsertDriver(Driver driver);
        Task<Response> InsertDriver(List<Driver> drivers);
        Task<Response> UpdateDriver(Driver driver);
        Task<Response> DeleteDriver(int id);
        Task<SingleResponse<Driver>> GetDriverById(int id);
    }
}
