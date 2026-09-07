using Dao.Interface;
using Entities.Class;
using Entities.Dtos.SessionResultDTOs;
using Services.Interfaces;
using Shared.Responses;

namespace Services.Impl
{
    public class SessionResultQualifyingsService : ISessionResultQualifyingsService
    {
        private readonly IUnityOfWork _unityOfWork;

        public SessionResultQualifyingsService(IUnityOfWork unityOfWork)
        {
            _unityOfWork = unityOfWork;
        }

        public async Task<DataResponse<SessionResultQualify>> CreateListResultQualyfing(List<SessionResultDto> itens)
        {
            try
            {
                var result = itens
                    .Select(x =>
                    {
                        int phase;

                        if (x.Duration?[2] > 0)
                            phase = 2;
                        else if (x.Duration?[1] > 0)
                            phase = 1;
                        else
                            phase = 0;

                        return new SessionResultQualify
                        {
                            SessionKey = x.SessionKey,
                            MeetingKey = x.MeetingKey,
                            DriverNumber = x.DriverNumber,
                            QualifyingPhase = $"Q{phase + 1}",
                            Duration = x.Duration![phase]
                        };
                    }).ToList();
                
                var response = await SaveQualys(result);
                if (!response.HasSuccess)
                    return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResultQualify>(response.Message, response.Exception);

                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(result);
            }
            catch (Exception ex)
            {   
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResultQualify>(ex);
            }
        }

        public async Task<DataResponse<SessionResultQualify>> GetQualyBySessionKey(int sessionKey)
        {
            try
            {
                var response = await _unityOfWork.SessionResultQualifyDao.GetQualifyingResultBySessionKey(sessionKey);
                if (!response.HasSuccess)
                {
                    if (response.Exception != null)
                        return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResultQualify>(response.Message, response.Exception);

                    if (response.Itens == null || response.Itens.Count == 0)
                        return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResultQualify>("No qualifying results found for the specified session key.");
                }
                    
                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(response.Itens);
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResultQualify>(ex);
            }
        }

        public async Task<Response> SaveQualys(List<SessionResultQualify> sessionResultQualifiess)
        {
            try
            {
                var response = await _unityOfWork.SessionResultQualifyDao.SaveQualifyResult(sessionResultQualifiess);
                
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
