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
                for (int i = 0; i < responseDriverDatabase.Itens.Count; i++)
                {
                    var responseLap = await _lapClient.GetAllLapsSessionByDriver(sessionKey, responseDriverDatabase.Itens[i].DriverNumber);
                    if (!responseLap.HasSuccess  && responseLap.Exception != null)
                        return ResponseFactory.CreateInstance().CreateFailureSingleResponse<LapFastLapDto>(responseLap.Message, responseLap.Exception);

                    if (responseLap.Itens == null || responseLap.Itens.Count == 0)
                        continue;

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
