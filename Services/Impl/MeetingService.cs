using Dao.Interface;
using Entities;
using Services.Interfaces;
using Shared.Responses;

namespace Services.Impl
{
    public class MeetingService : IMeetingService
    {
        private readonly IMeetingDao _meetingDao;
        public MeetingService(IMeetingDao meetingDao)
        {
            this._meetingDao = meetingDao;
        }

        public async Task<DataResponse<Meeting>> GetAllTracksOfCurrentYear(int year)
        {
            try
            {
                return await _meetingDao.GetAllTracksOfCurrentYear(year);
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
                return await _meetingDao.GetMeetingByKey(meetingKey);
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
                for (int i = 0; i < meetings.Count; i++)
                {
                    Meeting meeting = meetings[i];
                    SingleResponse<Meeting> response = await GetMeetingByKey(meeting.MeetingKey) ?? throw new Exception("No meeting found with the specified key.");
                    if (response.Item != null)
                    {
                        meetings.Remove(meeting);
                        i--;
                    }
                }

                var result = await _meetingDao.InsertTracksOfCurrentYear(meetings);
                result.Message = "All meetings inserted successfully.";
                return result;
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }
    }
}
