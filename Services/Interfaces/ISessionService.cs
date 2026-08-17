using Entities;
using Shared.Responses;

namespace Services.Interfaces
{
    public interface ISessionService
    {
        Task<Response> InsertSessions(List<Session> sessions);
        Task<SingleResponse<Session>> GetSessionByMeetingKeySessionKey(int meetingKey, int sessionKey);
    }
}
