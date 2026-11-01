using Dao.Interface;
using Entities.Class;
using Microsoft.EntityFrameworkCore;
using Shared.Common.Atrributes;
using Shared.Responses;

namespace Dao.Impl
{
    [IncludeDependencyInjection]
    public class CarDataDao : ICarDataDao
    {
        private readonly ApiF1DB _db;
        public CarDataDao(ApiF1DB db)
        {
            _db = db;
        }

        public async Task<DataResponse<CarData>> GetHighSpeedSessionDatabase(int sessionKey, int minimunSpeed)
            => await GetHighSpeedSessionDatabase(sessionKey, minimunSpeed, CancellationToken.None);

        public async Task<DataResponse<CarData>> GetHighSpeedSessionDatabase(int sessionKey, int minimunSpeed, CancellationToken cancellationToken)
        {
            try
            {
                var list = await _db.CarDatas.Where(x => x.SessionKey == sessionKey && x.Speed >= minimunSpeed).AsNoTracking().ToListAsync(cancellationToken);
                return ResponseFactory.CreateInstance().CreateSuccessDataResponse(list);

            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<CarData>(ex);
            }
        }

        public async Task<Response> SaveCarDatas(List<CarData> data)
            => await SaveCarDatas(data, CancellationToken.None);

        public async Task<Response> SaveCarDatas(List<CarData> data, CancellationToken cancellationToken)
        {
            try
            {
                await _db.AddRangeAsync(data, cancellationToken);
                return ResponseFactory.CreateInstance().CreateSuccessResponse($"The car data has been saved.");
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }
    }
}
