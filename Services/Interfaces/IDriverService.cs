using Entities.Class;
using Entities.Dtos;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Services.Interfaces
{
    [IncludeDependencyInjection]
    public interface IDriverService
    {
        Task<Response> InsertDrivers(DriverInsertDTO driverInsertDTO);
        Task<Response> DeleteDriver(Driver driver);
        Task<SingleResponse<Driver>> GetDriverById(int id);
        Task<DataResponse<Driver>> GetAllDriversSession(int sessionKey);
    }
}
