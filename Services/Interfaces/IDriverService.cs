using Entities;
using Shared.Responses;

namespace Services.Interfaces
{
    public interface IDriverService
    {
        Task<Response> InsertDrivers();
        Task<Response> UpdateDriver(Driver driver);
        Task<Response> DeleteDriver(Driver driver);
        Task<SingleResponse<Driver>> GetDriverById(int id);
    }
}
