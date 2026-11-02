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
    public class DriverClient : IDriverClient
    {
        private readonly IF1ApiClient _f1ApiClient;
        private readonly IMapper _mapper;

        public DriverClient(IF1ApiClient f1ApiClient, IMapper mapper)
        {
            _f1ApiClient = f1ApiClient;
            _mapper = mapper;
        }

        public async Task<DataResponse<Driver>> GetAllDriversSessionSelected(DriverInsertDTO driverInsertDTO)
            => await GetAllDriversSessionSelected(driverInsertDTO, CancellationToken.None);

        public async Task<DataResponse<Driver>> GetAllDriversSessionSelected(DriverInsertDTO driverInsertDTO, CancellationToken cancellationToken)
        {
			try
			{
                SingleResponse<string> tracks = await _f1ApiClient.Get("drivers?", $"session_key={driverInsertDTO.SessionKey}", cancellationToken);

                if (!tracks.HasSuccess)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<Driver>(tracks.Message, tracks.Exception);

                List<DriverDto>? driverViewModels = JsonSerializer.Deserialize<List<DriverDto>>(tracks.Item) ?? throw new Exception("Failed to deserialize driver(s).");

                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(_mapper.Map<List<Driver>>(driverViewModels));
            }
			catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
			catch (Exception ex)
			{
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<Driver>(ex);
			}
        }
    }
}
