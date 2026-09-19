using Entities.Class;
using Entities.Dtos;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Dao.Interface
{
    public interface ILapDao
    {
        Task<SingleResponse<Lap>> GetFastLapSessionBySessionKey(int sessionKey);
        Task<Response> SaveLap(Lap lap);
    }
}
