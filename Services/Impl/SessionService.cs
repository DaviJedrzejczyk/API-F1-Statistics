using Dao.Interface;
using Entities;
using ExternalApi.Impls;
using ExternalApi.Interfaces;
using Services.Interfaces;
using Shared.Responses;

namespace Services.Impl
{
    public class SessionService : ISessionService
    {
        private readonly IUnityOfWork _unityOfWork;
        private readonly ISessionClient _sessionClient;

        public SessionService(IUnityOfWork unityOfWork, ISessionClient sessionClient) 
        {
            _unityOfWork = unityOfWork;
            _sessionClient = sessionClient;
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

        public Task<DataResponse<Session>> HighSpeedDriversInSession(int meetingKey, int sessionKey)
        {
            //TODO: Começar a implementar depois de implementar as entidades CarData e o Driver  
            throw new NotImplementedException(); 
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
    }
}
