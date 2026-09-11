using Entities.Class;
using Shared.Responses;

namespace ExternalApi.Interfaces
{
    public interface ISessionClient
    {
        Task<DataResponse<Session>> GetSessionsByMeetingKey(int meetingKey);
    }
}
