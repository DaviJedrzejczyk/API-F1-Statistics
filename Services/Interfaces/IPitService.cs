using Entities;
using Shared.Responses;

namespace Services.Interfaces
{
    public interface IPitService
    {
        Task<Response> SavePits(List<Pit> pits);
        Task<DataResponse<Pit>> GetPitsBySessionKeyApi(int sessionKey);
        Task<DataResponse<Pit>> GetPitsBySessionKeyDb(int sessionKey);
    }
}
