using Entities.Class;
using Entities.Dtos;
using Shared.Responses;
using Shared.Common.Atrributes;

namespace Services.Interfaces
{
    [IncludeDependencyInjection]
    public interface ISessionResultQualifyingsService
    {
        Task<Response> SaveQualys(List<SessionResultQualify> sessionResultQualifiess);
        Task<DataResponse<SessionResultQualify>> GetQualyBySessionKey(int sessionKey);
        Task<DataResponse<SessionResultQualify>> CreateListResultQualyfing(List<SessionResultDto> sessionResultQualifiess);
    }
}
