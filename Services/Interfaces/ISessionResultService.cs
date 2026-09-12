using Entities.Class;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Services.Interfaces
{
    [IncludeDependencyInjection]
    public interface ISessionResultService
    {
        Task<DataResponse<SessionResult>> GetSessionResultBySessionKeyApi(int sessionKey);
        Task<DataResponse<SessionResult>> GetSessionResultBySessionKeyDatabase(int sessionKey);
        Task<Response> SaveSessionResults(List<SessionResult> sessionResult);
    }
}
