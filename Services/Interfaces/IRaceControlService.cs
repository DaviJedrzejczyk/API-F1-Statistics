using Entities.Class;
using Entities.Dtos;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Services.Interfaces
{
    public interface IRaceControlService
    {
        Task<DataResponse<RaceControl>> GetRaceControlsBySessionFlagsApi(int sessionKey, string[] flags);
        Task<DataResponse<RaceControl>> GetRaceControlsBySessionFlagsDb(int sessionKey, string[] flags);
        Task<DataResponse<RaceControlFilterDto>> GetRaceControlsBySessionFlags(int sessionKey, string[] flags);
        Task<Response> SaveRaceControls(List<RaceControl> raceControls);

    }
}
