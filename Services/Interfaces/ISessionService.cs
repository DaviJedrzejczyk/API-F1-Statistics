using Entities.Class;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Services.Interfaces
{
    [IncludeDependencyInjection]
    public interface ISessionService
    {
        Task<Response> InsertSessions(int meetingKey);
        Task<Response> InsertSessions(int meetingKey, CancellationToken cancellationToken);
        Task<SingleResponse<Session>> GetSessionByMeetingKeySessionKey(int meetingKey, int sessionKey);
        Task<SingleResponse<Session>> GetSessionByMeetingKeySessionKey(int meetingKey, int sessionKey, CancellationToken cancellationToken);
        Task<Response> ReturnAlreadyHaveSession(int meetingKey);
        Task<Response> ReturnAlreadyHaveSession(int meetingKey, CancellationToken cancellationToken);
        Task<Response> UpdateRecentSession();
        Task<Response> UpdateRecentSession(CancellationToken cancellationToken);
    }
}
