using Entities;
using Entities.Dtos;
using Shared.Responses;

namespace Services.Interfaces
{
    public interface ISessionService
    {
        Task<Response> InsertSessions(int meetingKey);
        Task<SingleResponse<Session>> GetSessionByMeetingKeySessionKey(int meetingKey, int sessionKey);
    }
}
