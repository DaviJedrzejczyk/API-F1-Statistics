using AutoMapper;
using Entities;
using ExternalApi.Interfaces;
using ExternalApi.ViewModels;
using Shared.Responses;
using System.Text.Json;

namespace ExternalApi.Impls
{
    public class CarDataClient : ICarDataClient
    {
        private readonly IF1ApiClient _f1ApiClient;
        private readonly IMapper _mapper;

        public CarDataClient(IF1ApiClient f1ApiClient, IMapper mapper)
        {
            _f1ApiClient = f1ApiClient;
            _mapper = mapper;
        }

        public async Task<DataResponse<CarData>> GetHighSpeedsSession(int sessionKey, int minimunSpeed)
        {
			try
			{
                SingleResponse<string> carData = await _f1ApiClient.Get("car_data?", $"session_key={sessionKey}&speed={minimunSpeed}");

                if (!carData.HasSuccess)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<CarData>(carData.Message, carData.Exception);

                List<CarDataDto>? carDataViewModels = JsonSerializer.Deserialize<List<CarDataDto>>(carData.Item) ?? throw new Exception("Failed to deserialize car data.");

                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(_mapper.Map<List<CarData>>(carDataViewModels));
            }
			catch (Exception ex)
			{
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<CarData>(ex);
			}
        }
    }
}
