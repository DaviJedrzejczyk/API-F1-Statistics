using Entities;
using Shared.Responses;

namespace ExternalApi.Interfaces
{
    public interface IDriverClient
    {
        Task<DataResponse<Driver>> GetAllDriversRecentMeeting(int meetingKey);
    }
}
