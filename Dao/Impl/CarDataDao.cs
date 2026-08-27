using Dao.Interface;
using Entities;
using Microsoft.EntityFrameworkCore;
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

        public async Task<DataResponse<CarData>> GetHighSpeedSessionDatabase(int sessionKey, int minimunSpeed)
        {
            try
            {
                var list = await _db.CarDatas.Where(x => x.SessionKey == sessionKey && x.Speed >= minimunSpeed).AsNoTracking().ToListAsync();
                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(list);

            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<CarData>(ex);
            }
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
