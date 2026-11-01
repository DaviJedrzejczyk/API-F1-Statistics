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
            => await GetAllTracksOfCurrentYear(year, CancellationToken.None);

        public async Task<DataResponse<Meeting>> GetAllTracksOfCurrentYear(int year, CancellationToken cancellationToken)
        {
            try
            {
                List<Meeting> meetings = await _db.Meetings
                    .Where(m => m.Year == year && m.MeetingName.Contains("Grand Prix"))
                    .OrderBy(m => m.DateStart)
                    .ToListAsync(cancellationToken);
                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(meetings);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<Meeting>("Failed to fetch meetings: " + ex.Message, ex);
            }
        }

        public async Task<SingleResponse<Meeting>> GetMeetingByKey(int meetingKey)
            => await GetMeetingByKey(meetingKey, CancellationToken.None);

        public async Task<SingleResponse<Meeting>> GetMeetingByKey(int meetingKey, CancellationToken cancellationToken)
        {
            try
            {
                var meeting = await _db.Meetings.FindAsync(new object[] { meetingKey }, cancellationToken);
                return ResponseFactory.CreateInstance().CreateSuccessSingleResponse<Meeting>(meeting);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureSingleResponse<Meeting>("An error has occurred when fetching the meeting: " + ex.Message, ex);
            }
        }

        public async Task<SingleResponse<int>> GetRecentMeetingKey()
            => await GetRecentMeetingKey(CancellationToken.None);

        public async Task<SingleResponse<int>> GetRecentMeetingKey(CancellationToken cancellationToken)
        {
            try
            {
                int meetingKey = await _db.Meetings.Where(x => !x.IsCancelled && x.DateEnd >= DateTime.UtcNow && x.DateEnd <= DateTime.UtcNow.AddDays(7))
                                            .OrderBy(x => x.DateStart)
                                            .Select(x => x.MeetingKey)
                                            .FirstOrDefaultAsync(cancellationToken);

                return ResponseFactory.CreateInstance().CreateSuccessSingleResponse(meetingKey);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureSingleResponse<int>(ex);
            }
        }

        public async Task<Response> InsertTracksOfCurrentYear(List<Meeting> meetings)
            => await InsertTracksOfCurrentYear(meetings, CancellationToken.None);

        public async Task<Response> InsertTracksOfCurrentYear(List<Meeting> meetings, CancellationToken cancellationToken)
        {
            try
            {
                await _db.Meetings.AddRangeAsync(meetings, cancellationToken);
                return ResponseFactory.CreateInstance().CreateSuccessResponse("All tracks inserted successfully.");
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse("Failed to insert meetings: " + ex.Message, ex);
            }
        }
    }
}
