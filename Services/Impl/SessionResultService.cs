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

        public async Task<DataResponse<SessionResult>> GetSessionResultBySessionKeyApi(int sessionKey)
            => await GetSessionResultBySessionKeyApi(sessionKey, CancellationToken.None);

        public async Task<DataResponse<SessionResult>> GetSessionResultBySessionKeyApi(int sessionKey, CancellationToken cancellationToken)
        {
            try
            {
                var key = sessionKey.ToString(System.Globalization.CultureInfo.InvariantCulture);
                var cached = await ImportWindowCache.Read<SessionResult>(_unityOfWork, "session_results", key, cancellationToken);
                if (cached != null) return WindowResult(cached);
                var response = await _sessionResultClient.GetSessionResultApi(sessionKey, cancellationToken);
                if (!response.HasSuccess || response.Itens == null)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResult>(response.Message, response.Exception);
                if (response.Itens.Any(x => x.SessionKey != sessionKey) || response.Itens.Select(x => x.DriverNumber).Distinct().Count() != response.Itens.Count)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResult>("Invalid session result window returned by upstream.");

                await using var transaction = await _unityOfWork.BeginImport("session_results", key, cancellationToken);
                cached = await ImportWindowCache.Read<SessionResult>(_unityOfWork, "session_results", key, cancellationToken);
                if (cached != null) return WindowResult(cached);
                var source = response.Itens;
                var qualifying = source.Any(x => x.Duration != null && x.Duration.Count > 1);
                var results = new List<SessionResult>();
                var qualis = new List<SessionResultQualify>();
                if (qualifying)
                {
                    var stints = await _stintService.GetStintsBySessionKey(sessionKey, cancellationToken);
                    if (!stints.HasSuccess || stints.Itens == null)
                        return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResult>(stints.Message, stints.Exception);
                    foreach (var item in source)
                    {
                        var durations = item.Duration ?? new List<double>();
                        var phase = Enumerable.Range(0, Math.Min(3, durations.Count)).Where(i => durations[i] > 0).DefaultIfEmpty(0).Last();
                        var duration = phase < durations.Count ? durations[phase] : 0;
                        var gaps = (item.GapToLeader ?? string.Empty).Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        var gap = phase < gaps.Length && double.TryParse(gaps[phase], out var parsed) ? parsed : 0;
                        qualis.Add(new SessionResultQualify { SessionKey = sessionKey, MeetingKey = item.MeetingKey, DriverNumber = item.DriverNumber, QualifyingPhase = $"Q{phase + 1}", Duration = duration, GapToLeader = gap });
                        results.Add(new SessionResult { SessionKey = sessionKey, MeetingKey = item.MeetingKey, DriverNumber = item.DriverNumber, Dnf = item.Dnf, Dns = item.Dns, Dsq = item.Dsq, NumberOfLaps = item.NumberOfLaps, Position = item.Position, Duration = duration, IsQualy = true, Compound = stints.Itens.FirstOrDefault(x => x.DriverNumber == item.DriverNumber && x.LapEnd == item.NumberOfLaps)?.Compound ?? string.Empty });
                    }
                    var leader = results.Where(x => x.Duration > 0).Select(x => x.Duration).DefaultIfEmpty(0).Min();
                    foreach (var item in results) item.GapToLeader = Math.Round(leader - item.Duration, 3).ToString();
                    var existingQualis = await _unityOfWork.SessionResultQualifyDao.GetQualifyingResultBySessionKey(sessionKey, cancellationToken);
                    if (!existingQualis.HasSuccess || existingQualis.Itens == null)
                        return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResult>(existingQualis.Message, existingQualis.Exception);
                    var keys = existingQualis.Itens.Select(x => (x.MeetingKey, x.SessionKey, x.DriverNumber, x.QualifyingPhase)).ToHashSet();
                    var missing = qualis.Where(x => !keys.Contains((x.MeetingKey, x.SessionKey, x.DriverNumber, x.QualifyingPhase))).ToList();
                    if (missing.Count > 0)
                    {
                        var saved = await _unityOfWork.SessionResultQualifyDao.SaveQualifyResult(missing, cancellationToken);
                        if (!saved.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResult>(saved.Message, saved.Exception);
                    }
                }
                else
                {
                    var mapped = await CreateListSessionResult(source);
                    if (!mapped.HasSuccess || mapped.Itens == null) return mapped;
                    results = mapped.Itens;
                }
                var existing = await _unityOfWork.SessionResultDao.GetSessionResultsBySesssionKey(sessionKey, cancellationToken);
                if (!existing.HasSuccess || existing.Itens == null)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResult>(existing.Message, existing.Exception);
                var drivers = existing.Itens.Select(x => x.DriverNumber).ToHashSet();
                var newResults = results.Where(x => !drivers.Contains(x.DriverNumber)).ToList();
                if (newResults.Count > 0)
                {
                    var saved = await _unityOfWork.SessionResultDao.SaveSessionResults(newResults, cancellationToken);
                    if (!saved.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResult>(saved.Message, saved.Exception);
                }
                results = qualifying
                    ? results.OrderBy(x => x.Position == 0 ? int.MaxValue : x.Position).ToList()
                    : results.Where(x => !(x.Dnf || x.Dsq || x.Dns)).OrderBy(x => x.Position == 0 ? int.MaxValue : x.Position)
                        .Concat(results.Where(x => x.Dnf || x.Dsq || x.Dns).OrderByDescending(x => x.NumberOfLaps)).ToList();
                await ImportWindowCache.Stage(_unityOfWork, "session_results", key, results, cancellationToken);
                var commit = await _unityOfWork.Commit(cancellationToken);
                if (!commit.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResult>(commit.Message, commit.Exception);
                await transaction.CommitAsync(cancellationToken);
                return WindowResult(results);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex) { return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResult>(ex); }
        }

        private static DataResponse<SessionResult> WindowResult(List<SessionResult> results)
            => results.Count == 0 ? ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResult>("Not found the results of this session.") : ResponseFactory.CreateInstance().CreateSuccessDataResponse(results);
        public async Task<DataResponse<SessionResult>> GetSessionResultBySessionKeyDatabase(int sessionKey)
        {
            try
            {
                var response = await _unityOfWork.SessionResultDao.GetSessionResultsBySesssionKey(sessionKey);
                if (!response.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResult>("An error has ocurred to found the results of this session.");

                if (response.Itens == null || response.Itens.Count == 0)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResult>("Not found the results of this session.");
                
                if (response.Itens[0].IsQualy)
                {
                    response.Itens = response.Itens.OrderBy(r => r.Position == 0 ? Int16.MaxValue : r.Position).ToList();
                    return _mapper.Map<DataResponse<SessionResult>>(response);
                }

                var nonRetired = response.Itens.Where(r => !(r.Dnf || r.Dsq || r.Dns)).OrderBy(r => r.Position == 0 ? Int16.MaxValue : r.Position);

                var retired = response.Itens.Where(r => r.Dnf || r.Dsq || r.Dns).OrderByDescending(r => r.NumberOfLaps);

                response.Itens = nonRetired.Concat(retired).ToList();

                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(response.Itens);
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

        private async Task<DataResponse<SessionResult>> CreateListSessionResult(List<SessionResultDto> itens)
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
                    GapToLeader = x.GapToLeader != null && x.GapToLeader.Length > 0 ? x.GapToLeader.ToString() : "0",
                    NumberOfLaps = x.NumberOfLaps,
                    Points = x.Points,
                    MeetingKey = x.MeetingKey,
                    Position = x.Position,
                    SessionKey = x.SessionKey,
                    IsQualy = false
                }).ToList());
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResult>(ex);
            }
        }

    }
}
