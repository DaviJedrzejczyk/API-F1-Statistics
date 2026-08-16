using Entities;
using Shared.Responses;

namespace Services.Interfaces
{
    public interface IMeetingService
    {
        Task<Response> InsertTracksOfCurrentYear(List<Meeting> sessions);
        Task<DataResponse<Meeting>> GetAllTracksOfCurrentYear(int year);
        Task<SingleResponse<Meeting>> GetMeetingByKey(int meetingKey);
    }
}
