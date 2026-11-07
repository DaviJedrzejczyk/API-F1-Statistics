using Entities.Class;
using Entities.Dtos;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Services.Interfaces
{
    public interface ILapService
    {
        Task<SingleResponse<LapFastLapDto>> GetFastLapOfRaceBySessionKey(int sessionKey);
        Task<SingleResponse<LapFastLapDto>> GetFastLapOfRaceBySessionKey(int sessionKey, CancellationToken cancellationToken);
        Task<SingleResponse<LapFastLapDto>> GetFastLapOfRaceBySessionKeyDb(int sessionKey);
        Task<Response> SaveLap(Lap lap);
    }
}
