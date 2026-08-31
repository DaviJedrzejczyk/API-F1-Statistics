using Entities;
using Shared.Responses;

namespace Services.Interfaces
{
    public interface IOvertakeService
    {
        Task<Response> SaveOvertakes(List<Overtake> overtakes);
        Task<DataResponse<Overtake>> GetOvertakesSessionApi(int sessionKey);
        Task<DataResponse<Overtake>> GetOvertakesSessionDb(int sessionKey);
    }
}
