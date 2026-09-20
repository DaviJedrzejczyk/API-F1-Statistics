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
        {
            try
            {
                SingleResponse<Session> response = await _unityOfWork.SessionDao.GetSessionByMeetingKeySessionKey(meetingKey, sessionKey);

                if (response.Item == null)
                    return ResponseFactory.CreateInstance().CreateFailureSingleResponse<Session>("Session not found.");

                return ResponseFactory.CreateInstance().CreateSuccessSingleResponse<Session>(response.Item);
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureSingleResponse<Session>("Error occurred while fetching session: " + ex.Message, ex);
            }
        }

        public async Task<Response> InsertSessions(int meetingKey)
        {
            try
            {
                DataResponse<Session> sessions = await _sessionClient.GetSessionsByMeetingKey(meetingKey);
                
                if (!sessions.HasSuccess)
                    return ResponseFactory.CreateInstance().CreateFailureResponse("Failed to fetch sessions: " + sessions.Message);

                Response response = await _unityOfWork.SessionDao.InsertSessions(sessions.Itens);

                if (!response.HasSuccess)
                    return ResponseFactory.CreateInstance().CreateFailureResponse("Failed to insert sessions.");

                response = await _unityOfWork.Commit();

                if (!response.HasSuccess)
                    return ResponseFactory.CreateInstance().CreateFailureResponse("Failed to commit session.");

                return ResponseFactory.CreateInstance().CreateSuccessResponse("Sessions inserted successfully.");
            }
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
        {
            try
            {
                var meetingKeyResponse = await _meetingService.GetRecentMeetingKey();
                if (!meetingKeyResponse.HasSuccess)
                    return ResponseFactory.CreateInstance().CreateFailureResponse("An error has occurred when fetching the recent meeting key: " + meetingKeyResponse.Message, meetingKeyResponse.Exception);

                if (meetingKeyResponse.Item == 0)
                    return ResponseFactory.CreateInstance().CreateFailureResponse("No recent meeting key found.");

                Response response = await InsertSessions(meetingKeyResponse.Item);
                if (!response.HasSuccess)
                    return ResponseFactory.CreateInstance().CreateFailureResponse("An error has occurred when inserting sessions: " + response.Message, response.Exception);

                return ResponseFactory.CreateInstance().CreateSuccessResponse("Recent session updated successfully.");
            }
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
