using Dao.Interface;
using Entities;
using Shared.Responses;

namespace Dao.Impl
{
    public class SessionDao : ISessionDao
    {
        private readonly ApiF1DB _db;
        public SessionDao(ApiF1DB db)
        {
            _db = db;
        }

        public async Task<Response> InsertTracksOfCurrentYear(List<Session> sessions)
        {
            try
            {
                await _db.Sessions.AddRangeAsync(sessions);
                return ResponseFactory.CreateInstance().CreateSuccessResponse("All tracks inserted successfully.");
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }
    }
}
