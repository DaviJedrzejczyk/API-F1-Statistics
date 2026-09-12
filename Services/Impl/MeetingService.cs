using Dao.Interface;
using Entities.Class;
using ExternalApi.Interfaces;
using Services.Interfaces;
using Shared.Responses;
using Shared.Common.Atrributes;

namespace Services.Impl
{
    [IncludeDependencyInjection]
    public class MeetingService : IMeetingService
    {
        private readonly IUnityOfWork _unityOfWork;
        private readonly IMeetingClient _meetingClient;
        public MeetingService(IUnityOfWork unityOfWork, IMeetingClient meetingClient)
        {
            this._unityOfWork = unityOfWork;
            this._meetingClient = meetingClient;
        }

        public async Task<DataResponse<Meeting>> GetAllTracksOfCurrentYear(int year)
        {
            try
            {
                if (year.ToString().Length != 4)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<Meeting>("Invalid year format.");

                if (year < 1951 || year > DateTime.Now.Year)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<Meeting>("Year must be between 1951 and the current year.");

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

        public async Task<SingleResponse<int>> GetRecentMeetingKey()
        {
            try
            {
                SingleResponse<int> response = await _unityOfWork.MeetingDao.GetRecentMeetingKey();

                if (!response.HasSuccess)
                    return ResponseFactory.CreateInstance().CreateFailureSingleResponse<int>("An error has ocurred when try to find the last meeting: " + response.Message, response.Exception);

                return response;
            }
            catch (Exception ex)
            {  
                return ResponseFactory.CreateInstance().CreateFailureSingleResponse<int>(ex);
            }
        }

        public async Task<Response> InsertTracksOfCurrentYear(int year)
        {
            try
            {
                DataResponse<Meeting> meetingsResponse = await _meetingClient.GetMeetingsByYear(year);

                for (int i = 0; i < meetingsResponse.Itens.Count; i++)
                {
                    Meeting meeting = meetingsResponse.Itens[i];
                    SingleResponse<Meeting> response = await GetMeetingByKey(meeting.MeetingKey) ?? throw new Exception("No meeting found with the specified key.");
                    if (response.Item != null)
                    {
                        meetingsResponse.Itens.Remove(meeting);
                        i--;
                    }
                }

                if (meetingsResponse.Itens.Count == 0)
                    return ResponseFactory.CreateInstance().CreateSuccessResponse("No new meetings to insert.");
                
                Response result = await _unityOfWork.MeetingDao.InsertTracksOfCurrentYear(meetingsResponse.Itens);

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
