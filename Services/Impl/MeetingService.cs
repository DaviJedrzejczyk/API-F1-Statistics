using Dao.Interface;
using Entities;
using Services.Interfaces;
using Shared.Responses;

namespace Services.Impl
{
    public class MeetingService : IMeetingService
    {
        private readonly IUnityOfWork _unityOfWork;
        public MeetingService(IUnityOfWork unityOfWork)
        {
            this._unityOfWork = unityOfWork;
        }

        public async Task<DataResponse<Meeting>> GetAllTracksOfCurrentYear(int year)
        {
            try
            {
                return await _unityOfWork.MeetingDao.GetAllTracksOfCurrentYear(year);
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
                if (meetingKey <= 0)
                    return ResponseFactory.CreateInstance().CreateFailureSingleResponse<Meeting>("Meeting key must be greater than 0");

                SingleResponse<Meeting> meeting = await _unityOfWork.MeetingDao.GetMeetingByKey(meetingKey);

                if (meeting.Item == null)
                    return ResponseFactory.CreateInstance().CreateFailureSingleResponse<Meeting>("No meeting found with the specified key.");

                return meeting;
                
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

                if (meetings.Count == 0)
                    return ResponseFactory.CreateInstance().CreateSuccessResponse("No new meetings to insert.");
                
                Response result = await _unityOfWork.MeetingDao.InsertTracksOfCurrentYear(meetings);

                if (!result.HasSuccess)
                   return ResponseFactory.CreateInstance().CreateFailureResponse(result.Message);

                result = await _unityOfWork.Commit();

                if (!result.HasSuccess)
                    return ResponseFactory.CreateInstance().CreateFailureResponse(result.Message, result.Exception);

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
