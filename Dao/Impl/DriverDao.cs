using Dao.Interface;
using Entities;
using Shared.Responses;

namespace Dao.Impl
{
    public class DriverDao : IDriverDao
    {
        private readonly ApiF1DB _db;
        public DriverDao(ApiF1DB db)
        {
            _db = db;
        }

        public Task<Response> InsertDriver(Driver driver)
        {
            throw new NotImplementedException();
        }

        public Task<Response> InsertDriver(List<Driver> drivers)
        {
            throw new NotImplementedException();
        }

        public Task<Response> DeleteDriver(int id)
        {
            throw new NotImplementedException();
        }

        public Task<SingleResponse<Driver>> GetDriverById(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Response> UpdateDriver(Driver driver)
        {
            throw new NotImplementedException();
        }
    }
}
