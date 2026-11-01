using Dao.Interface;
using Entities.Class;
using Entities.Dtos;
using ExternalApi.Interfaces;
using Services.Interfaces;
using Shared.Responses;
using Shared.Common.Atrributes;

namespace Services.Impl
{
    [IncludeDependencyInjection]
    public class OvertakeService : IOvertakeService
    {
        private readonly IOvertakeClient _client;
        private readonly IUnityOfWork _unityOfWork;
        private readonly IPitService _pitService;
        private readonly IRaceControlService _raceControlService;

        public OvertakeService(IOvertakeClient overtakeClient, IUnityOfWork unityOfWork, IPitService pitService, IRaceControlService raceControlService)
        {
            _client = overtakeClient;
            _unityOfWork = unityOfWork;
            _pitService = pitService;
            _raceControlService = raceControlService;
        }

        public async Task<DataResponse<Overtake>> GetOvertakesSessionApi(int sessionKey)
            => await GetOvertakesSessionApi(sessionKey, CancellationToken.None);

        public async Task<DataResponse<Overtake>> GetOvertakesSessionApi(int sessionKey, CancellationToken cancellationToken)
        {
            try
            {
                var key = sessionKey.ToString(System.Globalization.CultureInfo.InvariantCulture);
                var cached = await ImportWindowCache.Read<Overtake>(_unityOfWork, "overtakes", key, cancellationToken);
                if (cached != null) return WindowResult(cached);
                var responseApi = await _client.GetOvertakesSession(sessionKey, cancellationToken);
                if (!responseApi.HasSuccess || responseApi.Itens == null) return responseApi;
                if (responseApi.Itens.Any(x => x.SessionKey != sessionKey) ||
                    responseApi.Itens.Select(x => (x.MeetingKey, x.SessionKey, x.OvertakingDriverNumber, x.OvertakedDriverNumber, x.Date, x.Position)).Distinct().Count() != responseApi.Itens.Count)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<Overtake>("Invalid overtake window returned by upstream.");
                await using var transaction = await _unityOfWork.BeginImport("overtakes", key, cancellationToken);
                cached = await ImportWindowCache.Read<Overtake>(_unityOfWork, "overtakes", key, cancellationToken);
                if (cached != null) return WindowResult(cached);
                var overtakesFiltered = responseApi.Itens;
                if (overtakesFiltered.Count > 0)
                {
                    var pits = await _pitService.GetPitsBySessionKeyApi(sessionKey, cancellationToken);
                    if (!pits.HasSuccess || pits.Itens == null)
                        return ResponseFactory.CreateInstance().CreateFailureDataResponse<Overtake>(pits.Message, pits.Exception);
                    overtakesFiltered = FilterOvertakesMadeInPits(overtakesFiltered, pits.Itens);
                    var controls = await _raceControlService.GetRaceControlsBySessionFlags(sessionKey, ["YELLOW", "DOUBLE YELLOW", "CLEAR"], cancellationToken);
                    if (!controls.HasSuccess || controls.Itens == null)
                        return ResponseFactory.CreateInstance().CreateFailureDataResponse<Overtake>(controls.Message, controls.Exception);
                    overtakesFiltered = FilterOvertakesMadeInYellowFlags(overtakesFiltered, controls);
                }
                var db = await _unityOfWork.OvertakeDao.GetOvertakesBySession(sessionKey, cancellationToken);
                if (!db.HasSuccess || db.Itens == null) return db;
                var existing = db.Itens.Select(x => (x.MeetingKey, x.SessionKey, x.OvertakingDriverNumber, x.OvertakedDriverNumber, x.Date, x.Position)).ToHashSet();
                var missing = overtakesFiltered.Where(x => !existing.Contains((x.MeetingKey, x.SessionKey, x.OvertakingDriverNumber, x.OvertakedDriverNumber, x.Date, x.Position))).ToList();
                if (missing.Count > 0)
                {
                    var save = await _unityOfWork.OvertakeDao.SaveOvertakes(missing, cancellationToken);
                    if (!save.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureDataResponse<Overtake>(save.Message, save.Exception);
                }
                await ImportWindowCache.Stage(_unityOfWork, "overtakes", key, overtakesFiltered, cancellationToken);
                var commit = await _unityOfWork.Commit(cancellationToken);
                if (!commit.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureDataResponse<Overtake>(commit.Message, commit.Exception);
                await transaction.CommitAsync(cancellationToken);
                return WindowResult(overtakesFiltered);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<Overtake>(ex);
            }
        }

        private static DataResponse<Overtake> WindowResult(List<Overtake> items)
            => items.Count == 0
                ? ResponseFactory.CreateInstance().CreateFailureDataResponse<Overtake>("Overtakes in this session not found!")
                : ResponseFactory.CreateInstance().CreateSuccessDataResponse(items);

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

        private static List<Overtake> FilterOvertakesMadeInYellowFlags(List<Overtake> overtakesFiltered, DataResponse<RaceControlFilterDto> responseRaceControls)
        {
            overtakesFiltered = overtakesFiltered.Where(overtake =>
            {
                bool isDuringRaceControl = responseRaceControls.Itens.Any(raceControl => raceControl.DateStart <= overtake.Date && raceControl.DateEnd >= overtake.Date);

                return !isDuringRaceControl;
            }).ToList();

            return overtakesFiltered;
        }

        private static List<Overtake> FilterOvertakesMadeInPits(List<Overtake> overtakes, List<Pit> pits)
        {
            List<Overtake> overtakesFiltered = overtakes.Where(overtake =>
            {
                bool isOvertakingDriverInPit = pits.Any(pit =>
                    pit.DriverNumber == overtake.OvertakingDriverNumber &&
                    (pit.Date - overtake.Date).Duration() <= TimeSpan.FromSeconds(23));

                bool isOvertakenDriverInPit = pits.Any(pit =>
                    pit.DriverNumber == overtake.OvertakedDriverNumber &&
                    (pit.Date - overtake.Date).Duration() <= TimeSpan.FromSeconds(23));

                return !isOvertakingDriverInPit && !isOvertakenDriverInPit;
            }).ToList();

            return overtakesFiltered;
        }
    }
}
