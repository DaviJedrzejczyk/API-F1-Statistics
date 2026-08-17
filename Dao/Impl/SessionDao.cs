using Dao.Interface;
using Entities;
using Microsoft.EntityFrameworkCore;
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

        public async Task<SingleResponse<Session>> GetSessionByMeetingKeySessionKey(int meetingKey, int sessionKey)
        {
            try
            {
                var session = await _db.Sessions.FirstOrDefaultAsync(s => s.SessionKey == sessionKey && s.MeetingKey == meetingKey);

                if (session == null) 
                    return ResponseFactory.CreateInstance().CreateFailureSingleResponse<Session>($"Session with Meeting Key {meetingKey} and Session Key {sessionKey}, not found.");
                
                return ResponseFactory.CreateInstance().CreateSuccessSingleResponse<Session>(session);
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureSingleResponse<Session>(ex);
            }
        }

        public async Task<Response> InsertSessions(List<Session> sessions)
        {
            try
            {
                await _db.Sessions.AddRangeAsync(sessions);
                return ResponseFactory.CreateInstance().CreateSuccessResponse("The sessions were inserted successfully.");
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }
    }
}
