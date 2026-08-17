using Entities;
using Shared.Responses;

namespace Dao.Interface
{
    public interface ISessionDao
    {
        Task<Response> InsertSessions(List<Session> sessions);
        Task<SingleResponse<Session>> GetSessionByMeetingKeySessionKey(int meetingKey, int sessionKey);
    }
}
