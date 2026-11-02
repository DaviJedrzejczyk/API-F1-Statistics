using AutoMapper;
using Dao.Interface;
using Entities.Class;
using Entities.Dtos;
using ExternalApi.Interfaces;
using Services.Interfaces;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Services.Impl
{
    [IncludeDependencyInjection]
    public class LapService : ILapService
    {
        private readonly IUnityOfWork _unityOfWork;
        private readonly ILapClient _lapClient;
        private readonly IDriverService _driverService;
        private readonly IMapper _mapper;
        private readonly ILapFastSectorService _lapFastSectorService;

        public LapService(IUnityOfWork unityOfWork, ILapClient lapClient, IDriverService driverService, IMapper mapper, ILapFastSectorService lapFastSectorService)
        {
            _unityOfWork = unityOfWork;
            _lapClient = lapClient;
            _driverService = driverService;
            _mapper = mapper;
            _lapFastSectorService = lapFastSectorService;
        }

        public async Task<SingleResponse<LapFastLapDto>> GetFastLapOfRaceBySessionKey(int sessionKey)
            => await GetFastLapOfRaceBySessionKey(sessionKey, CancellationToken.None);

        public async Task<SingleResponse<LapFastLapDto>> GetFastLapOfRaceBySessionKey(int sessionKey, CancellationToken cancellationToken)
        {
            try
            {
                var key = sessionKey.ToString(System.Globalization.CultureInfo.InvariantCulture);
                var cached = await ImportWindowCache.Read<LapFastLapDto>(_unityOfWork, "fast_lap", key, cancellationToken);
                var cachedSectors = await ImportWindowCache.Read<LapFastSector>(_unityOfWork, "fast_sectors", key, cancellationToken);
                if (cached != null && cachedSectors != null) return LapWindowResult(cached);
                await using var transaction = await _unityOfWork.BeginImport("fast_lap", key, cancellationToken);
                cached = await ImportWindowCache.Read<LapFastLapDto>(_unityOfWork, "fast_lap", key, cancellationToken);
                cachedSectors = await ImportWindowCache.Read<LapFastSector>(_unityOfWork, "fast_sectors", key, cancellationToken);
                if (cached != null && cachedSectors != null) return LapWindowResult(cached);
                var drivers = await _driverService.GetAllDriversSession(sessionKey, cancellationToken);
                if (!drivers.HasSuccess || drivers.Itens == null)
                    return ResponseFactory.CreateInstance().CreateFailureSingleResponse<LapFastLapDto>(drivers.Message, drivers.Exception);
                var laps = new List<LapListDto>();
                foreach (var driver in drivers.Itens)
                {
                    var response = await _lapClient.GetAllLapsSessionByDriver(sessionKey, driver.DriverNumber, cancellationToken);
                    if (!response.HasSuccess || response.Itens == null)
                        return ResponseFactory.CreateInstance().CreateFailureSingleResponse<LapFastLapDto>(response.Message, response.Exception);
                    if (response.Itens.Any(x => x.SessionKey != sessionKey || x.DriverNumber != driver.DriverNumber))
                        return ResponseFactory.CreateInstance().CreateFailureSingleResponse<LapFastLapDto>("Invalid lap window returned by upstream.");
                    laps.AddRange(response.Itens);
                }
                var fastest = laps.Where(x => x.LapDuration > 0).MinBy(x => x.LapDuration);
                var sectors = new List<LapFastSector>();
                for (int sector = 1; sector <= 3; sector++)
                {
                    double Duration(LapListDto x) => sector == 1 ? x.DurationSector1 : sector == 2 ? x.DurationSector2 : x.DurationSector3;
                    var best = laps.Where(x => Duration(x) > 0).MinBy(Duration);
                    if (best == null) continue;
                    var driver = drivers.Itens.First(x => x.DriverNumber == best.DriverNumber);
                    if (string.IsNullOrEmpty(driver.LastName))
                        return ResponseFactory.CreateInstance().CreateFailureSingleResponse<LapFastLapDto>("Driver name not found for fastest sector.");
                    sectors.Add(new LapFastSector { SessionKey = sessionKey, DriverNumber = best.DriverNumber, DriverName = driver.LastName, Sector = sector, Duration = Duration(best) });
                }
                if (sectors.Count < 3) foreach (var sector in sectors) sector.Observation = "Not all fast sectors were found.";
                var result = new List<LapFastLapDto>();
                if (fastest != null)
                {
                    fastest.IsFastLap = true;
                    var saved = await _unityOfWork.LapDao.UpsertLap(_mapper.Map<Lap>(fastest), cancellationToken);
                    if (!saved.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureSingleResponse<LapFastLapDto>(saved.Message, saved.Exception);
                    result.Add(new LapFastLapDto { SessionKey = sessionKey, MeetingKey = fastest.MeetingKey, DriverNumber = fastest.DriverNumber, LapDuration = fastest.LapDuration, IsFastLap = true });
                }
                var saveSectors = await _unityOfWork.LapFastSectorDao.UpsertLapFastSectors(sectors, cancellationToken);
                if (!saveSectors.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureSingleResponse<LapFastLapDto>(saveSectors.Message, saveSectors.Exception);
                await ImportWindowCache.Stage(_unityOfWork, "fast_lap", key, result, cancellationToken);
                await ImportWindowCache.Stage(_unityOfWork, "fast_sectors", key, sectors, cancellationToken);
                var commit = await _unityOfWork.Commit(cancellationToken);
                if (!commit.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureSingleResponse<LapFastLapDto>(commit.Message, commit.Exception);
                await transaction.CommitAsync(cancellationToken);
                return LapWindowResult(result);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex) { return ResponseFactory.CreateInstance().CreateFailureSingleResponse<LapFastLapDto>(ex); }
        }

        private static SingleResponse<LapFastLapDto> LapWindowResult(List<LapFastLapDto> laps)
            => laps.Count == 0 ? ResponseFactory.CreateInstance().CreateFailureSingleResponse<LapFastLapDto>("No laps found for the given session key.") : ResponseFactory.CreateInstance().CreateSuccessSingleResponse(laps[0]);
        public async Task<SingleResponse<LapFastLapDto>> GetFastLapOfRaceBySessionKeyDb(int sessionKey)
        {
            try
            {
                SingleResponse<Lap> response = await _unityOfWork.LapDao.GetFastLapSessionBySessionKey(sessionKey);
                if (!response.HasSuccess && response.Exception != null)
                    return ResponseFactory.CreateInstance().CreateFailureSingleResponse<LapFastLapDto>(response.Message, response.Exception);

                if (response.Item == null)
                    return ResponseFactory.CreateInstance().CreateSuccessSingleResponse<LapFastLapDto>("No fast lap found for the given session key in the database.");

                return ResponseFactory.CreateInstance().CreateSuccessSingleResponse(_mapper.Map<LapFastLapDto>(response.Item));
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureSingleResponse<LapFastLapDto>(ex);
            }
        }

        public async Task<Response> SaveLap(Lap lap)
        {
            try
            {
                var response = await _unityOfWork.LapDao.SaveLap(lap);
                if (!response.HasSuccess && response.Exception != null)
                    return ResponseFactory.CreateInstance().CreateFailureResponse(response.Message, response.Exception);

                return await _unityOfWork.Commit();
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }
    }
}
