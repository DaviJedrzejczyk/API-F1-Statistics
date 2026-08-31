using Dao.Interface;
using Entities;
using ExternalApi.Interfaces;
using Services.Interfaces;
using Shared.Responses;

namespace Services.Impl
{
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
        {
            try
            {
                DataResponse<Pit> dataResponse = await _pitClient.GetAllPitsSession(sessionKey);
                return dataResponse;
            }
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
