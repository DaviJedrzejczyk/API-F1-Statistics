using AutoMapper;
using Entities;
using ExternalApi.Interfaces;
using ExternalApi.ViewModels;
using Shared.Responses;
using System.Text.Json;

namespace ExternalApi.Impls
{
    public class SessionClient : ISessionClient
    {
        private readonly IF1ApiClient _f1ApiClient;
        private readonly IMapper _mapper;

        public SessionClient(IF1ApiClient f1ApiClient, IMapper mapper)
        {
            _f1ApiClient = f1ApiClient;
            _mapper = mapper;
        }

        public async Task<DataResponse<Session>> GetSessionsByMeetingKey(int meetingKey)
        {
            try
            {
                SingleResponse<String> response = await _f1ApiClient.Get("sessions?", $"meeting_key={meetingKey}");

                if (!response.HasSuccess)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<Session>(response.Message, response.Exception);

                List<SessionViewModel> sessions = JsonSerializer.Deserialize<List<SessionViewModel>>(response.Item);

                return ResponseFactory.CreateInstance().CreateSuccessDataResponse<Session>(_mapper.Map<List<Session>>(sessions));

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error occurred while fetching sessions.", ex);
            }
        }
    }
}
