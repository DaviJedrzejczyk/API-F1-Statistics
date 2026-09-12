using Entities.Class;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Dao.Interface
{
    [IncludeDependencyInjection]
    public interface IPitDao
    {
        Task<Response> SavePits(List<Pit> pits);
        Task<DataResponse<Pit>> GetAllPitsBySessionKey(int sessionKey);
    }
}
