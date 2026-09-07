using Entities.Class;
using Entities.Dtos.SessionResultDTOs;
using Shared.Responses;

namespace Services.Interfaces
{
    public interface ISessionResultQualifyingsService
    {
        Task<Response> SaveQualys(List<SessionResultQualify> sessionResultQualifiess);
        Task<DataResponse<SessionResultQualify>> GetQualyBySessionKey(int sessionKey);
        Task<DataResponse<SessionResultQualify>> CreateListResultQualyfing(List<SessionResultDto> sessionResultQualifiess);
    }
}
