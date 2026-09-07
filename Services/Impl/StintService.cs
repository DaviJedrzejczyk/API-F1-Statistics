using Dao.Interface;
using Entities;
using ExternalApi.Interfaces;
using Services.Interfaces;
using Shared.Responses;

namespace Services.Impl
{
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
        {
            try
            {
                var responseDb = await GetStintsBySessionKeyDb(sessionKey);
                if (!responseDb.HasSuccess || (responseDb.Itens != null && responseDb.Itens.Count > 0))
                    return responseDb;

                var responseApi = await GetStintsBySessionKeyApi(sessionKey);
                if (!responseApi.HasSuccess || (responseApi.Itens == null || responseApi.Itens.Count == 0))
                    return responseApi;

                var responseSave = await SaveStints(responseApi.Itens);
                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(responseApi.Itens, "Stints retrieved from API and saved to database successfully.");
            }
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
