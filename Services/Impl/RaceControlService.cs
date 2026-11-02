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
    public class RaceControlService : IRaceControlService
    {
        private readonly IRaceControlClient _race;
        private readonly IUnityOfWork _unitOfWork;

        public RaceControlService(IRaceControlClient race, IUnityOfWork unitOfWork)
        {
            _race = race;
            _unitOfWork = unitOfWork;
        }

        public async Task<DataResponse<RaceControlFilterDto>> GetRaceControlsBySessionFlags(int sessionKey, string[] flags)
            => await GetRaceControlsBySessionFlags(sessionKey, flags, CancellationToken.None);

        public async Task<DataResponse<RaceControlFilterDto>> GetRaceControlsBySessionFlags(int sessionKey, string[] flags, CancellationToken cancellationToken)
        {
            try
            {
                var responseApi = await GetRaceControlsBySessionFlagsApi(sessionKey, flags, cancellationToken);
                if (!responseApi.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureDataResponse<RaceControlFilterDto>(responseApi.Message, responseApi.Exception);

                return CreateRaceControlListDto(responseApi.Itens.OrderBy(x => x.Date).ToList());
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<RaceControlFilterDto>(ex);
            }
        }

        public async Task<DataResponse<RaceControl>> GetRaceControlsBySessionFlagsApi(int sessionKey, string[] flags)
            => await GetRaceControlsBySessionFlagsApi(sessionKey, flags, CancellationToken.None);

        public async Task<DataResponse<RaceControl>> GetRaceControlsBySessionFlagsApi(int sessionKey, string[] flags, CancellationToken cancellationToken)
        {
            try
            {
                var session = sessionKey.ToString(System.Globalization.CultureInfo.InvariantCulture);
                var orderedFlags = flags.Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
                var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(orderedFlags))));
                var key = session + ":" + hash;
                var cached = await ImportWindowCache.Read<RaceControl>(_unitOfWork, "race_control", key, cancellationToken);
                if (cached != null) return ResponseFactory.CreateInstance().CreateSuccessDataResponse(cached);
                var response = await _race.GetRaceControlsBySessionFlags(sessionKey, orderedFlags, cancellationToken);
                if (!response.HasSuccess || response.Itens == null) return response;
                if (response.Itens.Any(x => x.SessionKey != sessionKey) ||
                    response.Itens.Select(x => (x.Date, x.SessionKey, x.MeetingKey)).Distinct().Count() != response.Itens.Count)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<RaceControl>("Invalid race control window returned by upstream.");
                await using var transaction = await _unitOfWork.BeginImport("race_control", session, cancellationToken);
                cached = await ImportWindowCache.Read<RaceControl>(_unitOfWork, "race_control", key, cancellationToken);
                if (cached != null) return ResponseFactory.CreateInstance().CreateSuccessDataResponse(cached);
                var db = await _unitOfWork.RaceControlDao.GetRaceControlsBySession(sessionKey, cancellationToken);
                if (!db.HasSuccess || db.Itens == null) return db;
                var existing = db.Itens.Select(x => (x.Date, x.SessionKey, x.MeetingKey)).ToHashSet();
                var missing = response.Itens.Where(x => !existing.Contains((x.Date, x.SessionKey, x.MeetingKey))).ToList();
                if (missing.Count > 0)
                {
                    var saved = await _unitOfWork.RaceControlDao.SaveRaceControls(missing, cancellationToken);
                    if (!saved.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureDataResponse<RaceControl>(saved.Message, saved.Exception);
                }
                await ImportWindowCache.Stage(_unitOfWork, "race_control", key, response.Itens, cancellationToken);
                var commit = await _unitOfWork.Commit(cancellationToken);
                if (!commit.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureDataResponse<RaceControl>(commit.Message, commit.Exception);
                await transaction.CommitAsync(cancellationToken);
                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(response.Itens);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<RaceControl>(ex);
            }
        }

        public async Task<DataResponse<RaceControl>> GetRaceControlsBySessionFlagsDb(int sessionKey, string[] flags)
        {
            try
            {
                return await _unitOfWork.RaceControlDao.GetRaceControlsBySessionFlags(sessionKey, flags);
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<RaceControl>(ex);
            }
        }

        public async Task<Response> SaveRaceControls(List<RaceControl> raceControls)
        {
            try
            {
                var response = await _unitOfWork.RaceControlDao.SaveRaceControls(raceControls);
                if (!response.HasSuccess)
                    return response;

                return await _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }

        private DataResponse<RaceControlFilterDto> CreateRaceControlListDto(List<RaceControl> raceControls)
        {
            try
            {
                List<RaceControlFilterDto> raceControlDtos = [];
                for (int i = 0; i < raceControls.Count; i++)
                {
                    var raceControl = raceControls[i];

                    if (raceControl.Flag != "YELLOW" && raceControl.Flag != "DOUBLE YELLOW")
                        continue;

                    var nextRaceControl = raceControls.Skip(i + 1)
                                            .FirstOrDefault(rc => rc.Sector == raceControl.Sector && (rc.Flag != "YELLOW" && rc.Flag != "DOUBLE YELLOW"));
                    
                    if (nextRaceControl != null)
                    {
                        raceControlDtos.Add(new RaceControlFilterDto
                        {
                            Sector = raceControl.Sector ?? nextRaceControl.Sector,
                            Flag = raceControl.Flag,
                            DateStart = raceControl.Date,
                            DateEnd = nextRaceControl.Date
                        });
                    }
                }

                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(raceControlDtos);
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<RaceControlFilterDto>(ex);
            }
        }
    }
}
