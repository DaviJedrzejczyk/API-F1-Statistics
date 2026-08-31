using Dao.Interface;
using Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Responses;

namespace Dao.Impl
{
    public class PitDao : IPitDao
    {
        private ApiF1DB _db;

        public PitDao(ApiF1DB apiF1DB)
        {
            _db = apiF1DB;
        }

        public async Task<DataResponse<Pit>> GetAllPitsBySessionKey(int sessionKey)
        {
            try
            {
                List<Pit> pits = await _db.Pits.Where(x => x.SessionKey ==  sessionKey).AsNoTracking().ToListAsync();
                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(pits);
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<Pit>(ex);
            }
        }

        public async Task<Response> SavePits(List<Pit> pits)
        {
            try
            {
                await _db.Pits.AddRangeAsync(pits);
                return ResponseFactory.CreateInstance().CreateSuccessResponse("Success to insert all pits.");
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }
    }
}
