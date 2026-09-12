using Dao.Interface;
using Entities.Class;
using Microsoft.EntityFrameworkCore;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Dao.Impl
{
    [IncludeDependencyInjection]
    public class OvertakeDao : IOvertakeDao
    {
        private readonly ApiF1DB _db;
        public OvertakeDao(ApiF1DB apiF1DB)
        {
            _db = apiF1DB;
        }

        public async Task<DataResponse<Overtake>> GetOvertakesBySession(int sessionKey)
        {
            try
            {
                List<Overtake> overtakes = await _db.Overtakes.Where(x => x.SessionKey == sessionKey).AsNoTracking().ToListAsync();
                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(overtakes);
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<Overtake>(ex);
            }
        }

        public async Task<Response> SaveOvertakes(List<Overtake> overtakes)
        {
            try
            {
                await _db.AddRangeAsync(overtakes);
                return ResponseFactory.CreateInstance().CreateSuccessResponse("The overtakes has been inserted.");
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }
    }
}
