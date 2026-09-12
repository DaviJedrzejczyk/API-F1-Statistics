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
        {
            try
            {
                var result = await _db.SessionResultQualifyings
                    .Where(x => x.SessionKey == sessionKey)
                    .OrderBy(x => x.QualifyingPhase)
                    .ThenBy(x => x.GapToLeader)
                    .AsNoTracking()
                    .ToListAsync();

                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(result);
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<SessionResultQualify>(ex);
            }
        }

        public async Task<Response> SaveQualifyResult(List<SessionResultQualify> sessionResultQualifiess)
        {
            try
            {
                await _db.SessionResultQualifyings.AddRangeAsync(sessionResultQualifiess);
                return ResponseFactory.CreateInstance().CreateSuccessResponse("Qualifying results saved successfully.");
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }
    }
}
