using Shared.Responses;

namespace Dao.Interface
{
    public interface IUnityOfWork
    {
        ISessionDao SessionDao { get; }
        IMeetingDao MeetingDao { get; }
        IDriverDao DriverDao { get; }
        ICarDataDao CarDataDao { get; }
        ISessionResultDao SessionResultDao { get; }
        IOvertakeDao OvertakeDao { get; }
        Task<Response> Commit();
    }
}
