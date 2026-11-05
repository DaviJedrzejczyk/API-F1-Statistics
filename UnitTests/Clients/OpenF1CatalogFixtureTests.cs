using AutoMapper;
using Entities.Class;
using Entities.Dtos;
using ExternalApi.Impls;
using ExternalApi.Interfaces;
using Moq;
using Shared.Responses;

namespace UnitTests.Clients;

[TestFixture]
public class OpenF1CatalogFixtureTests
{
    private static string Fixture(string name) =>
        File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "OpenF1", name));

    [TestCase(11, 3)]
    [TestCase(22, 4)]
    [TestCase(33, 2)]
    public async Task Laps_ThreeDriversKeepTheirOwnLapNumbers(int driverNumber, int count)
    {
        var api = new Mock<IF1ApiClient>();
        api.Setup(x => x.Get("laps?", $"session_key=2200&driver_number={driverNumber}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SingleResponse<string> { HasSuccess = true, Item = Fixture($"laps-{driverNumber}.json") });

        var result = await new LapClient(api.Object).GetAllLapsSessionByDriver(2200, driverNumber);

        Assert.That(result.HasSuccess, Is.True);
        Assert.That(result.Itens, Has.Count.EqualTo(count));
        Assert.That(result.Itens.Select(x => x.DriverNumber), Is.All.EqualTo(driverNumber));
        Assert.That(result.Itens.Select(x => x.LapNumber), Is.EqualTo(Enumerable.Range(1, count)));
        Assert.That(result.Itens.Select(x => x.SessionKey), Is.All.EqualTo(2200));
    }

    [Test]
    public async Task Laps_PreserveNullZeroAndPitOutSignal()
    {
        var api = new Mock<IF1ApiClient>();
        api.Setup(x => x.Get("laps?", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SingleResponse<string> { HasSuccess = true, Item = Fixture("laps-11.json") });
        var result = await new LapClient(api.Object).GetAllLapsSessionByDriver(2200, 11);
        Assert.Multiple(() =>
        {
            Assert.That(result.Itens[0].LapDuration, Is.Null);
            Assert.That(result.Itens[0].IsPitOutLap, Is.True);
            Assert.That(result.Itens[1].LapDuration, Is.EqualTo(91.25));
        });

        api.Setup(x => x.Get("laps?", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SingleResponse<string> { HasSuccess = true, Item = Fixture("laps-22.json") });
        result = await new LapClient(api.Object).GetAllLapsSessionByDriver(2200, 22);
        Assert.Multiple(() =>
        {
            Assert.That(result.Itens[0].LapDuration, Is.Zero);
            Assert.That(result.Itens[2].LapDuration, Is.Null);
            Assert.That(result.Itens[1].SegmentsSector1, Is.EqualTo(new int?[] { 2049, null, 0 }));
        });
    }

    [Test]
    public async Task Stints_MissingCompoundReachesMapperAsUnknown()
    {
        var api = new Mock<IF1ApiClient>();
        var mapper = new Mock<IMapper>();
        List<StintListDTO>? captured = null;
        api.Setup(x => x.Get("stints?", "session_key=2200", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SingleResponse<string> { HasSuccess = true, Item = Fixture("stints.json") });
        mapper.Setup(x => x.Map<List<Stint>>(It.IsAny<List<StintListDTO>>()))
            .Callback<object>(source => captured = (List<StintListDTO>)source)
            .Returns(new List<Stint>());

        var result = await new StintClient(api.Object, mapper.Object).GetAllStintsBySession(2200);

        Assert.That(result.HasSuccess, Is.True);
        Assert.That(captured, Has.Count.EqualTo(3));
        Assert.That(captured!.Single(x => x.DriverNumber == 22).Compound, Is.Empty);
        Assert.That(captured.Single(x => x.DriverNumber == 22).LapEnd, Is.EqualTo(4));
    }

    [Test]
    public async Task Pits_KeepDriverAndLapAssociation()
    {
        var api = new Mock<IF1ApiClient>();
        var mapper = new Mock<IMapper>();
        List<PitDto>? captured = null;
        api.Setup(x => x.Get("pit?", "session_key=2200", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SingleResponse<string> { HasSuccess = true, Item = Fixture("pits.json") });
        mapper.Setup(x => x.Map<List<Pit>>(It.IsAny<List<PitDto>>()))
            .Callback<object>(source => captured = (List<PitDto>)source)
            .Returns(new List<Pit>());

        var result = await new PitClient(api.Object, mapper.Object).GetAllPitsSession(2200);

        Assert.That(result.HasSuccess, Is.True);
        Assert.That(captured!.Select(x => (x.DriverNumber, x.LapNumber)), Is.EqualTo(new[] { (11, 3), (22, 4) }));
        Assert.That(captured[1].StopDuration, Is.Null);
    }

    [Test]
    public async Task RaceControl_PreservesExplicitDeletion()
    {
        var api = new Mock<IF1ApiClient>();
        var mapper = new Mock<IMapper>();
        List<RaceControlDto>? captured = null;
        api.Setup(x => x.Get("race_control?", "session_key=2200", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SingleResponse<string> { HasSuccess = true, Item = Fixture("race-control.json") });
        mapper.Setup(x => x.Map<List<RaceControl>>(It.IsAny<List<RaceControlDto>>()))
            .Callback<object>(source => captured = (List<RaceControlDto>)source)
            .Returns(new List<RaceControl>());

        var result = await new RaceControlClient(api.Object, mapper.Object)
            .GetRaceControlsBySessionFlags(2200, Array.Empty<string>());

        Assert.That(result.HasSuccess, Is.True);
        Assert.That(captured, Has.Count.EqualTo(2));
        Assert.That(captured![0].DriverNumber, Is.EqualTo(22));
        Assert.That(captured[0].LapNumber, Is.EqualTo(2));
        Assert.That(captured[0].Message, Does.Contain("LAP TIME DELETED"));
        Assert.That(captured[1].Flag, Is.EqualTo("YELLOW"));
    }

    [Test]
    public async Task RaceControl_EncodesRequestedFlagFilters()
    {
        var api = new Mock<IF1ApiClient>();
        var mapper = new Mock<IMapper>();
        api.Setup(x => x.Get("race_control?", "session_key=2200&flag=YELLOW&flag=GREEN", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SingleResponse<string> { HasSuccess = true, Item = "[]" });
        mapper.Setup(x => x.Map<List<RaceControl>>(It.IsAny<List<RaceControlDto>>()))
            .Returns(new List<RaceControl>());

        var result = await new RaceControlClient(api.Object, mapper.Object)
            .GetRaceControlsBySessionFlags(2200, new[] { "YELLOW", "GREEN" });

        Assert.That(result.HasSuccess, Is.True);
        Assert.That(result.Itens, Is.Empty);
    }
}
