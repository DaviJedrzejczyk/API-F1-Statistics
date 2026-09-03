using Dao.Interface;

namespace Dao.Impl
{
    public class RaceControlDao : IRaceControlDao
    {
        private readonly ApiF1DB _db;
        public RaceControlDao(ApiF1DB db)
        {
            _db = db;
        }
    }
}
