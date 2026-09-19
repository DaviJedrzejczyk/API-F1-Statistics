using Entities.Class;
using Entities.Dtos;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Services.Interfaces
{
    public interface ILapFastSectorService
    {
        Task<DataResponse<LapFastSector>> GetFastSectorsOfSession(int sessionKey);
        Task<Response> SaveFastSectors(List<LapFastSector> lapFastSectors, string observation);
        Task<DataResponse<LapFastSector>> CreateListWithFastSector(List<LapListFastSector> listFastSectorAllDrivers, int sessionKey, List<LapFastSector> listFastSector);
    }
}
