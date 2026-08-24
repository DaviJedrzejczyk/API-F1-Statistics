using Entities;
using Entities.Dtos;
using Shared.Responses;

namespace ExternalApi.Interfaces
{
    public interface IDriverClient
    {
        Task<DataResponse<Driver>> GetAllDriversSessionSelected(DriverInsertDTO driverInsertDTO);
    }
}
