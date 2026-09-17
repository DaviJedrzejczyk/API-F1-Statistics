using Shared.Common.Atrributes;
using Shared.Responses;

namespace Dao.Interface
{
    [IncludeDependencyInjection]
    public interface IUnityOfWork
    {
        ISessionDao SessionDao { get; }
        IMeetingDao MeetingDao { get; }
        IDriverDao DriverDao { get; }
        ICarDataDao CarDataDao { get; }
        ISessionResultDao SessionResultDao { get; }
        IOvertakeDao OvertakeDao { get; }
        IPitDao PitDao { get; }
        IRaceControlDao RaceControlDao { get; }
        IStintDao StintDao { get; }
        ISessionResultQualifyDao SessionResultQualifyDao { get; }
        ILapDao LapDao { get; }
        ILapSegmentDao LapSegmentDao { get; }
        ILapFastSectorDao LapFastSectorDao { get; }

        Task<Response> Commit();
    }
}
