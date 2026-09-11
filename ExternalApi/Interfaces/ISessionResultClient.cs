using Entities.Dtos;
using Shared.Responses;

namespace ExternalApi.Interfaces
{
    public interface ISessionResultClient
    {
        Task<DataResponse<SessionResultDto>> GetSessionResultApi(int sessionKey);
    }
}
