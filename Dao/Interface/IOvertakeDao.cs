using Entities.Class;
using Shared.Responses;

namespace Dao.Interface
{
    public interface IOvertakeDao
    {
        Task<Response> SaveOvertakes(List<Overtake> overtakes);
        Task<DataResponse<Overtake>> GetOvertakesBySession(int sessionKey);
    }
}
