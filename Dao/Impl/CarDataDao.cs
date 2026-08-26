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

        public async Task<Response> SaveCarDatas(List<CarData> data)
        {
            try
            {
                await _db.AddRangeAsync(data);
                return ResponseFactory.CreateInstance().CreateSuccessResponse($"The car data has been saved.");
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }
    }
}
