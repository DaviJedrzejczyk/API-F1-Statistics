using Dao.Interface;
using Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Responses;

namespace Dao.Impl
{
    public class SessionDao : ISessionDao
    {
        private readonly ApiF1DB _db;
        public SessionDao(ApiF1DB db)
        {
            _db = db;
        }
    }
}
