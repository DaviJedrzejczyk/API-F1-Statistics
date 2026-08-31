using Entities;
using Shared.Responses;

namespace Dao.Interface
{
    public interface IPitDao
    {
        Task<Response> SavePits(List<Pit> pits);
        Task<DataResponse<Pit>> GetAllPitsBySessionKey(int sessionKey);
    }
}
