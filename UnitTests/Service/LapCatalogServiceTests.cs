using Dao.Cache;
using Dao.Interface;
using Entities.Class;
using ExternalApi.Interfaces;
using Moq;
using NUnit.Framework;
using Services.Impl;
using Services.Interfaces;
using Services.Models;
using Shared.Responses;

namespace UnitTests.Service;

[TestFixture]
public class LapCatalogServiceTests
{
    private static LapCatalogService CreateStoredService(List<Lap> laps, List<Stint>? stints = null,
        List<Pit>? pits = null, List<RaceControl>? controls = null)
    {
        var unit = new Mock<IUnityOfWork>();
        var sessions = new Mock<ISessionDao>();
        var lapDao = new Mock<ILapDao>();
        var stintDao = new Mock<IStintDao>();
        var pitDao = new Mock<IPitDao>();
        var controlDao = new Mock<IRaceControlDao>();
        unit.SetupGet(x => x.SessionDao).Returns(sessions.Object);
        unit.SetupGet(x => x.LapDao).Returns(lapDao.Object);
        unit.SetupGet(x => x.StintDao).Returns(stintDao.Object);
        unit.SetupGet(x => x.PitDao).Returns(pitDao.Object);
        unit.SetupGet(x => x.RaceControlDao).Returns(controlDao.Object);
        sessions.Setup(x => x.GetSessionsByKeys(It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.CreateInstance().CreateSuccessDataResponse(new List<Session>
            { new() { SessionKey = 10, MeetingKey = 20, DateEnd = DateTimeOffset.UtcNow.AddDays(-1) } }));
        unit.Setup(x => x.GetImportWindow("lap_catalog", "10:1,2,3", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ImportWindow { IsComplete = true, Payload = "[]" });
        lapDao.Setup(x => x.GetSessionLaps(10, It.IsAny<int[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.CreateInstance().CreateSuccessDataResponse(laps));
        stintDao.Setup(x => x.GetStintsBySessionKey(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.CreateInstance().CreateSuccessDataResponse(stints ?? []));
        pitDao.Setup(x => x.GetAllPitsBySessionKey(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.CreateInstance().CreateSuccessDataResponse(pits ?? []));
        controlDao.Setup(x => x.GetRaceControlsBySession(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResponseFactory.CreateInstance().CreateSuccessDataResponse(controls ?? []));
        return new LapCatalogService(unit.Object, Mock.Of<ILapClient>(), Mock.Of<IStintService>(),
            Mock.Of<IPitService>(), Mock.Of<IRaceControlService>());
    }

    [Test]
    public async Task SuggestionExcludesIdentifiedPitAndDeletionButKeepsManualChoice()
    {
        var laps = new List<Lap>
        {
            new() { SessionKey = 10, MeetingKey = 20, DriverNumber = 1, LapNumber = 1, LapDuration = 70, IsPitOutLap = true },
            new() { SessionKey = 10, MeetingKey = 20, DriverNumber = 1, LapNumber = 2, LapDuration = 71 },
            new() { SessionKey = 10, MeetingKey = 20, DriverNumber = 2, LapNumber = 1, LapDuration = 68 },
            new() { SessionKey = 10, MeetingKey = 20, DriverNumber = 2, LapNumber = 2, LapDuration = 72 },
            new() { SessionKey = 10, MeetingKey = 20, DriverNumber = 3, LapNumber = 1, LapDuration = null }
        };
        var controls = new List<RaceControl> { new() { SessionKey = 10, MeetingKey = 20,
            DriverNumber = 2, LapNumber = 1, Message = "LAP TIME DELETED" } };
        var service = CreateStoredService(laps, controls: controls);
        var catalog = await service.Get(10, [1, 2, 3], null, null, null, false, default);
        Assert.That(catalog.Drivers.Select(x => x.SuggestedLapNumber), Is.EqualTo(new int?[] { 2, 2, null }));
        Assert.That(catalog.Drivers[2].ManualSelectionAllowed, Is.True);
        var preview = await service.Select(10, new LapSelectionRequest([
            new(1, 1), new(2, 2), new(3, 1)]), false, default);
        Assert.That(preview.Selections.Select(x => x.LapNumber), Is.EqualTo(new[] { 1, 2, 1 }));
        Assert.That(preview.Confirmed, Is.False);
    }

    [Test]
    public async Task FiltersApplyBeforeSuggestionAndPreserveUnknownCompound()
    {
        var service = CreateStoredService([
            new() { SessionKey = 10, MeetingKey = 20, DriverNumber = 1, LapNumber = 1, LapDuration = 70 },
            new() { SessionKey = 10, MeetingKey = 20, DriverNumber = 2, LapNumber = 1, LapDuration = 80 },
            new() { SessionKey = 10, MeetingKey = 20, DriverNumber = 3, LapNumber = 1, LapDuration = 90 }
        ], stints: [new() { SessionKey = 10, MeetingKey = 20, DriverNumber = 1,
            LapStart = 1, LapEnd = 2, Compound = "SOFT" }]);
        var result = await service.Get(10, [1, 2, 3], 1, "SOFT", false, false, default);
        Assert.That(result.Drivers[0].SuggestedLapNumber, Is.EqualTo(1));
        Assert.That(result.Drivers[1].Laps, Is.Empty);
        Assert.That(result.Drivers[2].NoSuggestionReason, Is.Not.Null);
    }
}
