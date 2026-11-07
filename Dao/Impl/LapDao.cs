using Dao.Interface;
using Entities.Class;
using Entities.Dtos;
using Microsoft.EntityFrameworkCore;
using Shared.Common.Atrributes;
using Shared.Responses;

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

        public async Task<SingleResponse<Lap>> GetFastLapSessionBySessionKey(int sessionKey)
        {
            try
            {
                Lap? lap = await _db.Laps.Where(x => x.SessionKey == sessionKey && x.IsFastLap && x.LapDuration > 0).AsNoTracking().OrderBy(x => x.LapDuration).FirstOrDefaultAsync();
                return ResponseFactory.CreateInstance().CreateSuccessSingleResponse(lap!);
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureSingleResponse<Lap>(ex);
            }
        }

        public async Task<Response> SaveLap(Lap lap)
        {
            try
            {
                await _db.Laps.AddAsync(lap);
                return ResponseFactory.CreateInstance().CreateSuccessResponse();
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }

        public async Task<Response> UpsertLap(Lap lap, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var existing = _db.Laps.Local.FirstOrDefault(x => x.MeetingKey == lap.MeetingKey && x.SessionKey == lap.SessionKey && x.DriverNumber == lap.DriverNumber && x.LapNumber == lap.LapNumber)
                    ?? await _db.Laps.FirstOrDefaultAsync(x => x.MeetingKey == lap.MeetingKey && x.SessionKey == lap.SessionKey && x.DriverNumber == lap.DriverNumber && x.LapNumber == lap.LapNumber, cancellationToken);
                if (existing == null) await _db.Laps.AddAsync(lap, cancellationToken);
                else _db.Entry(existing).CurrentValues.SetValues(lap);
                return ResponseFactory.CreateInstance().CreateSuccessResponse();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex) { return ResponseFactory.CreateInstance().CreateFailureResponse(ex); }
        }

        public async Task<DataResponse<Lap>> GetSessionLaps(int sessionKey, int[] driverNumbers, CancellationToken cancellationToken)
        {
            try
            {
                List<Lap> laps = await _db.Laps.AsNoTracking()
                    .Where(lap => lap.SessionKey == sessionKey && driverNumbers.Contains(lap.DriverNumber))
                    .OrderBy(lap => lap.DriverNumber).ThenBy(lap => lap.LapNumber)
                    .ToListAsync(cancellationToken);

                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(laps);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<Lap>(exception);
            }
        }
    }
}
