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
        {
            try
            {
                var responseDb = await GetFastLapOfRaceBySessionKeyDb(sessionKey);
                if (responseDb.HasSuccess && responseDb.Item != null)
                    return responseDb;

                var responseDriver = await _driverService.GetAllDriversSession(sessionKey);

                if (!responseDriver.HasSuccess)
                    return ResponseFactory.CreateInstance().CreateFailureSingleResponse<LapFastLapDto>("Error to fetch the drivers: " + responseDriver.Message, responseDriver.Exception);

                var responseLapFastSector = await _lapFastSectorService.GetFastSectorsOfSession(sessionKey);
                if (!responseLapFastSector.HasSuccess && responseLapFastSector.Exception != null)
                    return ResponseFactory.CreateInstance().CreateFailureSingleResponse<LapFastLapDto>(responseLapFastSector.Message, responseLapFastSector.Exception);

                LapFastLapDto fastLap = new();
                LapListDto fastLapToSave = new();
                List<LapListFastSector> listFastSectorAllDrivers = [];
                List<LapFastSector> listFastSector = responseLapFastSector.Itens ?? [];
                bool isFastSectorsAlreadySaved = listFastSector.Count > 0;

                for (int i = 0; i < responseDriver.Itens.Count; i++)
                {
                    var responseLap = await _lapClient.GetAllLapsSessionByDriver(sessionKey, responseDriver.Itens[i].DriverNumber);
                    if (!responseLap.HasSuccess && responseLap.Exception != null)
                        return ResponseFactory.CreateInstance().CreateFailureSingleResponse<LapFastLapDto>(responseLap.Message, responseLap.Exception);

                    if (responseLap.Itens == null || responseLap.Itens.Count == 0)
                        continue;

                    if (!isFastSectorsAlreadySaved)
                    {
                        var fastSectors = new LapListFastSector
                        {
                            DriverNumber = responseLap.Itens.First().DriverNumber,

                            Sector1Duration = responseLap.Itens
                                .Where(l => l.DurationSector1 > 0)
                                .Min(l => (double?)l.DurationSector1) ?? 0,

                            Sector2Duration = responseLap.Itens
                                .Where(l => l.DurationSector2 > 0)
                                .Min(l => (double?)l.DurationSector2) ?? 0,

                            Sector3Duration = responseLap.Itens
                                .Where(l => l.DurationSector3 > 0)
                                .Min(l => (double?)l.DurationSector3) ?? 0
                        };

                        listFastSectorAllDrivers.Add(fastSectors);
                    }

                    LapListDto? fastLapDriver = responseLap.Itens.MinBy(l => l.LapDuration);
                    if (fastLapDriver == null) continue;

                    if (fastLap.LapDuration > 0 && fastLapDriver.LapDuration >= fastLap.LapDuration || fastLapDriver.LapDuration == 0)
                        continue;

                    fastLapToSave = fastLapDriver;
                    fastLapToSave.IsFastLap = true;

                    fastLap = new LapFastLapDto
                    {
                        DriverNumber = responseDriver.Itens[i].DriverNumber,
                        LapDuration = fastLapDriver.LapDuration,
                        IsFastLap = true,
                        MeetingKey = fastLapToSave.MeetingKey,
                        SessionKey = sessionKey
                    };
                }

                if (!isFastSectorsAlreadySaved && listFastSectorAllDrivers.Count > 0)
                {
                    var responseSaveFastSectors = await _lapFastSectorService.CreateListWithFastSector(listFastSectorAllDrivers, sessionKey, listFastSector);
                    if (!responseSaveFastSectors.HasSuccess)
                        return ResponseFactory.CreateInstance().CreateFailureSingleResponse<LapFastLapDto>(responseSaveFastSectors.Message, responseSaveFastSectors.Exception);
                }

                if (fastLap.LapDuration == 0)
                    return ResponseFactory.CreateInstance().CreateFailureSingleResponse<LapFastLapDto>("No laps found for the given session key.");

                var responseSaveLap = await SaveLap(_mapper.Map<Lap>(fastLapToSave));
                if (!responseSaveLap.HasSuccess && responseSaveLap.Exception != null)
                    return ResponseFactory.CreateInstance().CreateFailureSingleResponse<LapFastLapDto>(responseSaveLap.Message, responseSaveLap.Exception);

                return ResponseFactory.CreateInstance().CreateSuccessSingleResponse(fastLap);
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureSingleResponse<LapFastLapDto>(ex);
            }
        }

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
