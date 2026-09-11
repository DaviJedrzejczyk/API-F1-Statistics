using Entities.Class;
using Shared.Responses;

namespace Dao.Interface
{
    public interface IRaceControlDao
    {
        Task<DataResponse<RaceControl>> GetRaceControlsBySessionFlags(int sessionKey, string[] sessionFlags);
        Task<Response> SaveRaceControls(List<RaceControl> raceControls);
    }
}
