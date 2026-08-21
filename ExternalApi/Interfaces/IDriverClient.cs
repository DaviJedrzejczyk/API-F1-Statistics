using Shared.Responses;

namespace ExternalApi.Interfaces
{
    public interface IDriverClient
    {
        Task<DataResponse<String>> GetAllDriversRecentMeeting();
    }
}
