using Dao.Interface;
using Entities.Class;
using ExternalApi.Interfaces;
using Services.Interfaces;
using Shared.Responses;
using Shared.Common.Atrributes;

namespace Services.Impl
{
    [IncludeDependencyInjection]
    public class PitService : IPitService
    {
        private readonly IUnityOfWork _unityOfWork;
        private readonly IPitClient _pitClient;

        public PitService(IUnityOfWork unityOfWork, IPitClient pitClient)
        {
            _unityOfWork = unityOfWork;
            _pitClient = pitClient;
        }

        public async Task<DataResponse<Pit>> GetPitsBySessionKeyApi(int sessionKey)
            => await GetPitsBySessionKeyApi(sessionKey, CancellationToken.None);

        public async Task<DataResponse<Pit>> GetPitsBySessionKeyApi(int sessionKey, CancellationToken cancellationToken)
        {
            try
            {
                var key = sessionKey.ToString(System.Globalization.CultureInfo.InvariantCulture);
                var cached = await ImportWindowCache.Read<Pit>(_unityOfWork, "pits", key, cancellationToken);
                if (cached != null) return ResponseFactory.CreateInstance().CreateSuccessDataResponse(cached);
                DataResponse<Pit> dataResponse = await _pitClient.GetAllPitsSession(sessionKey, cancellationToken);
                if (!dataResponse.HasSuccess || dataResponse.Itens == null) return dataResponse;
                if (dataResponse.Itens.Any(x => x.SessionKey != sessionKey) ||
                    dataResponse.Itens.Select(x => (x.Date, x.DriverNumber, x.SessionKey, x.MeetingKey)).Distinct().Count() != dataResponse.Itens.Count)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<Pit>("Invalid pit window returned by upstream.");
                await using var transaction = await _unityOfWork.BeginImport("pits", key, cancellationToken);
                cached = await ImportWindowCache.Read<Pit>(_unityOfWork, "pits", key, cancellationToken);
                if (cached != null) return ResponseFactory.CreateInstance().CreateSuccessDataResponse(cached);
                var responseDb = await _unityOfWork.PitDao.GetAllPitsBySessionKey(sessionKey, cancellationToken);
                if (!responseDb.HasSuccess || responseDb.Itens == null) return responseDb;
                var existing = responseDb.Itens.Select(x => (x.Date, x.DriverNumber, x.SessionKey, x.MeetingKey)).ToHashSet();
                var newPits = dataResponse.Itens
                    .Where(x => existing.Add((x.Date, x.DriverNumber, x.SessionKey, x.MeetingKey)))
                    .ToList();
                if (newPits.Count > 0)
                {
                    var responseInsert = await _unityOfWork.PitDao.SavePits(newPits, cancellationToken);
                    if (!responseInsert.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureDataResponse<Pit>(responseInsert.Message, responseInsert.Exception);
                }
                await ImportWindowCache.Stage(_unityOfWork, "pits", key, dataResponse.Itens, cancellationToken);
                var commit = await _unityOfWork.Commit(cancellationToken);
                if (!commit.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureDataResponse<Pit>(commit.Message, commit.Exception);
                await transaction.CommitAsync(cancellationToken);
                return dataResponse;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
               return ResponseFactory.CreateInstance().CreateFailureDataResponse<Pit>(ex);
            }
        }

        public async Task<DataResponse<Pit>> GetPitsBySessionKeyDb(int sessionKey)
        {
            try
            {
                var response = await _unityOfWork.PitDao.GetAllPitsBySessionKey(sessionKey);
                
                if (response.Itens == null || response.Itens.Count <= 0)
                {
                    response.Message = "Pits not found for this session in database.";
                    return response;
                }

                return response;
            }
            catch (Exception ex)
            {
               return ResponseFactory.CreateInstance().CreateFailureDataResponse<Pit>(ex);
            }
        }

        public async Task<Response> SavePits(List<Pit> pits)
        {
            try
            {
                var response = await _unityOfWork.PitDao.SavePits(pits);
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
