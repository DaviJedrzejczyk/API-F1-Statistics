using Dao.Interface;
using Entities.Class;
using Entities.Dtos;
using ExternalApi.Interfaces;
using Services.Interfaces;
using Shared.Responses;
using Shared.Common.Atrributes;
using AutoMapper;

namespace Services.Impl
{
    [IncludeDependencyInjection]
    public class SessionResultService : ISessionResultService
    {
        private readonly ISessionResultClient _sessionResultClient;
        private readonly IUnityOfWork _unityOfWork;
        private readonly ISessionResultQualifyingsService _sessionResultQualifyings;
        private readonly IStintService _stintService;
        private readonly ILapService _lapService;
        private readonly IMapper _mapper;

        public SessionResultService(ISessionResultClient sessionResultClient, IUnityOfWork unityOfWork, ISessionResultQualifyingsService sessionResultQualifyings, IStintService stintService, ILapService lapService, IMapper mapper)
        {
            _sessionResultClient = sessionResultClient;
            _unityOfWork = unityOfWork;
            _sessionResultQualifyings = sessionResultQualifyings;
            _stintService = stintService;
            _lapService = lapService;
            _mapper = mapper;
        }

        public async Task<DataResponse<SessionResultFastLapDto>> GetSessionResultBySessionKeyApi(int sessionKey)
        {
			try
            {
                DataResponse<SessionResultFastLapDto> sessionResult = await GetSessionResultBySessionKeyDatabase(sessionKey);

                if (sessionResult.HasSuccess && sessionResult.Itens.Count > 0 || !sessionResult.HasSuccess && sessionResult.Exception != null)
                    return sessionResult; 

                DataResponse<SessionResultDto> response = await _sessionResultClient.GetSessionResultApi(sessionKey);
                
                if (!response.HasSuccess || response.Itens == null || response.Itens.Count == 0)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResultFastLapDto>(response.Message, response.Exception);

                Response responseSave = new();
                if (response.HasSuccess && response.Itens[0].Duration != null && response.Itens[0].Duration.Count > 1)
                {
                    var qualifyingResults = await GetQualyfingResult(response.Itens);

                    if (!qualifyingResults.HasSuccess) 
                        return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResultFastLapDto>(qualifyingResults.Message, qualifyingResults.Exception);

                    responseSave = await SaveSessionResults(qualifyingResults.Itens);
                    if (!responseSave.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResultFastLapDto>(responseSave.Message, responseSave.Exception);

                    return ResponseFactory.CreateInstance().CreateSuccessDataResponse(_mapper.Map<List<SessionResultFastLapDto>>(qualifyingResults.Itens));
                }
                
                var responseCreatList = await CreateListSessionResult(response.Itens);
                if (!responseCreatList.HasSuccess) return responseCreatList;

                responseSave = await SaveSessionResults(_mapper.Map<List<SessionResult>>(responseCreatList.Itens));
                if (!responseSave.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResultFastLapDto>(responseSave.Message, response.Exception);

                return responseCreatList;
            }
            catch (Exception ex)
			{
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResultFastLapDto>(ex);
			}
        }

        public async Task<DataResponse<SessionResultFastLapDto>> GetSessionResultBySessionKeyDatabase(int sessionKey)
        {
            try
            {
                var response = await _unityOfWork.SessionResultDao.GetSessionResultsBySesssionKey(sessionKey);
                if (!response.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResultFastLapDto>("An error has ocurred to found the results of this session.");

                if (response.Itens == null || response.Itens.Count == 0)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResultFastLapDto>("Not found the results of this session.");
                
                if (response.Itens[0].IsQualy)
                {
                    response.Itens = response.Itens.OrderBy(r => r.Position == 0 ? Int16.MaxValue : r.Position).ToList();
                    return _mapper.Map<DataResponse<SessionResultFastLapDto>>(response);
                }

                var nonRetired = response.Itens.Where(r => !(r.Dnf || r.Dsq || r.Dns)).OrderBy(r => r.Position == 0 ? Int16.MaxValue : r.Position);

                var retired = response.Itens.Where(r => r.Dnf || r.Dsq || r.Dns).OrderByDescending(r => r.NumberOfLaps);

                response.Itens = nonRetired.Concat(retired).ToList();

                var fastLap = await _lapService.GetFastLapOfRaceBySessionKey(sessionKey);
                if (!fastLap.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResultFastLapDto>(fastLap.Message, fastLap.Exception);

                var driverListFastLap = _mapper.Map<List<SessionResultFastLapDto>>(response.Itens);

                var driverFastLap = driverListFastLap.FirstOrDefault(d => d.DriverNumber == fastLap.Item.DriverNumber);
                driverFastLap!.LapFastLap = fastLap.Item;

                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(driverListFastLap);
            }
            catch (Exception ex)
            { 
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResultFastLapDto>(ex);
            }
        }

        public async Task<Response> SaveSessionResults(List<SessionResult> sessionResult)
        {
            try
            {
                var response = await _unityOfWork.SessionResultDao.SaveSessionResults(sessionResult);
                
                if (!response.HasSuccess)
                    return response;

                return await _unityOfWork.Commit();
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }

        private async Task<DataResponse<SessionResultFastLapDto>> CreateListSessionResult(List<SessionResultDto> itens)
        {
            try
            {
                var fastLap = await _lapService.GetFastLapOfRaceBySessionKey(itens[0].SessionKey);
                if (!fastLap.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResultFastLapDto>(fastLap.Message, fastLap.Exception);

                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(itens.Select(x => new SessionResultFastLapDto
                {
                    Dnf = x.Dnf,
                    Dns = x.Dns,
                    Dsq = x.Dsq,
                    DriverNumber = x.DriverNumber,
                    Duration = x.Duration != null && x.Duration.Count > 0 ? x.Duration[0] : 0,
                    GapToLeader = x.GapToLeader != null && x.GapToLeader.Length > 0 ? x.GapToLeader.ToString() : "0",
                    NumberOfLaps = x.NumberOfLaps,
                    Points = x.Points,
                    MeetingKey = x.MeetingKey,
                    Position = x.Position,
                    SessionKey = x.SessionKey,
                    IsQualy = false,
                    LapFastLap = fastLap.Item.DriverNumber == x.DriverNumber ? fastLap.Item : null
                }).ToList());
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResultFastLapDto>(ex);
            }
        }

        private async Task<DataResponse<SessionResult>> GetQualyfingResult(List<SessionResultDto> itens)
        {
            try
            {
                List<SessionResultQualify> qualis = [];
                List<SessionResult> sessionResults = [];

                DataResponse<SessionResultQualify> response = await _sessionResultQualifyings.GetQualyBySessionKey(itens[0].SessionKey);

                if (!response.HasSuccess && response.Exception != null)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResult>(response.Message, response.Exception);

                if (response.HasSuccess && response.Itens.Count > 0)
                    qualis = response.Itens;
                else
                {
                    var qualifyingResults = await _sessionResultQualifyings.CreateListResultQualyfing(itens);

                    if (!qualifyingResults.HasSuccess)
                        return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResult>(qualifyingResults.Message, qualifyingResults.Exception);

                    qualis = qualifyingResults.Itens;
                }

                double timeLeader = qualis.Min(x => x.Duration);
                for (int i = 0; i < qualis.Count; i++)
                {
                    sessionResults.Add(new SessionResult
                    {
                        Dnf = itens[i].Dnf,
                        Dns = itens[i].Dns,
                        Dsq = itens[i].Dsq,
                        DriverNumber = qualis[i].DriverNumber,
                        Duration = qualis[i].Duration,
                        GapToLeader = Math.Round(timeLeader - qualis[i].Duration, 3).ToString(),
                        NumberOfLaps = itens[i].NumberOfLaps,
                        MeetingKey = qualis[i].MeetingKey,
                        Position = itens[i].Position,
                        SessionKey = qualis[i].SessionKey,
                        IsQualy = true
                    });
                }


                var stints = await _stintService.GetStintsBySessionKey(itens[0].SessionKey);
                if (!stints.HasSuccess && stints.Exception != null)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResult>(stints.Message, stints.Exception);
                
                ApplyLastCompoundUsed(sessionResults, stints);

                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(sessionResults);
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResult>(ex);
            }
        }

        private static void ApplyLastCompoundUsed(List<SessionResult> sessionResults, DataResponse<Stint> stints)
        {
            if (stints.HasSuccess && stints.Itens.Count > 0)
            {
                for (int i = 0; i < sessionResults.Count; i++)
                {
                    var stint = stints.Itens.FirstOrDefault(x => x.LapEnd == sessionResults[i].NumberOfLaps && x.DriverNumber == sessionResults[i].DriverNumber);
                    if (stint != null)
                        sessionResults[i].Compound = stint.Compound;
                }
            }
        }
    }
}
