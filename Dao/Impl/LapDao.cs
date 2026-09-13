using Dao.Interface;
using Entities.Class;
using Entities.Dtos;
using Microsoft.EntityFrameworkCore;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Dao.Impl
{
    [IncludeDependencyInjection]
    public class LapDao : ILapDao
    {
        private readonly ApiF1DB _db;

        public LapDao(ApiF1DB db)
        {
            _db = db;
        }

        public async Task<SingleResponse<Lap>> GetFastLapSessionBySessionKey(int sessionKey)
        {
            try
            {
                Lap? lap = await _db.Laps.Where(x => x.SessionKey == sessionKey && x.IsFastLap).AsNoTracking().FirstOrDefaultAsync();
                return ResponseFactory.CreateInstance().CreateSuccessSingleResponse(lap!);
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureSingleResponse<Lap>(ex);
            }
        }

        public async Task<Response> SaveLap(Lap lap)
        {
            try
            {
                await _db.Laps.AddAsync(lap);
                return ResponseFactory.CreateInstance().CreateSuccessResponse();
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }
    }
}
