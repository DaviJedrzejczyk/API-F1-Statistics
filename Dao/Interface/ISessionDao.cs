using Entities.Class;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Dao.Interface
{
    [IncludeDependencyInjection]
    public interface ISessionDao
    {
        Task<Response> InsertSessions(List<Session> sessions);
        Task<SingleResponse<Session>> GetSessionByMeetingKeySessionKey(int meetingKey, int sessionKey);
    }
}
