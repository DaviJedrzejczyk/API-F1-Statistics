using Dao.Interface;
using Entities.Class;
using ExternalApi.Interfaces;
using Services.Interfaces;
using Shared.Responses;
using Shared.Common.Atrributes;

namespace Services.Impl
{
    [IncludeDependencyInjection]
    public class StintService : IStintService
    {
        private readonly IStintClient _client;
        private readonly IUnityOfWork _unityOfWork;
        public StintService(IStintClient client, IUnityOfWork unityOfWork)
        {
            _client = client;
            _unityOfWork = unityOfWork;
        }

        public async Task<DataResponse<Stint>> GetStintsBySessionKey(int sessionKey)
            => await GetStintsBySessionKey(sessionKey, CancellationToken.None);

        public async Task<DataResponse<Stint>> GetStintsBySessionKey(int sessionKey, CancellationToken cancellationToken)
        {
            try
            {
                var key = sessionKey.ToString(System.Globalization.CultureInfo.InvariantCulture);
                var cached = await ImportWindowCache.Read<Stint>(_unityOfWork, "stints", key, cancellationToken);
                if (cached != null) return ResponseFactory.CreateInstance().CreateSuccessDataResponse(cached);
                var responseApi = await _client.GetAllStintsBySession(sessionKey, cancellationToken);
                if (!responseApi.HasSuccess || responseApi.Itens == null)
                    return responseApi;
                if (responseApi.Itens.Any(x => x.SessionKey != sessionKey) ||
                    responseApi.Itens.Select(x => (x.MeetingKey, x.SessionKey, x.DriverNumber, x.StintNumber)).Distinct().Count() != responseApi.Itens.Count)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<Stint>("Invalid stint window returned by upstream.");

                await using var transaction = await _unityOfWork.BeginImport("stints", key, cancellationToken);
                cached = await ImportWindowCache.Read<Stint>(_unityOfWork, "stints", key, cancellationToken);
                if (cached != null) return ResponseFactory.CreateInstance().CreateSuccessDataResponse(cached);
                var responseDb = await _unityOfWork.StintDao.GetStintsBySessionKey(sessionKey, cancellationToken);
                if (!responseDb.HasSuccess || responseDb.Itens == null) return responseDb;
                var existing = responseDb.Itens.Select(x => (x.MeetingKey, x.SessionKey, x.DriverNumber, x.StintNumber)).ToHashSet();
                var missing = responseApi.Itens.Where(x => !existing.Contains((x.MeetingKey, x.SessionKey, x.DriverNumber, x.StintNumber))).ToList();
                if (missing.Count > 0)
                {
                    var saved = await _unityOfWork.StintDao.SaveStints(missing, cancellationToken);
                    if (!saved.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureDataResponse<Stint>(saved.Message, saved.Exception);
                }
                await ImportWindowCache.Stage(_unityOfWork, "stints", key, responseApi.Itens, cancellationToken);
                var commit = await _unityOfWork.Commit(cancellationToken);
                if (!commit.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureDataResponse<Stint>(commit.Message, commit.Exception);
                await transaction.CommitAsync(cancellationToken);
                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(responseApi.Itens);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<Stint>(ex);
            }
        }

        public async Task<DataResponse<Stint>> GetStintsBySessionKeyApi(int sessionKey)
        {
            try
            {
                return await _client.GetAllStintsBySession(sessionKey);
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<Stint>(ex);
            }
        }

        public async Task<DataResponse<Stint>> GetStintsBySessionKeyDb(int sessionKey)
        {
            try
            {
                return await _unityOfWork.StintDao.GetStintsBySessionKey(sessionKey);
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<Stint>(ex);
            }
        }

        public async Task<Response> SaveStints(List<Stint> stints)
        {
            try
            {
                var response = await _unityOfWork.StintDao.SaveStints(stints);
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
