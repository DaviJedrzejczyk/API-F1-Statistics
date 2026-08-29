using Entities;
using Shared.Responses;

namespace Dao.Interface
{
    public interface ISessionResultDao
    {
        Task<Response> SaveSessionResults(List<SessionResult> sessionResults);
        Task<DataResponse<SessionResult>> GetSessionResultsBySesssionKey(int sessionKey);
    }
}
