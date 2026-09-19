using Entities.Class;
using Entities.Dtos;
using Shared.Common.Atrributes;
using Shared.Responses;
using System;
namespace ExternalApi.Interfaces
{
    public interface ILapClient
    {
        Task<DataResponse<LapListDto>> GetAllLapsSessionByDriver(int sessionKey, int driverNumber);
    }
}
