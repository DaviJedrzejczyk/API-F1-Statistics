using Dao.Interface;

namespace Dao.Impl
{
    public class PitDao : IPitDao
    {
        private ApiF1DB _db;

        public PitDao(ApiF1DB apiF1DB)
        {
            _db = apiF1DB;
        }
    }
}
