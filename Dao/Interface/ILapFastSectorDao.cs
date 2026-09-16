using Entities.Class;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Dao.Interface
{
    [IncludeDependencyInjection]
    public interface ILapFastSectorDao
    {
        Task<Response> SaveLapFastSector(List<LapFastSector> lapFastSectors);
        Task<DataResponse<LapFastSector>> GetAllLapFastSectorsSession(int sessionKey);
    }
}
