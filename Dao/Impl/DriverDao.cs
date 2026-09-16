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
        {
            try
            {
                await _db.Drivers.AddRangeAsync(drivers);
                return ResponseFactory.CreateInstance().CreateSuccessResponse("Drivers inserted successfully.");
            }
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
        {
            try
            {
                List<Driver> drivers = await _db.Drivers.Where(x => x.SessionKey == sessionKey).AsNoTracking().ToListAsync();
                return ResponseFactory.CreateInstance().CreateSuccessDataResponse<Driver>(drivers);
            }
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
