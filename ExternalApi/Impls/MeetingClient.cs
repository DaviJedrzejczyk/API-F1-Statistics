using AutoMapper;
using Entities.Class;
using Entities.Dtos;
using ExternalApi.Interfaces;
using Shared.Common.Atrributes;
using Shared.Responses;
using System.Text.Json;

namespace ExternalApi.Impls
{
    [IncludeDependencyInjection]
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
            => await GetMeetingsByYear(year, CancellationToken.None);

        public async Task<DataResponse<Meeting>> GetMeetingsByYear(int year, CancellationToken cancellationToken)
        {
            try
            {
                SingleResponse<string> tracks = await _f1ApiClient.Get("meetings?", "year=" + year.ToString(), cancellationToken);

                if (!tracks.HasSuccess)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<Meeting>(tracks.Message, tracks.Exception);

                List<MeetingDto>? meetingViewModels = JsonSerializer.Deserialize<List<MeetingDto>>(tracks.Item) ?? throw new Exception("Failed to deserialize tracks.");

                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(_mapper.Map<List<Meeting>>(meetingViewModels));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<Meeting>("An error occurred while retrieving meetings.", ex);
            }
           
        }
    }
}
