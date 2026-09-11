using Dao.Interface;
using Entities.Class;
using Microsoft.EntityFrameworkCore;
using Shared.Responses;

namespace Dao.Impl
{
    public class StintDao : IStintDao
    {
        private readonly ApiF1DB _apiF1;
        public StintDao(ApiF1DB apiF1DB)
        {
            _apiF1 = apiF1DB;
        }

        public async Task<DataResponse<Stint>> GetStintsBySessionKey(int sessionKey)
        {
            try
            {
                List<Stint> stints = await _apiF1.Stints.Where(s => s.SessionKey == sessionKey).AsNoTracking().ToListAsync();
                return ResponseFactory.CreateInstance().CreateSuccessDataResponse<Stint>(stints);
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureDataResponse<Stint>(ex);
            }
        }

        public async Task<Response> SaveStints(List<Stint> stints)
        {
            try
            {
                await _apiF1.Stints.AddRangeAsync(stints);
                return ResponseFactory.CreateInstance().CreateSuccessResponse("Stints saved successfully.");
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }
    }
}
