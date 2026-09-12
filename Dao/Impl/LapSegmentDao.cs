using Dao.Interface;
using Shared.Common.Atrributes;

namespace Dao.Impl
{
    [IncludeDependencyInjection]
    public class LapSegmentDao : ILapSegmentDao
    {
        private readonly ApiF1DB _db;
        public LapSegmentDao(ApiF1DB db)
        {
            _db = db;
        }
    }
}
