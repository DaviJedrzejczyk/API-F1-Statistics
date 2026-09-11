using Entities.Class;
using Shared.Responses;

namespace ExternalApi.Interfaces
{
    public interface IMeetingClient
    {
        Task<DataResponse<Meeting>> GetMeetingsByYear(int year);
    }
}
