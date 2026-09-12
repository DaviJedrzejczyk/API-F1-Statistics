using Entities.Class;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Dao.Interface
{
    [IncludeDependencyInjection]
    public interface ISessionResultDao
    {
        Task<Response> SaveSessionResults(List<SessionResult> sessionResults);
        Task<DataResponse<SessionResult>> GetSessionResultsBySesssionKey(int sessionKey);
    }
}
