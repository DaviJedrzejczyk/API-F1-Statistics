using Dao.Interface;
using Shared.Responses;


namespace Dao.Impl
{
    public class UnityOfWork : IUnityOfWork
    {
        private readonly ApiF1DB _db;
        private ISessionDao? session = null;
        public UnityOfWork(ApiF1DB db, ISessionDao sessionDao)
        {
            _db = db;
            session = sessionDao;
        }

        public async Task<Response> Commit()
        {
            try
            {
                await _db.SaveChangesAsync();
                return ResponseFactory.CreateInstance().CreateSuccessResponse();
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }
        public ISessionDao SessionDao
        {
            get
            {
                session ??= new SessionDao(_db);
                return session;
            }
        }

        public void Dispose()
        {
            _db?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
