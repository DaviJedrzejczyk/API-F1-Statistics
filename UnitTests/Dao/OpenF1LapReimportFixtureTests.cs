using System.Text.Json;
using Dao;
using Dao.Impl;
using Entities.Class;
using Entities.Dtos;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.Dao;

[TestFixture]
public class OpenF1LapReimportFixtureTests
{
    [Test]
    public async Task Reimport_ThreeDriversWithDifferentLapCounts_DoesNotDuplicateRows()
    {
        var options = new DbContextOptionsBuilder<ApiF1DB>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var db = new ApiF1DB(options);
        var dao = new LapDao(db);
        var source = new[] { 11, 22, 33 }
            .SelectMany(driver => JsonSerializer.Deserialize<List<LapListDto>>(
                File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory,
                    "Fixtures", "OpenF1", $"laps-{driver}.json")))!).ToList();

        foreach (var item in source)
        {
            var response = await dao.UpsertLap(ToLap(item), CancellationToken.None);
            Assert.That(response.HasSuccess, Is.True);
        }
        await db.SaveChangesAsync();

        foreach (var item in source)
        {
            var response = await dao.UpsertLap(ToLap(item), CancellationToken.None);
            Assert.That(response.HasSuccess, Is.True);
        }
        await db.SaveChangesAsync();

        var persisted = await db.Laps.AsNoTracking().ToListAsync();
        Assert.That(persisted, Has.Count.EqualTo(9));
        Assert.That(persisted.Where(x => x.DriverNumber == 11).Select(x => x.LapNumber), Is.EquivalentTo(new[] { 1, 2, 3 }));
        Assert.That(persisted.Where(x => x.DriverNumber == 22).Select(x => x.LapNumber), Is.EquivalentTo(new[] { 1, 2, 3, 4 }));
        Assert.That(persisted.Where(x => x.DriverNumber == 33).Select(x => x.LapNumber), Is.EquivalentTo(new[] { 1, 2 }));
        Assert.That(persisted.Single(x => x.DriverNumber == 22 && x.LapNumber == 1).LapDuration, Is.Zero);
        Assert.That(persisted.Single(x => x.DriverNumber == 11 && x.LapNumber == 1).LapDuration, Is.Null);
    }

    private static Lap ToLap(LapListDto dto) => new()
    {
        MeetingKey = dto.MeetingKey,
        SessionKey = dto.SessionKey,
        DriverNumber = dto.DriverNumber,
        LapNumber = dto.LapNumber,
        LapDuration = dto.LapDuration,
        IsPitOutLap = dto.IsPitOutLap,
        DateStart = dto.DateStart
    };
}
