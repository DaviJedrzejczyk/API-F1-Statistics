using Entities.Class;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Services.Interfaces
{
    public interface IMeetingService
    {
        Task<Response> InsertTracksOfCurrentYear(int year);
        Task<Response> InsertTracksOfCurrentYear(int year, CancellationToken cancellationToken);
        Task<DataResponse<Meeting>> GetAllTracksOfCurrentYear(int year);
        Task<DataResponse<Meeting>> GetAllTracksOfCurrentYear(int year, CancellationToken cancellationToken);
        Task<SingleResponse<Meeting>> GetMeetingByKey(int meetingKey);
        Task<SingleResponse<Meeting>> GetMeetingByKey(int meetingKey, CancellationToken cancellationToken);
        Task<SingleResponse<int>> GetRecentMeetingKey();
        Task<SingleResponse<int>> GetRecentMeetingKey(CancellationToken cancellationToken);
    }
}
