using Entities.Class;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Dao.Interface
{
    public interface IStintDao
    {
        Task<DataResponse<Stint>> GetStintsBySessionKey(int sessionKey);
        Task<Response> SaveStints(List<Stint> stints);
    }
}
