using AutoMapper;
using Entities;
using ExternalApi.Interfaces;
using ExternalApi.ViewModels;
using Shared.Responses;
using System.Text.Json;

namespace ExternalApi.Impls
{
    public class MeetingClient : IMeetingClient
    {
        private readonly IF1ApiClient _f1ApiClient;
        private readonly IMapper _mapper;

        public MeetingClient(IF1ApiClient f1ApiClient, IMapper mapper)
        {
            _f1ApiClient = f1ApiClient;
            _mapper = mapper;
        }

        public async Task<DataResponse<Meeting>> GetMeetingsByYear(int year)
        {
            try
            {
                SingleResponse<string> tracks = await _f1ApiClient.Get("meetings", "year=" + year.ToString());

                if (!tracks.HasSuccess)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<Meeting>(tracks.Message, tracks.Exception);

                List<MeetingViewModel>? meetingViewModels = JsonSerializer.Deserialize<List<MeetingViewModel>>(tracks.Item) ?? throw new Exception("Failed to deserialize tracks.");

                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(_mapper.Map<List<Meeting>>(meetingViewModels));
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<Meeting>("An error occurred while retrieving meetings.", ex);
            }
           
        }
    }
}
