using Dao.Interface;
using Entities.Class;
using Microsoft.EntityFrameworkCore;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Dao.Impl
{
    [IncludeDependencyInjection]
    public class RaceControlDao : IRaceControlDao
    {
        private readonly ApiF1DB _db;
        public RaceControlDao(ApiF1DB db)
        {
            _db = db;
        }

        public async Task<DataResponse<RaceControl>> GetRaceControlsBySessionFlags(int sessionKey, string[] sessionFlags)
        {
            try
            {
                var raceControls = await _db.RaceControls.Where(rc => rc.SessionKey == sessionKey && sessionFlags.Contains(rc.Flag)).AsNoTracking().ToListAsync();
                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(raceControls);
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<RaceControl>("Error retrieving race controls by session flags.", ex);
            }
        }

        public async Task<Response> SaveRaceControls(List<RaceControl> raceControls)
            => await SaveRaceControls(raceControls, CancellationToken.None);

        public async Task<DataResponse<RaceControl>> GetRaceControlsBySession(int sessionKey, CancellationToken cancellationToken)
        {
            try
            {
                var items = await _db.RaceControls.Where(x => x.SessionKey == sessionKey).AsNoTracking().ToListAsync(cancellationToken);
                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(items);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex) { return ResponseFactory.CreateInstance().CreateFailureDataResponse<RaceControl>(ex); }
        }

        public async Task<Response> SaveRaceControls(List<RaceControl> raceControls, CancellationToken cancellationToken)
        {
            try
            {
                await _db.RaceControls.AddRangeAsync(raceControls, cancellationToken);
                return ResponseFactory.CreateInstance().CreateSuccessResponse("Race controls saved successfully");
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse("Error saving race controls", ex);
            }
        }
    }
}
