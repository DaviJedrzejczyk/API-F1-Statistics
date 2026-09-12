using Entities.Class;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Services.Interfaces
{
    [IncludeDependencyInjection]
    public interface IOvertakeService
    {
        Task<Response> SaveOvertakes(List<Overtake> overtakes);
        Task<DataResponse<Overtake>> GetOvertakesSessionApi(int sessionKey);
        Task<DataResponse<Overtake>> GetOvertakesSessionDb(int sessionKey);
    }
}
