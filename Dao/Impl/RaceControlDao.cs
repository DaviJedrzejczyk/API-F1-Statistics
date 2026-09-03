using Dao.Interface;
using Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Responses;

namespace Dao.Impl
{
    public class RaceControlDao : IRaceControlDao
    {
        private readonly ApiF1DB _db;
        public RaceControlDao(ApiF1DB db)
        {
            _db = db;
        }

        public async Task<DataResponse<RaceControl>> GetRaceControlsBySessionFlags(int sessionKey, string[] sessionFlags)
        {
            try
            {
                var raceControls = await _db.RaceControls.Where(rc => rc.SessionKey == sessionKey && sessionFlags.Contains(rc.FlagType)).AsNoTracking().ToListAsync();
                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(raceControls);
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<RaceControl>("Error retrieving race controls by session flags.", ex);
            }
        }

        public async Task<Response> SaveRaceControls(List<RaceControl> raceControls)
        {
            try
            {
                await _db.RaceControls.AddRangeAsync(raceControls);
                return ResponseFactory.CreateInstance().CreateSuccessResponse("Race controls saved successfully");
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse("Error saving race controls", ex);
            }
        }
    }
}
