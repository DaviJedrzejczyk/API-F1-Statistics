using Entities;
using Shared.Responses;

namespace ExternalApi.Interfaces
{
    public interface ISessionResultClient
    {
        Task<DataResponse<SessionResult>> GetSessionResultApi(int sessionKey);
    }
}
