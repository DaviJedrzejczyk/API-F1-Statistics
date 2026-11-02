using Dao.Interface;
using Entities.Class;
using Microsoft.EntityFrameworkCore;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Dao.Impl
{
    [IncludeDependencyInjection]
    public class SessionDao : ISessionDao
    {
        private readonly ApiF1DB _db;
        public SessionDao(ApiF1DB db)
        {
            _db = db;
        }

        public async Task<Response> GetAlreadyHaveSession(int meetingKey)
            => await GetAlreadyHaveSession(meetingKey, CancellationToken.None);

        public async Task<Response> GetAlreadyHaveSession(int meetingKey, CancellationToken cancellationToken)
        {
            try
            {
               var response = await _db.Sessions.AnyAsync(s => s.MeetingKey == meetingKey, cancellationToken);

               if (response)
                   return ResponseFactory.CreateInstance().CreateSuccessResponse("Session already exists.");
               else
                   return ResponseFactory.CreateInstance().CreateFailureResponse("Session does not exist.");
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }

        public async Task<SingleResponse<Session>> GetSessionByMeetingKeySessionKey(int meetingKey, int sessionKey)
            => await GetSessionByMeetingKeySessionKey(meetingKey, sessionKey, CancellationToken.None);

        public async Task<SingleResponse<Session>> GetSessionByMeetingKeySessionKey(int meetingKey, int sessionKey, CancellationToken cancellationToken)
        {
            try
            {
                var session = await _db.Sessions.FirstOrDefaultAsync(s => s.SessionKey == sessionKey && s.MeetingKey == meetingKey, cancellationToken);

                if (session == null) 
                    return ResponseFactory.CreateInstance().CreateFailureSingleResponse<Session>($"Session with Meeting Key {meetingKey} and Session Key {sessionKey}, not found.");
                
                return ResponseFactory.CreateInstance().CreateSuccessSingleResponse<Session>(session);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureSingleResponse<Session>(ex);
            }
        }

        public async Task<Response> InsertSessions(List<Session> sessions)
            => await InsertSessions(sessions, CancellationToken.None);

        public async Task<Response> InsertSessions(List<Session> sessions, CancellationToken cancellationToken)
        {
            try
            {
                await _db.Sessions.AddRangeAsync(sessions, cancellationToken);
                return ResponseFactory.CreateInstance().CreateSuccessResponse("The sessions were inserted successfully.");
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }

        public async Task<DataResponse<Session>> GetSessionsByKeys(List<int> sessionKeys, CancellationToken cancellationToken)
        {
            try
            {
                var items = await _db.Sessions.Where(x => sessionKeys.Contains(x.SessionKey)).AsNoTracking().ToListAsync(cancellationToken);
                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(items);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex) { return ResponseFactory.CreateInstance().CreateFailureDataResponse<Session>(ex); }
        }
    }
}
