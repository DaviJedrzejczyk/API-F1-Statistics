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

        public LapService(IUnityOfWork unityOfWork, ILapClient lapClient, IDriverService driverService, IMapper mapper)
        {
            _unityOfWork = unityOfWork;
            _lapClient = lapClient;
            _driverService = driverService;
            _mapper = mapper;
        }

        public async Task<SingleResponse<LapFastLapDto>> GetFastLapOfRaceBySessionKey(int sessionKey)
        {
            try
            {
                var responseDb = await GetFastLapOfRaceBySessionKeyDb(sessionKey);
                if (responseDb.HasSuccess && responseDb.Item != null)
                    return responseDb;

                var responseDriverDatabase = await _driverService.SearchDriversDatabase(new DriverInsertDTO { SessionKey = sessionKey });
                
                if (!responseDriverDatabase.HasSuccess && responseDriverDatabase.Exception != null) 
                    return ResponseFactory.CreateInstance().CreateFailureSingleResponse<LapFastLapDto>(responseDriverDatabase.Message, responseDriverDatabase.Exception);

                if (responseDriverDatabase.Itens == null || responseDriverDatabase.Itens.Count == 0)
                {
                    responseDriverDatabase = await _driverService.SearchDriversExternalApi(new DriverInsertDTO { SessionKey = sessionKey });

                    if (!responseDriverDatabase.HasSuccess && responseDriverDatabase.Exception != null)
                        return ResponseFactory.CreateInstance().CreateFailureSingleResponse<LapFastLapDto>(responseDriverDatabase.Message, responseDriverDatabase.Exception);

                    if (responseDriverDatabase.Itens == null || responseDriverDatabase.Itens.Count == 0)
                        return ResponseFactory.CreateInstance().CreateFailureSingleResponse<LapFastLapDto>("No drivers found for the given session key.");
                }

                LapFastLapDto fastLap = new();
                LapListDto fastLapToSave = new();
                List<LapListFastSector> listFastSectorAllDrivers = [];
                List<LapFastSectorDriverDto> listFastSector = [];
                for (int i = 0; i < responseDriverDatabase.Itens.Count; i++)
                {
                    var responseLap = await _lapClient.GetAllLapsSessionByDriver(sessionKey, responseDriverDatabase.Itens[i].DriverNumber);
                    if (!responseLap.HasSuccess  && responseLap.Exception != null)
                        return ResponseFactory.CreateInstance().CreateFailureSingleResponse<LapFastLapDto>(responseLap.Message, responseLap.Exception);

                    if (responseLap.Itens == null || responseLap.Itens.Count == 0)
                        continue;

                    var fastSectors = new LapListFastSector
                    {
                        DriverNumber = responseLap.Itens.First().DriverNumber,

                        Sector1Duration = responseLap.Itens
                        .Where(l => l.DurationSector1 > 0)
                        .Min(l => l.DurationSector1),

                        Sector2Duration = responseLap.Itens
                        .Where(l => l.DurationSector2 > 0)
                        .Min(l => l.DurationSector2),

                        Sector3Duration = responseLap.Itens
                        .Where(l => l.DurationSector3 > 0)
                        .Min(l => l.DurationSector3)
                    };

                    listFastSectorAllDrivers.Add(fastSectors);

                    LapListDto? fastLapDriver = responseLap.Itens.MinBy(l => l.LapDuration);
                    if (fastLapDriver == null) continue;

                    if (fastLap.LapDuration > 0 && fastLapDriver.LapDuration >= fastLap.LapDuration || fastLapDriver.LapDuration == 0)
                        continue;

                    fastLapToSave = fastLapDriver;
                    fastLapToSave.IsFastLap = true;

                    fastLap = new LapFastLapDto
                    {
                        DriverNumber = responseDriverDatabase.Itens[i].DriverNumber,
                        LapDuration = fastLapDriver.LapDuration
                    };
                }

                var responseFastSectors = await CreateListWithFastSector(listFastSectorAllDrivers, sessionKey, listFastSector);
                
                if (!responseFastSectors.HasSuccess)
                    return ResponseFactory.CreateInstance().CreateFailureSingleResponse<LapFastLapDto>(responseFastSectors.Message, responseFastSectors.Exception);
    
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

        private async Task<DataResponse<LapFastSectorDriverDto>> CreateListWithFastSector(List<LapListFastSector> listFastSectorAllDrivers, int sessionKey, List<LapFastSectorDriverDto> listFastSector)
        {
            try
            {
                var bestSector1 = listFastSectorAllDrivers
                    .Where(x => x.Sector1Duration > 0)
                    .MinBy(x => x.Sector1Duration);

                var bestSector2 = listFastSectorAllDrivers
                    .Where(x => x.Sector2Duration > 0)
                    .MinBy(x => x.Sector2Duration);

                var bestSector3 = listFastSectorAllDrivers
                    .Where(x => x.Sector3Duration > 0)
                    .MinBy(x => x.Sector3Duration);

                if (bestSector1 != null)
                {
                    var driverName = await _driverService.GetDriverName(bestSector1.DriverNumber, sessionKey);

                    if (!driverName.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureDataResponse<LapFastSectorDriverDto>(driverName.Message, driverName.Exception);

                    listFastSector.Add(new LapFastSectorDriverDto
                    {
                        DriverNumber = bestSector1.DriverNumber,
                        Duration = bestSector1.Sector1Duration,
                        DriverName = driverName.Item
                    });
                }

                if (bestSector2 != null)
                {
                    var driverName = await _driverService.GetDriverName(bestSector2.DriverNumber, sessionKey);

                    if (!driverName.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureDataResponse<LapFastSectorDriverDto>(driverName.Message, driverName.Exception);

                    listFastSector.Add(new LapFastSectorDriverDto
                    {
                        DriverNumber = bestSector2.DriverNumber,
                        Duration = bestSector2.Sector2Duration,
                        DriverName = driverName.Item
                    });
                }

                if (bestSector3 != null)
                {
                    var driverName = await _driverService.GetDriverName(bestSector3.DriverNumber, sessionKey);

                    if (!driverName.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureDataResponse<LapFastSectorDriverDto>(driverName.Message, driverName.Exception);

                    listFastSector.Add(new LapFastSectorDriverDto
                    {
                        DriverNumber = bestSector3.DriverNumber,
                        Duration = bestSector3.Sector3Duration,
                        DriverName = driverName.Item
                    });
                }

                if (listFastSector.Count < 3)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<LapFastSectorDriverDto>("Not all fast sectors were found.");

                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(listFastSector);
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<LapFastSectorDriverDto>(ex);
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

        public void GetFastSector(List<LapListDto> listDtos, List<LapFastSectorDriverDto> actualFastSec, out List<LapFastSectorDriverDto> fastSectors)
        {
            fastSectors = actualFastSec;
            try
            {
                

            }
            catch (Exception ex)
            {
                //return ResponseFactory.CreateInstance().CreateFailureDataResponse<LapFastSectorDriverDto>(ex);
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
