using Entities;
using Shared.Responses;

namespace ExternalApi.Interfaces
{
    public interface IStintClient
    {
        Task<DataResponse<Stint>> GetAllStintsBySession(int sessionKey);
    }
}
