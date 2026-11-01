using Dao.Interface;
using Entities.Class;
using Microsoft.EntityFrameworkCore;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Dao.Impl
{
    [IncludeDependencyInjection]
    public class DriverDao : IDriverDao
    {
        private readonly ApiF1DB _db;
        public DriverDao(ApiF1DB db)
        {
            _db = db;
        }

        public async Task<Response> InsertDrivers(List<Driver> drivers)
            => await InsertDrivers(drivers, CancellationToken.None);

        public async Task<Response> InsertDrivers(List<Driver> drivers, CancellationToken cancellationToken)
        {
            try
            {
                await _db.Drivers.AddRangeAsync(drivers, cancellationToken);
                return ResponseFactory.CreateInstance().CreateSuccessResponse("Drivers inserted successfully.");
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }

        public async Task<Response> DeleteDriver(Driver driver)
        {
            try
            {
                _db.Drivers.Remove(driver);
                return ResponseFactory.CreateInstance().CreateSuccessResponse("Driver has been removed.");
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }

        public async Task<SingleResponse<Driver>> GetDriverById(int id)
        {
            try
            {
                Driver? response = await _db.Drivers.FindAsync(id);
                return ResponseFactory.CreateInstance().CreateSuccessSingleResponse<Driver>(response);
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureSingleResponse<Driver>(ex);
            }
        }
        public async Task<DataResponse<Driver>> GetAllDriversSession(int sessionKey)
            => await GetAllDriversSession(sessionKey, CancellationToken.None);

        public async Task<DataResponse<Driver>> GetAllDriversSession(int sessionKey, CancellationToken cancellationToken)
        {
            try
            {
                List<Driver> drivers = await _db.Drivers.Where(x => x.SessionKey == sessionKey).AsNoTracking().ToListAsync(cancellationToken);
                return ResponseFactory.CreateInstance().CreateSuccessDataResponse<Driver>(drivers);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<Driver>(ex);
            }
        }

        public async Task<SingleResponse<string>> GetDriverName(int driverNumber, int sessionKey)
        {
            try
            {
                string? driverName = await _db.Drivers.Where(x => x.DriverNumber == driverNumber && x.SessionKey == sessionKey).AsNoTracking().Select(x => x.LastName).FirstOrDefaultAsync();
                return ResponseFactory.CreateInstance().CreateSuccessSingleResponse(driverName ?? string.Empty);
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureSingleResponse<string>(ex);
            }
        }
    }
}
