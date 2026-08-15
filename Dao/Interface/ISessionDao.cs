using Entities;
using Shared.Responses;

namespace Dao.Interface
{
    public interface ISessionDao
    {
       Task<Response> InsertTracksOfCurrentYear(List<Session> sessions);
    }
}
