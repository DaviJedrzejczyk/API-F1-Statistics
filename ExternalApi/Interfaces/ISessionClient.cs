using Entities.Class;
using Shared.Responses;

namespace ExternalApi.Interfaces
{
    public interface ISessionClient
    {
        Task<DataResponse<Session>> GetSessionsByMeetingKey(int meetingKey);
        Task<DataResponse<Session>> GetSessionsByMeetingKey(int meetingKey, CancellationToken cancellationToken);
    }
}
