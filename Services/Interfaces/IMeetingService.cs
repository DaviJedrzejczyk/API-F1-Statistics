using Entities.Class;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Services.Interfaces
{
    public interface IMeetingService
    {
        Task<Response> InsertTracksOfCurrentYear(int year);
        Task<DataResponse<Meeting>> GetAllTracksOfCurrentYear(int year);
        Task<SingleResponse<Meeting>> GetMeetingByKey(int meetingKey);
        Task<SingleResponse<int>> GetRecentMeetingKey();
    }
}
