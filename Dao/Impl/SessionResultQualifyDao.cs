using Dao.Interface;
using Entities.Class;
using Microsoft.EntityFrameworkCore;
using Shared.Responses;
using Shared.Common.Atrributes;

namespace Dao.Impl
{
    [IncludeDependencyInjection]
    public class SessionResultQualifyDao : ISessionResultQualifyDao
    {
        private readonly ApiF1DB _db;

        public SessionResultQualifyDao(ApiF1DB db)
        {
            _db = db;
        }

        public async Task<DataResponse<SessionResultQualify>> GetQualifyingResultBySessionKey(int sessionKey)
            => await GetQualifyingResultBySessionKey(sessionKey, CancellationToken.None);

        public async Task<DataResponse<SessionResultQualify>> GetQualifyingResultBySessionKey(int sessionKey, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _db.SessionResultQualifyings
                    .Where(x => x.SessionKey == sessionKey)
                    .OrderBy(x => x.QualifyingPhase)
                    .ThenBy(x => x.GapToLeader)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(result);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResultQualify>(ex);
            }
        }

        public async Task<Response> SaveQualifyResult(List<SessionResultQualify> sessionResultQualifiess)
            => await SaveQualifyResult(sessionResultQualifiess, CancellationToken.None);

        public async Task<Response> SaveQualifyResult(List<SessionResultQualify> sessionResultQualifiess, CancellationToken cancellationToken)
        {
            try
            {
                await _db.SessionResultQualifyings.AddRangeAsync(sessionResultQualifiess, cancellationToken);
                return ResponseFactory.CreateInstance().CreateSuccessResponse("Qualifying results saved successfully.");
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }
    }
}
