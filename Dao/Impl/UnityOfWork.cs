using Dao.Interface;
using Shared.Responses;


namespace Dao.Impl
{
    public class UnityOfWork : IUnityOfWork
    {
        private readonly ApiF1DB _db;
        private ISessionDao? session = null;
        private IMeetingDao? meeting = null;
        private IDriverDao? driver = null;
        private ICarDataDao? carData = null;
        private ISessionResultDao? sessionResult = null;
        private IOvertakeDao? overtake = null;
        private IPitDao? pit = null;
        private IRaceControlDao? raceControl = null;

        public UnityOfWork(ApiF1DB db, ISessionDao sessionDao, IMeetingDao meetingDao, IDriverDao driverDao, ICarDataDao carDataDao, ISessionResultDao resultDao, IOvertakeDao overtakeDao, IPitDao pitDao, IRaceControlDao raceControlDao)
        {
            _db = db;
            session = sessionDao;
            meeting = meetingDao;
            driver = driverDao;
            carData = carDataDao;
            sessionResult = resultDao;
            overtake = overtakeDao;
            pit = pitDao;
            raceControl = raceControlDao;
        }

        public async Task<Response> Commit()
        {
            try
            {
                await _db.SaveChangesAsync();
                return ResponseFactory.CreateInstance().CreateSuccessResponse();
            }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }
        public ISessionDao SessionDao
        {
            get
            {
                session ??= new SessionDao(_db);
                return session;
            }
        }

        public IMeetingDao MeetingDao
        {
            get
            {
                meeting ??= new MeetingDao(_db);
                return meeting;
            }
        }

        public IDriverDao DriverDao
        {
            get
            {
                driver ??= new DriverDao(_db);
                return driver;
            }
        }

        public ICarDataDao CarDataDao
        {
            get
            {
                carData ??= new CarDataDao(_db);
                return carData;
            }
        }

        public ISessionResultDao SessionResultDao
        {
            get
            {
                sessionResult ??= new SessionResultDao(_db);
                return sessionResult;
            }
        }

        public IOvertakeDao OvertakeDao
        {
            get
            {
                overtake ??= new OvertakeDao(_db);
                return overtake;
            }
        }

        public IPitDao PitDao
        {
            get
            {
                pit ??= new PitDao(_db);
                return pit;
            }
        }

        public IRaceControlDao RaceControlDao
        {
            get
            {
                raceControl ??= new RaceControlDao(_db);
                return raceControl;
            }
        }
        public void Dispose()
        {
            _db?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
