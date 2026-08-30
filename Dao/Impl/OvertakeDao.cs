using Dao.Interface;

namespace Dao.Impl
{
    public class OvertakeDao : IOvertakeDao
    {
        private readonly ApiF1DB _db;
        public OvertakeDao(ApiF1DB apiF1DB)
        {
            _db = apiF1DB;
        }
    }
}
