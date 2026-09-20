using Entities.Class;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Services.Interfaces
{
    [IncludeDependencyInjection]
    public interface ISessionService
    {
        Task<Response> InsertSessions(int meetingKey);
        Task<SingleResponse<Session>> GetSessionByMeetingKeySessionKey(int meetingKey, int sessionKey);
        Task<Response> ReturnAlreadyHaveSession(int meetingKey);
        Task<Response> UpdateRecentSession();
    }
}
