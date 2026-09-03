using AutoMapper;
using Entities;
using ExternalApi.Interfaces;
using ExternalApi.ViewModels;
using Shared.Responses;
using System.Text.Json;

namespace ExternalApi.Impls
{
    public class RaceControlClient : IRaceControlClient
    {
        private readonly IF1ApiClient _client;
        private readonly IMapper _mapper;
        public RaceControlClient(IF1ApiClient f1ApiClient, IMapper mapper)
        {
            _client = f1ApiClient;
            _mapper = mapper;
        }

        public async Task<DataResponse<RaceControl>> GetRaceControlsBySession(int sessionKey)
        {
            try
            {
                SingleResponse<string> response = await _client.Get("race_control?", $"session_key={sessionKey}");
                if (!response.HasSuccess || response.Item == null)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<RaceControl>(response.Message, response.Exception);

                List<RaceControlDto>? raceControls = JsonSerializer.Deserialize<List<RaceControlDto>>(response.Item);
                if (raceControls == null)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<RaceControl>("Failed to deserialize race controls");

                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(_mapper.Map<List<RaceControl>>(raceControls));
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<RaceControl>(ex);
            }
        }
    }
}
