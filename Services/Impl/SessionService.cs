using Dao.Interface;
using Entities;
using Services.Interfaces;
using Shared.Responses;

namespace Services.Impl
{
    public class SessionService : ISessionService
    {
        private readonly IUnityOfWork _unityOfWork;
        public SessionService(IUnityOfWork unityOfWork) => _unityOfWork = unityOfWork;

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

        public async Task<Response> InsertSessions(List<Session> sessions)
        {
            try
            {
                Response response = await _unityOfWork.SessionDao.InsertSessions(sessions);

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
