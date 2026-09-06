using Entities;
using Entities.Dtos.DriverDTOs;
using Shared.Responses;

namespace ExternalApi.Interfaces
{
    public interface IDriverClient
    {
        Task<DataResponse<Driver>> GetAllDriversSessionSelected(DriverInsertDTO driverInsertDTO);
    }
}
