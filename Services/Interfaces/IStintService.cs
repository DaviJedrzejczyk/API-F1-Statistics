using Entities.Class;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Services.Interfaces
{
    public interface IStintService
    {
        Task<DataResponse<Stint>> GetStintsBySessionKeyApi(int sessionKey);
        Task<DataResponse<Stint>> GetStintsBySessionKeyDb(int sessionKey);
        Task<DataResponse<Stint>> GetStintsBySessionKey(int sessionKey);
        Task<Response> SaveStints(List<Stint> stints);
    }
}
