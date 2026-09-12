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
    public class SessionResultClient : ISessionResultClient
    {
        private readonly IF1ApiClient _f1ApiClient;
        private readonly IMapper _mapper;
        public SessionResultClient(IF1ApiClient f1ApiClient, IMapper mapper)
        {
            _f1ApiClient = f1ApiClient;
            _mapper = mapper;
        }
        public async Task<DataResponse<SessionResultDto>> GetSessionResultApi(int sessionKey)
        {
            try
            {
                SingleResponse<string> response = await _f1ApiClient.Get("session_result?", $"session_key={sessionKey}");
                if (!response.HasSuccess || response.Item == null) return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResultDto>(response.Message, response.Exception);
                
                List<SessionResultDto>? dto = JsonSerializer.Deserialize<List<SessionResultDto>>(response.Item); 
                if (dto == null ) return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResultDto>($"The result of session: {sessionKey}, was not found!");

                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(dto);
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResultDto>(ex);
            }
        }
    }
}
