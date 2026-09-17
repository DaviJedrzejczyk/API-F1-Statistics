using Entities.Class;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Services.Interfaces
{
    [IncludeDependencyInjection]
    public interface ILapFastSectorService
    {
        Task<DataResponse<LapFastSector>> GetFastSectorsOfSession(int sessionKey);
        Task<Response> SaveFastSectors(List<LapFastSector> lapFastSectors);
    }
}
