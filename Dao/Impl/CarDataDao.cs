using Dao.Interface;
using Entities;
using Shared.Responses;

namespace Dao.Impl
{   
    public class CarDataDao : ICarDataDao
    {
        private readonly ApiF1DB _db;
        public CarDataDao(ApiF1DB db)
        {
            _db = db;
        }

        public async Task<Response> InsertHighSpeedSessionDriver(CarData carData)
        {
            try
            {
                await _db.AddAsync(carData);
                return ResponseFactory.CreateInstance().CreateSuccessResponse($"The highest speed for the driver number {carData.DriveNumber}, has been inserted succesfully.");
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }
    }
}
