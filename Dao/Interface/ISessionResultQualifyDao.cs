using Entities.Class;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Dao.Interface
{
    public interface ISessionResultQualifyDao
    {
        Task<Response> SaveQualifyResult(List<SessionResultQualify> sessionResultQualifiess);
        Task<DataResponse<SessionResultQualify>> GetQualifyingResultBySessionKey(int sessionKey);
    }
}
