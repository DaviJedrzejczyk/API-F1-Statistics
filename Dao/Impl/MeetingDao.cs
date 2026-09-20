using Dao.Interface;
using Entities.Class;
using Microsoft.EntityFrameworkCore;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Dao.Impl
{
    [IncludeDependencyInjection]
    public class MeetingDao : IMeetingDao
    {
        private readonly ApiF1DB _db;
        public MeetingDao(ApiF1DB db)
        {
            _db = db;
        }

        public async Task<DataResponse<Meeting>> GetAllTracksOfCurrentYear(int year)
        {
            try
            {
                List<Meeting> meetings = await _db.Meetings.Where(m => m.Year == year).ToListAsync();
                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(meetings);
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<Meeting>("Failed to fetch meetings: " + ex.Message, ex);
            }
        }

        public async Task<SingleResponse<Meeting>> GetMeetingByKey(int meetingKey)
        {
            try
            {
                var meeting = await _db.Meetings.FindAsync(meetingKey);
                return ResponseFactory.CreateInstance().CreateSuccessSingleResponse<Meeting>(meeting);
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureSingleResponse<Meeting>("An error has occurred when fetching the meeting: " + ex.Message, ex);
            }
        }

        public async Task<SingleResponse<int>> GetRecentMeetingKey()
        {
            try
            {
                int meetingKey = await _db.Meetings.Where(x => x.DateEnd >= DateTime.UtcNow && x.DateEnd <= DateTime.UtcNow.AddDays(7))
                                                   .OrderBy(x => x.DateStart)
                                                   .Select(x => x.MeetingKey)
                                                   .FirstOrDefaultAsync();

                return ResponseFactory.CreateInstance().CreateSuccessSingleResponse(meetingKey);
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureSingleResponse<int>(ex);
            }
        }

        public async Task<Response> InsertTracksOfCurrentYear(List<Meeting> meetings)
        {
            try
            {
                await _db.Meetings.AddRangeAsync(meetings);
                return ResponseFactory.CreateInstance().CreateSuccessResponse("All tracks inserted successfully.");
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse("Failed to insert meetings: " + ex.Message, ex);
            }
        }
    }
}
