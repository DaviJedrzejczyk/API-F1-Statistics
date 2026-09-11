using Dao.Interface;

namespace Dao.Impl
{
    public class LapSegmentDao : ILapSegmentDao
    {
        private readonly ApiF1DB _db;
        public LapSegmentDao(ApiF1DB db)
        {
            _db = db;
        }
    }
}
