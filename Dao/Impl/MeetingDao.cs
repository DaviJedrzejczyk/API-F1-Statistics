using Dao.Interface;
using Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Responses;

namespace Dao.Impl
{
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
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<Meeting>(ex);
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
                return ResponseFactory.CreateInstance().CreateFailureSingleResponse<Meeting>(ex);
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
                return ResponseFactory.CreateInstance().CreateFailureResponse("Failed to insert meetings.");
            }
        }
    }
}
