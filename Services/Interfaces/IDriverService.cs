using Entities;
using Entities.Dtos;
using Shared.Responses;

namespace Services.Interfaces
{
    public interface IDriverService
    {
        Task<Response> InsertDrivers(DriverInsertDTO driverInsertDTO);
        Task<Response> DeleteDriver(Driver driver);
        Task<SingleResponse<Driver>> GetDriverById(int id);
        Task<DataResponse<Driver>> GetAllDriversSession(int sessionKey);
    }
}
