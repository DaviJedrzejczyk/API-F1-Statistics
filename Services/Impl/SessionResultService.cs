using Dao.Interface;
using Entities;
using ExternalApi.Interfaces;
using Services.Interfaces;
using Shared.Responses;
using System.Security.Principal;

namespace Services.Impl
{
    public class SessionResultService : ISessionResultService
    {
        private readonly ISessionResultClient _sessionResultClient;
        private readonly IUnityOfWork _unityOfWork;

        public SessionResultService(ISessionResultClient sessionResultClient, IUnityOfWork unityOfWork)
        {
            _sessionResultClient = sessionResultClient;
            _unityOfWork = unityOfWork;
        }

        public async Task<DataResponse<SessionResult>> GetSessionResultBySessionKeyApi(int sessionKey)
        {
			try
            {
                DataResponse<SessionResult> sessionResult = await GetSessionResultBySessionKeyDatabase(sessionKey);

                if (sessionResult.HasSuccess && sessionResult.Itens.Count > 0 || !sessionResult.HasSuccess && sessionResult.Exception != null) return sessionResult; //Todo: Can be better this line.

                DataResponse<SessionResult> response = await _sessionResultClient.GetSessionResultApi(sessionKey);

                if (!response.HasSuccess || response.Itens == null || response.Itens.Count == 0)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResult>(response.Message, response.Exception);

                if (sessionResult.Itens != null && sessionResult.Itens.Count > 0)
                    response.Itens = response.Itens.Where(x => !sessionResult.Itens.Any(r => r.SessionKey  == sessionKey)).ToList();
                
                Response responseInsert = await SaveSessionResults(response.Itens);
                if (!responseInsert.HasSuccess) return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResult>(response.Message, response.Exception);

                return response;
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

    }
}
