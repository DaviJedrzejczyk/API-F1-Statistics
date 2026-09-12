using Dao.Interface;
using Shared.Common.Atrributes;

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
    }
}
