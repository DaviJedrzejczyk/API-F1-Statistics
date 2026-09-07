using Dao.Interface;
using Entities;
using Entities.Class;
using Entities.Dtos.SessionResultDTOs;
using ExternalApi.Interfaces;
using Services.Interfaces;
using Shared.Responses;
using System.Numerics;

namespace Services.Impl
{
    public class SessionResultService : ISessionResultService
    {
        private readonly ISessionResultClient _sessionResultClient;
        private readonly IUnityOfWork _unityOfWork;
        private readonly ISessionResultQualifyingsService _sessionResultQualifyings;

        public SessionResultService(ISessionResultClient sessionResultClient, IUnityOfWork unityOfWork, ISessionResultQualifyingsService sessionResultQualifyings)
        {
            _sessionResultClient = sessionResultClient;
            _unityOfWork = unityOfWork;
            _sessionResultQualifyings = sessionResultQualifyings;
        }

        public async Task<DataResponse<SessionResult>> GetSessionResultBySessionKeyApi(int sessionKey)
        {
			try
            {
                DataResponse<SessionResult> sessionResult = await GetSessionResultBySessionKeyDatabase(sessionKey);

                if (sessionResult.HasSuccess && sessionResult.Itens.Count > 0 || !sessionResult.HasSuccess && sessionResult.Exception != null) 
                    return sessionResult; 

                DataResponse<SessionResultDto> response = await _sessionResultClient.GetSessionResultApi(sessionKey);
                
                if (!response.HasSuccess || response.Itens == null || response.Itens.Count == 0)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResult>(response.Message, response.Exception);

                if (response.HasSuccess && response.Itens[0].Duration != null && response.Itens[0].Duration.Count > 1)
                {
                    var qualifyingResults = await GetQualyfingResult(response.Itens);
                    return ResponseFactory.CreateInstance().CreateSuccessDataResponse(qualifyingResults.Itens);
                }
                
                return await CreatListSessionResult(response.Itens);
            }
            catch (Exception ex)
			{
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResult>(ex);
			}
        }

        public async Task<DataResponse<SessionResult>> GetSessionResultBySessionKeyDatabase(int sessionKey)
        {
            try
            {
                var response = await _unityOfWork.SessionResultDao.GetSessionResultsBySesssionKey(sessionKey);
                if (!response.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResult>("An error has ocurred to found the results of this session.");

                if (response.Itens == null || response.Itens.Count == 0)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResult>("Not found the results of this session.");

                return response;
            }
            catch (Exception ex)
            { 
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResult>(ex);
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

        private async Task<DataResponse<SessionResult>> CreatListSessionResult(List<SessionResultDto> itens)
        {
            try
            {
                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(itens.Select(x => new SessionResult
                {
                    Dnf = x.Dnf,
                    Dns = x.Dns,
                    Dsq = x.Dsq,
                    DriverNumber = x.DriverNumber,
                    Duration = x.Duration != null && x.Duration.Count > 0 ? x.Duration[0] : 0,
                    GapToLeader = x.GapToLeader != null && x.GapToLeader.Count > 0 ? x.GapToLeader[0] : 0,
                    NumberOfLaps = x.NumberOfLaps,
                    MeetingKey = x.MeetingKey,
                    Position = x.Position,
                    SessionKey = x.SessionKey
                }).ToList());
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResult>(ex);
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
                        GapToLeader = Math.Round(timeLeader - qualis[i].Duration, 3),
                        NumberOfLaps = itens[i].NumberOfLaps,
                        MeetingKey = qualis[i].MeetingKey,
                        Position = itens[i].Position,
                        SessionKey = qualis[i].SessionKey
                    });
                }

                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(sessionResults);
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResult>(ex);
            }
        }

    }
}
