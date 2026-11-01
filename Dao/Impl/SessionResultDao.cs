using Dao.Interface;
using Entities.Class;
using Microsoft.EntityFrameworkCore;
using Shared.Responses;
using Shared.Common.Atrributes;

namespace Dao.Impl
{
    [IncludeDependencyInjection]
    public class SessionResultDao : ISessionResultDao
    {
        private readonly ApiF1DB _db;
        public SessionResultDao(ApiF1DB apiF1DB)
        {
            _db = apiF1DB;
        }

        public async Task<DataResponse<SessionResult>> GetSessionResultsBySesssionKey(int sessionKey)
            => await GetSessionResultsBySesssionKey(sessionKey, CancellationToken.None);

        public async Task<DataResponse<SessionResult>> GetSessionResultsBySesssionKey(int sessionKey, CancellationToken cancellationToken)
        {
            try
            {
                var list = await _db.SessionResults.Where(x => x.SessionKey == sessionKey).AsNoTracking().ToListAsync(cancellationToken);
                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(list);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResult>(ex);
            }
        }

        public async Task<Response> SaveSessionResults(List<SessionResult> sessionResults)
            => await SaveSessionResults(sessionResults, CancellationToken.None);

        public async Task<Response> SaveSessionResults(List<SessionResult> sessionResults, CancellationToken cancellationToken)
        {
            try
            {
                await _db.AddRangeAsync(sessionResults, cancellationToken);
                return ResponseFactory.CreateInstance().CreateSuccessResponse("The session results has been saved.");
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }
    }
}
