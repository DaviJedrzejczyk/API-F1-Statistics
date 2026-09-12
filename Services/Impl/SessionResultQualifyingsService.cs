using Dao.Interface;
using Entities.Class;
using Entities.Dtos;
using Services.Interfaces;
using Shared.Responses;
using Shared.Common.Atrributes;

namespace Services.Impl
{
    [IncludeDependencyInjection]
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
                        string[] gaps = x.GapToLeader.Split(';', StringSplitOptions.RemoveEmptyEntries);
                        double gap;

                        if (x.Duration?[2] > 0)
                        {
                            phase = 2;
                            gap = double.TryParse(gaps[2], out double parsedGap) ? parsedGap : 0;
                        }
                        else if (x.Duration?[1] > 0)
                        {
                            phase = 1;
                            gap = double.TryParse(gaps[1], out double parsedGap) ? parsedGap : 0;
                        }
                        else
                        {
                            phase = 0;
                            gap = double.TryParse(gaps[0], out double parsedGap) ? parsedGap : 0;
                        }

                        return new SessionResultQualify
                        {
                            SessionKey = x.SessionKey,
                            MeetingKey = x.MeetingKey,
                            DriverNumber = x.DriverNumber,
                            QualifyingPhase = $"Q{phase + 1}",
                            Duration = x.Duration![phase],
                            GapToLeader = gap
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
