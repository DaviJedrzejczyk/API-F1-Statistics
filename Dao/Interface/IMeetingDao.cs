using Entities;
using Shared.Responses;

namespace Dao.Interface
{
    public interface IMeetingDao
    {
        Task<Response> InsertTracksOfCurrentYear(List<Meeting> meetings);
        Task<DataResponse<Meeting>> GetAllTracksOfCurrentYear(int year);
        Task<SingleResponse<Meeting>> GetMeetingByKey(int meetingKey);
    }
}
