using Entities.Dtos;
using ExternalApi.Interfaces;
using Shared.Common.Atrributes;
using Shared.Responses;
using System.Text.Json;

namespace ExternalApi.Impls
{
    [IncludeDependencyInjection]
    public class LapClient : ILapClient
    {
        private readonly IF1ApiClient _f1ApiClient;
        public LapClient(IF1ApiClient f1ApiClient)
        {
            _f1ApiClient = f1ApiClient;
        }
        public async Task<DataResponse<LapListDto>> GetAllLapsSessionByDriver(int sessionKey, int driverNumber)
        {
            try
            {
                SingleResponse<string> response = await _f1ApiClient.Get("laps?", "session_key=" + sessionKey + "&driver_number=" + driverNumber);
                if (!response.HasSuccess) 
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<LapListDto>(response.Message, response.Exception);

                List<LapListDto>? lapList = JsonSerializer.Deserialize<List<LapListDto>>(response.Item);

                if (lapList == null)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<LapListDto>("Failed to deserialize lap list.");

                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(lapList);
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<LapListDto>(ex);
            }
        }
    }
}
