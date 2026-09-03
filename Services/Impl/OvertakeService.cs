using Dao.Interface;
using Entities;
using ExternalApi.Interfaces;
using Services.Interfaces;
using Shared.Responses;

namespace Services.Impl
{
    public class OvertakeService : IOvertakeService
    {
        private readonly IOvertakeClient _client;
        private readonly IUnityOfWork _unityOfWork;
        private readonly IPitService _pitService;

        public OvertakeService(IOvertakeClient overtakeClient, IUnityOfWork unityOfWork, IPitService pitService)
        {
            _client = overtakeClient;
            _unityOfWork = unityOfWork;
            _pitService = pitService;
        }

        public async Task<DataResponse<Overtake>> GetOvertakesSessionApi(int sessionKey)
        {
            try
            {
                DataResponse<Overtake> responseApi = await _client.GetOvertakesSession(sessionKey);
                if (!responseApi.HasSuccess || (responseApi.Itens == null || responseApi.Itens.Count == 0))
                    return responseApi;

                DataResponse<Pit> pitsResponse = await _pitService.GetPitsBySessionKeyApi(sessionKey);

                List<Overtake> overtakesFiltered = responseApi.Itens.Where(overtake =>
                {
                    // Exclude overtakes that happen within a 23-second window of a pit for either driver
                    bool isOvertakingDriverInPit = pitsResponse.Itens.Any(pit =>
                        pit.DriverNumber == overtake.OvertakingDriverNumber &&
                        (pit.Date - overtake.Date).Duration() <= TimeSpan.FromSeconds(23));

                    bool isOvertakenDriverInPit = pitsResponse.Itens.Any(pit =>
                        pit.DriverNumber == overtake.OvertakedDriverNumber &&
                        (pit.Date - overtake.Date).Duration() <= TimeSpan.FromSeconds(23));

                    return !isOvertakingDriverInPit && !isOvertakenDriverInPit;
                }).ToList();

                Response responseSave = await SaveOvertakes(overtakesFiltered);
                if (!responseSave.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureDataResponse<Overtake>(responseSave.Message, responseSave.Exception);

                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(overtakesFiltered);
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<Overtake>(ex);
            }
        }

        public async Task<DataResponse<Overtake>> GetOvertakesSessionDb(int sessionKey)
        {
            try
            {
                var overtakes = await _unityOfWork.OvertakeDao.GetOvertakesBySession(sessionKey);

                if (overtakes.Itens == null || overtakes.Itens.Count == 0)
                {
                    overtakes.Message = "The overtakes of session not found or dosent have.";
                    return overtakes;
                }

                return overtakes;
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<Overtake>(ex);
            }
        }

        public async Task<Response> SaveOvertakes(List<Overtake> overtakes)
        {
            try
            {
                if (overtakes == null || overtakes.Count == 0) return ResponseFactory.CreateInstance().CreateFailureResponse("Overtakes must be informed.");
                
                var response = await _unityOfWork.OvertakeDao.SaveOvertakes(overtakes);
                if (!response.HasSuccess) return response;

                return await _unityOfWork.Commit();
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }
    }
}
