using Entities;
using Shared.Responses;

namespace ExternalApi.Interfaces
{
    public interface IRaceControlClient
    {
        Task<DataResponse<RaceControl>> GetRaceControlsBySession(int sessionKey);
    }
}
