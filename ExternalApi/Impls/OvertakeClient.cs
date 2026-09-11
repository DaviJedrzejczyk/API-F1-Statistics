using AutoMapper;
using Entities.Class;
using Entities.Dtos;
using ExternalApi.Interfaces;
using Shared.Responses;
using System.Text.Json;

namespace ExternalApi.Impls
{
    public class OvertakeClient : IOvertakeClient
    {
        private readonly IF1ApiClient _client;
        private readonly IMapper _mapper;
        public OvertakeClient(IF1ApiClient f1ApiClient, IMapper mapper)
        {
            _client = f1ApiClient;
            _mapper = mapper;
        }

        public async Task<DataResponse<Overtake>> GetOvertakesSession(int sessionKey)
        {
            try
            {
                SingleResponse<string> response = await _client.Get("overtakes?", $"session_key={sessionKey}");
                if (!response.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureDataResponse<Overtake>(response.Message, response.Exception);

                List<OvertakeDto>? overtakes = JsonSerializer.Deserialize<List<OvertakeDto>>(response.Item);
                if (overtakes == null || overtakes.Count <= 0) return ResponseFactory.CreateInstance().CreateFailureDataResponse<Overtake>("Overtakes in this session not found!");

                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(_mapper.Map<List<Overtake>>(overtakes));

            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<Overtake>(ex);
            }
        }
    }
}
