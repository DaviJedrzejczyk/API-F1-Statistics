using Dao.Interface;
using Shared.Responses;
using Shared.Common.Atrributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using System.Security.Cryptography;
using System.Text;


namespace Dao.Impl
{
    [IncludeDependencyInjection]
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
        private ILapFastSectorDao? lapFastSector = null;

        public UnityOfWork(ApiF1DB db, ISessionDao sessionDao, IMeetingDao meetingDao, IDriverDao driverDao, ICarDataDao carDataDao, ISessionResultDao resultDao, IOvertakeDao overtakeDao, IPitDao pitDao, IRaceControlDao raceControlDao, IStintDao stintDao, ISessionResultQualifyDao sessionResultQualifyDao, ILapDao lapDao, ILapSegmentDao lapSegmentDao, ILapFastSectorDao lapFastSectorDao)
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
            lapFastSector = lapFastSectorDao;
        }

        public async Task<Response> Commit()
            => await Commit(CancellationToken.None);

        public async Task<Response> Commit(CancellationToken cancellationToken)
        {
            try
            {
                await _db.SaveChangesAsync(cancellationToken);
                return ResponseFactory.CreateInstance().CreateSuccessResponse();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                return ResponseFactory.CreateInstance().CreateFailureResponse(ex);
            }
        }

        public async Task<Dao.Cache.ImportWindow?> GetImportWindow(string dataset, string windowKey, CancellationToken cancellationToken = default)
            => await _db.ImportWindows.AsNoTracking().SingleOrDefaultAsync(x => x.Dataset == dataset && x.WindowKey == windowKey, cancellationToken);

        public async Task StageImportWindow(Dao.Cache.ImportWindow window, CancellationToken cancellationToken = default)
        {
            var existing = await _db.ImportWindows.FindAsync(new object[] { window.Dataset, window.WindowKey }, cancellationToken);
            if (existing == null)
                _db.ImportWindows.Add(window);
            else
                _db.Entry(existing).CurrentValues.SetValues(window);
        }

        public async Task<IImportTransaction> BeginImport(string dataset, string windowKey, CancellationToken cancellationToken)
        {
            var ownsTransaction = _db.Database.CurrentTransaction == null;
            var transaction = _db.Database.CurrentTransaction ?? await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                var resource = "F1-import-" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(dataset + "\n" + windowKey)));
                await _db.Database.ExecuteSqlInterpolatedAsync($"DECLARE @result int; EXEC @result = sp_getapplock @Resource={resource}, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=30000; IF @result < 0 THROW 50000, 'Unable to lock import window', 1;", cancellationToken);
                return new ImportTransaction(transaction, ownsTransaction);
            }
            catch { if (ownsTransaction) await transaction.DisposeAsync(); throw; }
        }

        private sealed class ImportTransaction(IDbContextTransaction transaction, bool ownsTransaction) : IImportTransaction
        {
            public Task CommitAsync(CancellationToken cancellationToken) => ownsTransaction ? transaction.CommitAsync(cancellationToken) : Task.CompletedTask;
            public ValueTask DisposeAsync() => ownsTransaction ? transaction.DisposeAsync() : ValueTask.CompletedTask;
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

        public ILapFastSectorDao LapFastSectorDao
        {
            get
            {
                lapFastSector ??= new LapFastSectorDao(_db);
                return lapFastSector;
            }
        }

        public void Dispose()
        {
            _db?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
