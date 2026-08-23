using AutoMapper;
using Entities;
using ExternalApi.Interfaces;
using ExternalApi.ViewModels;
using Shared.Responses;
using System.Text.Json;

namespace ExternalApi.Impls
{
    public class DriverClient : IDriverClient
    {
        private readonly IF1ApiClient _f1ApiClient;
        private readonly IMapper _mapper;

        public DriverClient(IF1ApiClient f1ApiClient, IMapper mapper)
        {
            _f1ApiClient = f1ApiClient;
            _mapper = mapper;
        }

        public async Task<DataResponse<Driver>> GetAllDriversRecentMeeting(int meetingKey)
        {
			try
			{
                SingleResponse<string> tracks = await _f1ApiClient.Get("drivers", "meetingKey=" + meetingKey.ToString());

                if (!tracks.HasSuccess)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<Driver>(tracks.Message, tracks.Exception);

                List<DriverViewModel>? driverViewModels = JsonSerializer.Deserialize<List<DriverViewModel>>(tracks.Item) ?? throw new Exception("Failed to deserialize driver(s).");

                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(_mapper.Map<List<Driver>>(driverViewModels));
            }
			catch (Exception ex)
			{
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<Driver>(ex);
			}
        }
    }
}
