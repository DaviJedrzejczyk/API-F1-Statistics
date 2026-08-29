using Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dao.Interface
{
    public interface IUnityOfWork
    {
        ISessionDao SessionDao { get; }
        IMeetingDao MeetingDao { get; }
        IDriverDao DriverDao { get; }
        ICarDataDao CarDataDao { get; }
        ISessionResultDao SessionResultDao { get; }
        Task<Response> Commit();
    }
}
