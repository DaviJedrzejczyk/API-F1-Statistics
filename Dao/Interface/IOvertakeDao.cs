using Entities.Class;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Dao.Interface
{
    [IncludeDependencyInjection]
    public interface IOvertakeDao
    {
        Task<Response> SaveOvertakes(List<Overtake> overtakes);
        Task<DataResponse<Overtake>> GetOvertakesBySession(int sessionKey);
    }
}
