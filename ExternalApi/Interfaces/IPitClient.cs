using Entities;
using Shared.Responses;

namespace ExternalApi.Interfaces
{
    public interface IPitClient
    {
        Task<DataResponse<Pit>> GetAllPitsSession(int sessionKey);
    }
}
