using AutoMapper;
using Entities.Class;
using Entities.Dtos;
using ExternalApi.Interfaces;
using Shared.Responses;
using Shared.Common.Atrributes;
using System.Text.Json;

namespace ExternalApi.Impls
{
    [IncludeDependencyInjection]
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
            => await GetAllStintsBySession(sessionKey, CancellationToken.None);

        public async Task<DataResponse<Stint>> GetAllStintsBySession(int sessionKey, CancellationToken cancellationToken)
        {
            try
            {
                SingleResponse<string> response = await _api.Get("stints?", $"session_key={sessionKey}", cancellationToken);
                if (!response.HasSuccess)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<Stint>(response.Message, response.Exception);

                List<StintListDTO>? stList = JsonSerializer.Deserialize<List<StintListDTO>>(response.Item);
                if (stList == null)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<Stint>("No stints found for the given session key.");

                return ResponseFactory.CreateInstance().CreateSuccessDataResponse<Stint>(_mapper.Map<List<Stint>>(stList));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<Stint>(ex);
            }
        }
    }
}
