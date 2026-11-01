using Dao.Interface;
using Entities.Class;
using Microsoft.EntityFrameworkCore;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Dao.Impl
{
    [IncludeDependencyInjection]
    public class OvertakeDao : IOvertakeDao
    {
        private readonly ApiF1DB _db;
        public OvertakeDao(ApiF1DB apiF1DB)
        {
            _db = apiF1DB;
        }

        public async Task<DataResponse<Overtake>> GetOvertakesBySession(int sessionKey)
            => await GetOvertakesBySession(sessionKey, CancellationToken.None);

        public async Task<DataResponse<Overtake>> GetOvertakesBySession(int sessionKey, CancellationToken cancellationToken)
        {
            try
            {
                List<Overtake> overtakes = await _db.Overtakes.Where(x => x.SessionKey == sessionKey).AsNoTracking().ToListAsync(cancellationToken);
                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(overtakes);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<Overtake>(ex);
            }
        }

        public async Task<Response> SaveOvertakes(List<Overtake> overtakes)
            => await SaveOvertakes(overtakes, CancellationToken.None);

        public async Task<Response> SaveOvertakes(List<Overtake> overtakes, CancellationToken cancellationToken)
        {
            try
            {
                await _db.AddRangeAsync(overtakes, cancellationToken);
                return ResponseFactory.CreateInstance().CreateSuccessResponse("The overtakes has been inserted.");
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }
    }
}
