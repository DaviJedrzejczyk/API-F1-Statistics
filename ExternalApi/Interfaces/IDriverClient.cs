using Entities.Class;
using Entities.Dtos;
using Shared.Responses;

namespace ExternalApi.Interfaces
{
    public interface IDriverClient
    {
        Task<DataResponse<Driver>> GetAllDriversSessionSelected(DriverInsertDTO driverInsertDTO);
        Task<DataResponse<Driver>> GetAllDriversSessionSelected(DriverInsertDTO driverInsertDTO, CancellationToken cancellationToken);
    }
}
