using Entities;
using Services.Interfaces;
using Shared.Responses;

namespace Services.Impl
{
    public class DriverService : IDriverService
    {
        public Task<Response> DeleteDriver(int id)
        {
            throw new NotImplementedException();
        }

        public Task<SingleResponse<Driver>> GetDriverById(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Response> InsertDriver(Driver driver)
        {
            throw new NotImplementedException();
        }

        public Task<Response> InsertDriver(List<Driver> drivers)
        {
            throw new NotImplementedException();
        }

        public Task<Response> UpdateDriver(Driver driver)
        {
            throw new NotImplementedException();
        }
    }
}
