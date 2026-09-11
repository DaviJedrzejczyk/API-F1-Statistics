using Dao.Interface;
using Entities.Class;
using Microsoft.EntityFrameworkCore;
using Shared.Responses;

namespace Dao.Impl
{
    public class SessionResultDao : ISessionResultDao
    {
        private readonly ApiF1DB _db;
        public SessionResultDao(ApiF1DB apiF1DB)
        {
            _db = apiF1DB;
        }

        public async Task<DataResponse<SessionResult>> GetSessionResultsBySesssionKey(int sessionKey)
        {
            try
            {
                var list = await _db.SessionResults.Where(x => x.SessionKey == sessionKey).AsNoTracking().ToListAsync();
                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(list);
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResult>(ex);
            }
        }

        public async Task<Response> SaveSessionResults(List<SessionResult> sessionResults)
        {
            try
            {
                await _db.AddRangeAsync(sessionResults);
                return ResponseFactory.CreateInstance().CreateSuccessResponse("The session results has been saved.");
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }
    }
}
