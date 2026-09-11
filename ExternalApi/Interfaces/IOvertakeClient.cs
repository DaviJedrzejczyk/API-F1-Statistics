using Entities.Class;
using Shared.Responses;

namespace ExternalApi.Interfaces
{
    public interface IOvertakeClient
    {
        Task<DataResponse<Overtake>> GetOvertakesSession(int sessionKey);
    }
}
