using Entities;
using Shared.Responses;

namespace Services.Interfaces
{
    public interface ISessionService
    {
        Task<Response> InsertTracksOfCurrentYear(List<Session> sessions);
    }
}
