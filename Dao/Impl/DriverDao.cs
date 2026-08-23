using Dao.Interface;
using Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Responses;
using System.Reflection.Metadata.Ecma335;

namespace Dao.Impl
{
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

        public async Task<Response> UpdateDriver(Driver driver)
        {
            try
            {
                _db.Drivers.Update(driver);
                return ResponseFactory.CreateInstance().CreateSuccessResponse("Driver has been updated.");
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }
    }
}
