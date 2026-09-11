using Dao.Interface;

namespace Dao.Impl
{
    public class LapDao : ILapDao
    {
        private readonly ApiF1DB _db;

        public LapDao(ApiF1DB db)
        {
            _db = db;
        }
    }
}
