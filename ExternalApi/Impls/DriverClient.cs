using ExternalApi.Interfaces;
using Shared.Responses;

namespace ExternalApi.Impls
{
    public class DriverClient : IDriverClient
    {
        public Task<DataResponse<string>> GetAllDriversRecentMeeting()
        {
            throw new NotImplementedException();
        }
    }
}
