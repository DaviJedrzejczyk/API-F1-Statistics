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
        private IStintDao? stint = null;
        private ISessionResultQualifyDao? sessionResultQualify = null;
        private ILapDao? lap = null;
        private ILapSegmentDao? lapSegment = null;

        public UnityOfWork(ApiF1DB db, ISessionDao sessionDao, IMeetingDao meetingDao, IDriverDao driverDao, ICarDataDao carDataDao, ISessionResultDao resultDao, IOvertakeDao overtakeDao, IPitDao pitDao, IRaceControlDao raceControlDao, IStintDao stintDao, ISessionResultQualifyDao sessionResultQualifyDao, ILapDao lapDao, ILapSegmentDao lapSegmentDao)
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
            stint = stintDao;
            sessionResultQualify = sessionResultQualifyDao;
            lap = lapDao;
            lapSegment = lapSegmentDao;
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

        public IStintDao StintDao
        {
            get
            {
                stint ??= new StintDao(_db);
                return stint;
            }
        }

        public ISessionResultQualifyDao SessionResultQualifyDao
        {
            get
            {
                sessionResultQualify ??= new SessionResultQualifyDao(_db);
                return sessionResultQualify;
            }
        }

        public ILapDao LapDao
        {
            get
            {
                lap ??= new LapDao(_db);
                return lap;
            }
        }

        public ILapSegmentDao LapSegmentDao
        {
            get
            {
                lapSegment ??= new LapSegmentDao(_db);
                return lapSegment;
            }
        }

        public void Dispose()
        {
            _db?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
