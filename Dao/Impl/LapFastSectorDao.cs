using Dao.Interface;
using Entities.Class;
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

        public Task<DataResponse<LapFastSector>> GetAllLapFastSectorsSession(int sessionKey)
        {
            throw new NotImplementedException();
        }

        public Task<Response> SaveLapFastSector(List<LapFastSector> lapFastSectors)
        {
            throw new NotImplementedException();
        }
    }
}
