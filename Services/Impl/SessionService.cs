using Dao.Interface;
using Entities.Class;
using Entities.Dtos;
using ExternalApi.Impls;
using ExternalApi.Interfaces;
using Services.Interfaces;
using Shared.Responses;
using Shared.Common.Atrributes;
using System.Runtime.ExceptionServices;

namespace Services.Impl
{
    [IncludeDependencyInjection]
    public class SessionService : ISessionService
    {
        private readonly IUnityOfWork _unityOfWork;
        private readonly ISessionClient _sessionClient;
        private readonly IMeetingService _meetingService;

        public SessionService(IUnityOfWork unityOfWork, ISessionClient sessionClient, IMeetingService meetingService) 
        {
            _unityOfWork = unityOfWork;
            _sessionClient = sessionClient;
            _meetingService = meetingService;
        }

        public async Task<SingleResponse<Session>> GetSessionByMeetingKeySessionKey(int meetingKey, int sessionKey)
            => await GetSessionByMeetingKeySessionKey(meetingKey, sessionKey, CancellationToken.None);

        public async Task<SingleResponse<Session>> GetSessionByMeetingKeySessionKey(int meetingKey, int sessionKey, CancellationToken cancellationToken)
        {
            try
            {
                SingleResponse<Session> response = await _unityOfWork.SessionDao.GetSessionByMeetingKeySessionKey(meetingKey, sessionKey, cancellationToken);

                if (response.Item == null)
                    return ResponseFactory.CreateInstance().CreateFailureSingleResponse<Session>("Session not found.");

                return ResponseFactory.CreateInstance().CreateSuccessSingleResponse<Session>(response.Item);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureSingleResponse<Session>("Error occurred while fetching session: " + ex.Message, ex);
            }
        }

        public async Task<Response> InsertSessions(int meetingKey)
            => await InsertSessions(meetingKey, CancellationToken.None);

        public async Task<Response> InsertSessions(int meetingKey, CancellationToken cancellationToken)
        {
            try
            {
                var key = meetingKey.ToString(System.Globalization.CultureInfo.InvariantCulture);
                var cached = await ImportWindowCache.Read<Session>(_unityOfWork, "sessions", key, cancellationToken);
                if (cached != null) return ResponseFactory.CreateInstance().CreateSuccessResponse("Sessions inserted successfully.");
                DataResponse<Session> sessions = await _sessionClient.GetSessionsByMeetingKey(meetingKey, cancellationToken);
                
                if (!sessions.HasSuccess || sessions.Itens == null)
                    return ResponseFactory.CreateInstance().CreateFailureResponse("Failed to fetch sessions: " + sessions.Message, sessions.Exception);
                if (sessions.Itens.Any(x => x.MeetingKey != meetingKey) || sessions.Itens.Select(x => x.SessionKey).Distinct().Count() != sessions.Itens.Count)
                    return ResponseFactory.CreateInstance().CreateFailureResponse("Invalid session window returned by upstream.");
                await using var transaction = await _unityOfWork.BeginImport("sessions", key, cancellationToken);
                cached = await ImportWindowCache.Read<Session>(_unityOfWork, "sessions", key, cancellationToken);
                if (cached != null) return ResponseFactory.CreateInstance().CreateSuccessResponse("Sessions inserted successfully.");
                var db = await _unityOfWork.SessionDao.GetSessionsByKeys(sessions.Itens.Select(x => x.SessionKey).ToList(), cancellationToken);
                if (!db.HasSuccess || db.Itens == null) return ResponseFactory.CreateInstance().CreateFailureResponse(db.Message, db.Exception);
                if (db.Itens.Any(x => x.MeetingKey != meetingKey))
                    return ResponseFactory.CreateInstance().CreateFailureResponse("An existing session belongs to another meeting.");
                var keys = db.Itens.Select(x => x.SessionKey).ToHashSet();
                var missing = sessions.Itens.Where(x => !keys.Contains(x.SessionKey)).ToList();
                if (missing.Count > 0)
                {
                    var insert = await _unityOfWork.SessionDao.InsertSessions(missing, cancellationToken);
                    if (!insert.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureResponse("Failed to insert sessions.", insert.Exception);
                }
                await ImportWindowCache.Stage(_unityOfWork, "sessions", key, sessions.Itens, cancellationToken);
                var response = await _unityOfWork.Commit(cancellationToken);
                if (!response.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureResponse("Failed to commit session.", response.Exception);
                await transaction.CommitAsync(cancellationToken);
                return ResponseFactory.CreateInstance().CreateSuccessResponse("Sessions inserted successfully.");
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }

        public async Task<Response> ReturnAlreadyHaveSession(int meetingKey)
            => await ReturnAlreadyHaveSession(meetingKey, CancellationToken.None);

        public async Task<Response> ReturnAlreadyHaveSession(int meetingKey, CancellationToken cancellationToken)
        {
            try
            {
                return await _unityOfWork.SessionDao.GetAlreadyHaveSession(meetingKey, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }

        /// <summary>
        /// This method just been call by the scheduler to update the recent session, it will be called every week. DO NOT CALL IN ANY OTHER CONTEXT.
        /// </summary>
        /// <returns></returns>
        public async Task<Response> UpdateRecentSession()
            => await UpdateRecentSession(CancellationToken.None);

        public async Task<Response> UpdateRecentSession(CancellationToken cancellationToken)
        {
            try
            {
                var meetingKeyResponse = await _meetingService.GetRecentMeetingKey(cancellationToken);
                if (!meetingKeyResponse.HasSuccess)
                    return ResponseFactory.CreateInstance().CreateFailureResponse("An error has occurred when fetching the recent meeting key: " + meetingKeyResponse.Message, meetingKeyResponse.Exception);

                if (meetingKeyResponse.Item == 0)
                    return ResponseFactory.CreateInstance().CreateFailureResponse("No recent meeting key found.");

                Response response = await InsertSessions(meetingKeyResponse.Item, cancellationToken);
                if (!response.HasSuccess)
                    return ResponseFactory.CreateInstance().CreateFailureResponse("An error has occurred when inserting sessions: " + response.Message, response.Exception);

                return ResponseFactory.CreateInstance().CreateSuccessResponse("Recent session updated successfully.");
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (TaskCanceledException tex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse("Request timed out or was canceled: " + tex.Message, tex);
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }
    }
}
