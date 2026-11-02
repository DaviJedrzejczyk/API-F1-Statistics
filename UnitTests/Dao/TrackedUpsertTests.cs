using Dao;
using Dao.Impl;
using Entities.Class;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.Dao;

[TestFixture]
public class TrackedUpsertTests
{
    [Test]
    public async Task Upserts_UpdateTrackedEntitiesBeforeTheyAreSaved()
    {
        var options = new DbContextOptionsBuilder<ApiF1DB>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using var db = new ApiF1DB(options);
        var lapDao = new LapDao(db);
        var sectorDao = new LapFastSectorDao(db);
        var firstLap = new Lap { MeetingKey = 1, SessionKey = 2, DriverNumber = 3, LapNumber = 4, LapDuration = 90 };
        var secondLap = new Lap { MeetingKey = 1, SessionKey = 2, DriverNumber = 3, LapNumber = 4, LapDuration = 80 };
        var firstSector = new LapFastSector { SessionKey = 2, DriverNumber = 3, Sector = 1, Duration = 30, DriverName = "Driver" };
        var secondSector = new LapFastSector { SessionKey = 2, DriverNumber = 3, Sector = 1, Duration = 25, DriverName = "Driver" };

        Assert.That((await lapDao.UpsertLap(firstLap, default)).HasSuccess, Is.True);
        Assert.That((await lapDao.UpsertLap(secondLap, default)).HasSuccess, Is.True);
        Assert.That((await sectorDao.UpsertLapFastSectors([firstSector, secondSector], default)).HasSuccess, Is.True);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        Assert.That((await db.Laps.SingleAsync()).LapDuration, Is.EqualTo(80));
        Assert.That((await db.LapFastSectors.SingleAsync()).Duration, Is.EqualTo(25));
    }
}
