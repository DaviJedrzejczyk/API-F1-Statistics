using Dao.Interface;
using Entities.Class;
using Microsoft.EntityFrameworkCore;
using Shared.Responses;

namespace Dao.Impl
{
    public class LapFastSectorDao : ILapFastSectorDao
    {
        private readonly ApiF1DB _db;

        public LapFastSectorDao(ApiF1DB db)
        {
            _db = db;
        }

        public async Task<DataResponse<LapFastSector>> GetAllLapFastSectorsSession(int sessionKey)
        {
            try
            {
                var lapFastSectors = await _db.LapFastSectors.Where(l => l.SessionKey == sessionKey).AsNoTracking().ToListAsync();
                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(lapFastSectors);
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<LapFastSector>(ex);
            }
        }

        public async Task<Response> SaveLapFastSector(List<LapFastSector> lapFastSectors)
        {
            try
            {
                await _db.LapFastSectors.AddRangeAsync(lapFastSectors);
                return ResponseFactory.CreateInstance().CreateSuccessResponse("LapFastSectors saved successfully.");
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }
    }
}
