using AutoMapper;
using Entities;
using Entities.Dtos;
using ExternalApi.Interfaces;
using Shared.Responses;
using System.Text.Json;

namespace ExternalApi.Impls
{
    public class PitClient : IPitClient
    {
        private readonly IF1ApiClient _client;
        private readonly IMapper _mapper;
        public PitClient(IF1ApiClient f1ApiClient, IMapper mapper)
        {
            _client = f1ApiClient;
            _mapper = mapper;
        }

        public async Task<DataResponse<Pit>> GetAllPitsSession(int sessionKey)
        {
            try
            {
                SingleResponse<string> response = await _client.Get("pit?", $"session_key={sessionKey}");
                if (!response.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureDataResponse<Pit>("Error to search the pits with this session key.");

                if (response.Item == null || response.Item == string.Empty) return ResponseFactory.CreateInstance().CreateFailureDataResponse<Pit>("The session dosen't contains pits.");

                List<PitDto>? pits = JsonSerializer.Deserialize<List<PitDto>>(response.Item);

                if (pits == null) return ResponseFactory.CreateInstance().CreateFailureDataResponse<Pit>("Cannot deserealize the JSON of pits.");

                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(_mapper.Map<List<Pit>>(pits));
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<Pit>(ex);
            }
        }
    }
}
