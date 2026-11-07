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
            => await GetAllTracksOfCurrentYear(year, CancellationToken.None);

        public async Task<DataResponse<Meeting>> GetAllTracksOfCurrentYear(int year, CancellationToken cancellationToken)
        {
            try
            {
                if (year.ToString().Length != 4)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<Meeting>("Invalid year format.");

                if (year < 2023 || year > DateTime.Now.Year)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<Meeting>("Year must be between 2023 and the current year.");

                return await _unityOfWork.MeetingDao.GetAllTracksOfCurrentYear(year, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<Meeting>(ex);
            }
        }

        public async Task<SingleResponse<Meeting>> GetMeetingByKey(int meetingKey)
            => await GetMeetingByKey(meetingKey, CancellationToken.None);

        public async Task<SingleResponse<Meeting>> GetMeetingByKey(int meetingKey, CancellationToken cancellationToken)
        {
            try
            {
                if (meetingKey <= 0)
                    return ResponseFactory.CreateInstance().CreateFailureSingleResponse<Meeting>("Meeting key must be greater than 0");

                SingleResponse<Meeting> meeting = await _unityOfWork.MeetingDao.GetMeetingByKey(meetingKey, cancellationToken);

                if (meeting.Item == null)
                    return ResponseFactory.CreateInstance().CreateFailureSingleResponse<Meeting>("No meeting found with the specified key.");

                return meeting;
                
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureSingleResponse<Meeting>(ex);
            }
            
        }

        public async Task<SingleResponse<int>> GetRecentMeetingKey()
            => await GetRecentMeetingKey(CancellationToken.None);

        public async Task<SingleResponse<int>> GetRecentMeetingKey(CancellationToken cancellationToken)
        {
            try
            {
                SingleResponse<int> response = await _unityOfWork.MeetingDao.GetRecentMeetingKey(cancellationToken);

                if (!response.HasSuccess)
                    return ResponseFactory.CreateInstance().CreateFailureSingleResponse<int>("An error has ocurred when try to find the last meeting: " + response.Message, response.Exception);

                return response;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {  
                return ResponseFactory.CreateInstance().CreateFailureSingleResponse<int>(ex);
            }
        }

        public async Task<Response> InsertTracksOfCurrentYear(int year)
            => await InsertTracksOfCurrentYear(year, CancellationToken.None);

        public async Task<Response> InsertTracksOfCurrentYear(int year, CancellationToken cancellationToken)
        {
            try
            {
                DataResponse<Meeting> meetingsResponse = await _meetingClient.GetMeetingsByYear(year, cancellationToken);
                if (!meetingsResponse.HasSuccess || meetingsResponse.Itens == null)
                    return ResponseFactory.CreateInstance().CreateFailureResponse("The request could not be completed.", meetingsResponse.Exception);

                for (int i = 0; i < meetingsResponse.Itens.Count; i++)
                {
                    Meeting meeting = meetingsResponse.Itens[i];
                    SingleResponse<Meeting> response = await GetMeetingByKey(meeting.MeetingKey, cancellationToken);
                    if (response.Exception != null) return ResponseFactory.CreateInstance().CreateFailureResponse(response.Message, response.Exception);
                    if (response.Item != null)
                    {
                        meetingsResponse.Itens.Remove(meeting);
                        i--;
                    }
                }

                if (meetingsResponse.Itens.Count == 0)
                    return ResponseFactory.CreateInstance().CreateSuccessResponse("No new meetings to insert.");
                
                Response result = await _unityOfWork.MeetingDao.InsertTracksOfCurrentYear(meetingsResponse.Itens, cancellationToken);

                if (!result.HasSuccess)
                   return ResponseFactory.CreateInstance().CreateFailureResponse(result.Message);

                result = await _unityOfWork.Commit(cancellationToken);

                if (!result.HasSuccess)
                    return ResponseFactory.CreateInstance().CreateFailureResponse(result.Message, result.Exception);

                result.Message = "All meetings inserted successfully.";
                return result;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }
    }
}
