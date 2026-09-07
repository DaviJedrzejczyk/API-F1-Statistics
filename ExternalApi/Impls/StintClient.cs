using AutoMapper;
using Entities;
using Entities.Dtos.StintDTOs;
using ExternalApi.Interfaces;
using Shared.Responses;
using System.Text.Json;

namespace ExternalApi.Impls
{
    public class StintClient : IStintClient
    {
        private readonly IF1ApiClient _api;
        private readonly IMapper _mapper;
        public StintClient(IF1ApiClient f1ApiClient, IMapper mapper)
        {
            _api = f1ApiClient;
            _mapper = mapper;
        }

        public async Task<DataResponse<Stint>> GetAllStintsBySession(int sessionKey)
        {
            try
            {
                SingleResponse<string> response = await _api.Get("stints?", $"session_key={sessionKey}");

                List<StintListDTO>? stList = JsonSerializer.Deserialize<List<StintListDTO>>(response.Item);
                if (stList == null)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<Stint>("No stints found for the given session key.");

                return ResponseFactory.CreateInstance().CreateSuccessDataResponse<Stint>(_mapper.Map<List<Stint>>(stList));
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<Stint>(ex);
            }
        }
    }
}
