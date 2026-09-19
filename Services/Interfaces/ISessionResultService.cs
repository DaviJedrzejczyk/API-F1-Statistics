using Entities.Class;
using Entities.Dtos;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Services.Interfaces
{
    public interface ISessionResultService
    {
        Task<DataResponse<SessionResult>> GetSessionResultBySessionKeyApi(int sessionKey);
        Task<DataResponse<SessionResult>> GetSessionResultBySessionKeyDatabase(int sessionKey);
        Task<Response> SaveSessionResults(List<SessionResult> sessionResult);
    }
}
