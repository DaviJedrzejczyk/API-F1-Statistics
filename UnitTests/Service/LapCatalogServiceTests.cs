using System.Text.Json;
using Dao.Cache;
using Dao.Interface;
using Entities.Class;
using Entities.Dtos;
using ExternalApi.Interfaces;
using Moq;
using Services.Impl;
using Services.Interfaces;
using Services.Models;
using Shared.Responses;

namespace UnitTests.Service;

[TestFixture]
public sealed class LapCatalogServiceTests
{
    private Mock<IUnityOfWork> _unit = null!;
    private Mock<ILapClient> _laps = null!;
    private Mock<IStintService> _stints = null!;
    private Mock<IPitService> _pits = null!;
    private Mock<IRaceControlService> _controls = null!;
    private Dictionary<(string Dataset, string Key), ImportWindow> _windows = null!;
    private List<Lap> _stored = null!;
    private Dictionary<int, List<LapListDto>> _source = null!;
    private LapCatalogService _service = null!;
    private Session _session = null!;

    [SetUp]
    public void SetUp()
    {
        _unit = new Mock<IUnityOfWork> { DefaultValue = DefaultValue.Mock };
        _laps = new Mock<ILapClient>(MockBehavior.Strict);
        _stints = new Mock<IStintService>(MockBehavior.Strict);
        _pits = new Mock<IPitService>(MockBehavior.Strict);
        _controls = new Mock<IRaceControlService>(MockBehavior.Strict);
        _windows = [];
        _stored = [];
        _source = [];
        _session = new Session
        {
            SessionKey = 100, MeetingKey = 10,
            DateStart = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero),
            DateEnd = new DateTimeOffset(2024, 1, 1, 14, 0, 0, TimeSpan.Zero)
        };
        _unit.Setup(unit => unit.SessionDao.GetSessionsByKeys(It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => Success(new List<Session> { _session }));
        _unit.Setup(unit => unit.DriverDao.GetAllDriversSession(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success(new List<Driver>
            {
                new() { DriverNumber = 1, SessionKey = 100, MeetingKey = 10 },
                new() { DriverNumber = 2, SessionKey = 100, MeetingKey = 10 },
                new() { DriverNumber = 3, SessionKey = 100, MeetingKey = 10 },
                new() { DriverNumber = 4, SessionKey = 100, MeetingKey = 10 }
            }));
        _unit.Setup(unit => unit.GetImportWindow(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string dataset, string key, CancellationToken token) => _windows.GetValueOrDefault((dataset, key)));
        _unit.Setup(unit => unit.StageImportWindow(It.IsAny<ImportWindow>(), It.IsAny<CancellationToken>()))
            .Callback((ImportWindow window, CancellationToken token) => _windows[(window.Dataset, window.WindowKey)] = window)
            .Returns(Task.CompletedTask);
        Mock<IImportTransaction> transaction = new();
        transaction.Setup(transaction => transaction.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _unit.Setup(unit => unit.BeginImport(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction.Object);
        _unit.Setup(unit => unit.Commit(It.IsAny<CancellationToken>())).ReturnsAsync(new Response { HasSuccess = true });
        _unit.Setup(unit => unit.LapDao.GetSessionLaps(100, It.IsAny<int[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int session, int[] drivers, CancellationToken token) => Success(_stored.Where(lap => drivers.Contains(lap.DriverNumber)).ToList()));
        _unit.Setup(unit => unit.LapDao.UpsertLap(It.IsAny<Lap>(), It.IsAny<CancellationToken>()))
            .Callback((Lap lap, CancellationToken token) =>
            {
                _stored.RemoveAll(stored => stored.DriverNumber == lap.DriverNumber && stored.LapNumber == lap.LapNumber);
                _stored.Add(lap);
            }).ReturnsAsync(new Response { HasSuccess = true });
        foreach (int driver in new[] { 1, 2, 3, 4 })
        {
            _source[driver] = [SourceLap(driver, 1, 91), SourceLap(driver, 2, 90), SourceLap(driver, 3, null)];
        }
        _laps.Setup(client => client.GetAllLapsSessionByDriver(100, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int session, int driver, CancellationToken token) => Success(_source[driver]));
        _stints.Setup(service => service.GetStintsBySessionKey(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success(new List<Stint>
            {
                new() { SessionKey = 100, MeetingKey = 10, DriverNumber = 1, LapStart = 1, LapEnd = 3, Compound = "SOFT" },
                new() { SessionKey = 100, MeetingKey = 10, DriverNumber = 2, LapStart = 1, LapEnd = 3, Compound = "HARD" }
            }));
        _pits.Setup(service => service.GetPitsBySessionKeyApi(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success(new List<Pit>()));
        _controls.Setup(service => service.GetRaceControlsBySessionFlagsApi(100, It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success(new List<RaceControl>()));
        _service = new LapCatalogService(_unit.Object, _laps.Object, _stints.Object, _pits.Object, _controls.Object);
    }

    [Test]
    public async Task Import_PersistsEveryLapIncludingMissingMeasurements_AndReopensWithoutSourceCalls()
    {
        _source[1][0].LapDuration = 0;
        _source[1][2].IsPitOutLap = null;
        _source[1][2].DateStart = null;
        SingleResponse<LapCatalogDto> imported = await Get(import: true);
        Assert.That(imported.HasSuccess, Is.True, imported.Message);
        ClearExternalCalls();

        SingleResponse<LapCatalogDto> reopened = await Get();
        Assert.That(reopened.HasSuccess, Is.True, reopened.Message);
        Assert.That(_stored, Has.Count.EqualTo(9));
        CatalogLapDto missing = reopened.Item.Drivers[0].Laps[2];
        Assert.Multiple(() =>
        {
            Assert.That(reopened.Item.Drivers.All(driver => driver.ImportComplete), Is.True);
            Assert.That(reopened.Item.Drivers[0].Laps[0].Duration, Is.Zero);
            Assert.That(missing.Duration, Is.Null);
            Assert.That(missing.UnavailableData, Does.Contain("lap_duration").And.Contain("pit_out_status").And.Contain("lap_start_time"));
            Assert.That(reopened.Item.Drivers[2].Laps[0].Compound, Is.Null);
            Assert.That(reopened.Item.ComparisonAvailable, Is.False);
        });
        VerifyNoExternalCalls();
    }

    [Test]
    public async Task Import_WithLegacyFastestLap_DoesNotTreatItAsComplete_AndPreservesFastestFlag()
    {
        _stored.Add(new Lap { SessionKey = 100, MeetingKey = 10, DriverNumber = 1, LapNumber = 2, IsFastLap = true, LapDuration = 90 });
        SingleResponse<LapCatalogDto> response = await Get(import: true);
        Assert.That(response.HasSuccess, Is.True, response.Message);
        Assert.That(_stored.Single(lap => lap.DriverNumber == 1 && lap.LapNumber == 2).IsFastLap, Is.True);
        Assert.That(_stored, Has.Count.EqualTo(9));
        _laps.Verify(client => client.GetAllLapsSessionByDriver(100, 1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Reimport_UsesPerDriverCompletionAcrossDifferentTrios_WithoutDuplicatingRows()
    {
        await Get(import: true);
        ClearExternalCalls();
        SingleResponse<LapCatalogDto> response = await _service.GetCatalog(100, [1, 2, 4], null, null, null, true, default);
        Assert.That(response.HasSuccess, Is.True, response.Message);
        Assert.That(_stored, Has.Count.EqualTo(12));
        _laps.Verify(client => client.GetAllLapsSessionByDriver(100, 4, It.IsAny<CancellationToken>()), Times.Once);
        _laps.Verify(client => client.GetAllLapsSessionByDriver(100, 1, It.IsAny<CancellationToken>()), Times.Never);
        _laps.Verify(client => client.GetAllLapsSessionByDriver(100, 2, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Import_RepairsMissingPersistedRow_WithoutRefetchingOtherDrivers()
    {
        await Get(import: true);
        _stored.RemoveAll(lap => lap.DriverNumber == 1 && lap.LapNumber == 2);
        ClearExternalCalls();
        SingleResponse<LapCatalogDto> missing = await Get();
        AssertFailure(missing, 409);
        SingleResponse<LapCatalogDto> repaired = await Get(import: true);
        Assert.That(repaired.HasSuccess, Is.True, repaired.Message);
        Assert.That(_stored, Has.Count.EqualTo(9));
        _laps.Verify(client => client.GetAllLapsSessionByDriver(100, 1, It.IsAny<CancellationToken>()), Times.Once);
        _laps.Verify(client => client.GetAllLapsSessionByDriver(100, 2, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Import_GappedWindowRemainsExplicitlyIncomplete_AndCanBeRepaired()
    {
        _source[1].RemoveAt(1);
        SingleResponse<LapCatalogDto> response = await Get(import: true);
        Assert.That(response.HasSuccess, Is.True, response.Message);
        Assert.That(response.Item.Drivers[0].ImportComplete, Is.False);
        Assert.That(response.Item.Drivers[0].IncompleteReason, Does.Contain("gaps"));
        _source[1].Insert(1, SourceLap(1, 2, 90));
        SingleResponse<LapCatalogDto> repaired = await Get(import: true);
        Assert.That(repaired.Item.Drivers[0].ImportComplete, Is.True);
        Assert.That(_stored, Has.Count.EqualTo(9));
    }

    [Test]
    public async Task Import_ShrinkingResponseRetainsPreviousLapsAndReportsIncomplete()
    {
        await Get(import: true);
        _windows.Remove(("lap_catalog_v2", "100:1"));
        _source[1].RemoveAt(2);
        SingleResponse<LapCatalogDto> response = await Get(import: true);
        Assert.That(response.HasSuccess, Is.True, response.Message);
        Assert.That(response.Item.Drivers[0].ImportComplete, Is.False);
        Assert.That(response.Item.Drivers[0].Laps, Has.Count.EqualTo(3));
    }

    [Test]
    public async Task Import_EmptySuccessfulWindowIsPersistedWithoutInventedLaps()
    {
        _source[3].Clear();
        SingleResponse<LapCatalogDto> response = await Get(import: true);
        Assert.That(response.HasSuccess, Is.True, response.Message);
        Assert.That(response.Item.Drivers[2].Laps, Is.Empty);
        Assert.That(response.Item.Drivers[2].NoSuggestionReason, Is.Not.Empty);
        ClearExternalCalls();
        await Get(import: true);
        VerifyNoExternalCalls();
    }

    [Test]
    public async Task Import_FailedEnrichmentIsExplicit_AndLaterImportRetriesOnlyUnavailableSources()
    {
        _pits.Setup(service => service.GetPitsBySessionKeyApi(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DataResponse<Pit> { HasSuccess = false });
        SingleResponse<LapCatalogDto> first = await Get(import: true);
        Assert.That(first.Item.Drivers[0].Laps[0].UnavailableData, Does.Contain("pit"));
        ClearExternalCalls();
        _pits.Setup(service => service.GetPitsBySessionKeyApi(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success(new List<Pit> { new() { SessionKey = 100, MeetingKey = 10, DriverNumber = 1, LapNumber = 2 } }));

        SingleResponse<LapCatalogDto> reopened = await Get();
        Assert.That(reopened.Item.Drivers[0].Laps[0].UnavailableData, Does.Contain("pit"));
        VerifyNoExternalCalls();
        SingleResponse<LapCatalogDto> retried = await Get(import: true);
        Assert.That(retried.Item.Drivers[0].SuggestedLapNumber, Is.EqualTo(1));
        Assert.That(retried.Item.Drivers[0].Laps[1].Alerts.Select(alert => alert.Code), Does.Contain("pit_in"));
        _pits.Verify(service => service.GetPitsBySessionKeyApi(100, It.IsAny<CancellationToken>()), Times.Once);
        Assert.That(_laps.Invocations, Is.Empty);
        Assert.That(_stints.Invocations, Is.Empty);
        Assert.That(_controls.Invocations, Is.Empty);
    }

    [Test]
    public async Task Import_RefreshedStintSnapshotClosesIntervals_WithoutUsingStaleDaoRows()
    {
        _stints.Setup(service => service.GetStintsBySessionKey(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success(new List<Stint> { new() { SessionKey = 100, MeetingKey = 10, DriverNumber = 1, LapStart = 1, LapEnd = null, Compound = "SOFT" } }));
        SingleResponse<LapCatalogDto> first = await Get(import: true);
        Assert.That(first.Item.Drivers[0].Laps[0].Compound, Is.Null);
        _stints.Setup(service => service.GetStintsBySessionKey(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success(new List<Stint>
            {
                new() { SessionKey = 100, MeetingKey = 10, DriverNumber = 1, LapStart = 1, LapEnd = 1, Compound = "SOFT" },
                new() { SessionKey = 100, MeetingKey = 10, DriverNumber = 1, LapStart = 2, LapEnd = 3, Compound = "HARD" }
            }));
        await Get(import: true);
        ClearExternalCalls();
        SingleResponse<LapCatalogDto> reopened = await Get(compound: "hard");
        Assert.That(reopened.Item.Drivers[0].Laps.Select(lap => lap.LapNumber), Is.EqualTo(new[] { 2, 3 }));
        VerifyNoExternalCalls();
    }

    [Test]
    public async Task Filters_AreAppliedBeforeSuggestion_AndMissingDurationRemainsSelectable()
    {
        await Get(import: true);
        SingleResponse<LapCatalogDto> filtered = await Get(lapNumber: 1, compound: "soft", hasAlerts: false);
        Assert.That(filtered.Item.Drivers[0].SuggestedLapNumber, Is.EqualTo(1));
        Assert.That(filtered.Item.Drivers[1].SuggestedLapNumber, Is.Null);
        SingleResponse<LapCatalogDto> missing = await Get(lapNumber: 3);
        Assert.That(missing.Item.Drivers[0].Laps, Has.Count.EqualTo(1));
        Assert.That(missing.Item.Drivers[0].SuggestedLapNumber, Is.Null);
        Assert.That(missing.Item.Drivers[0].ManualSelectionAllowed, Is.True);
    }

    [Test]
    public async Task OverlappingOrOpenStints_DoNotInventACompound()
    {
        _stints.Setup(service => service.GetStintsBySessionKey(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success(new List<Stint>
            {
                new() { SessionKey = 100, MeetingKey = 10, DriverNumber = 1, LapStart = 1, LapEnd = null, Compound = "SOFT" },
                new() { SessionKey = 100, MeetingKey = 10, DriverNumber = 1, LapStart = 2, LapEnd = 3, Compound = "HARD" }
            }));
        SingleResponse<LapCatalogDto> response = await Get(import: true);

        Assert.That(response.Item.Drivers[0].Laps.All(lap => lap.Compound == null), Is.True);
        Assert.That(response.Item.Drivers[0].Laps[1].UnavailableData, Does.Contain("compound_not_identified"));
    }

    [Test]
    public async Task HistoricalSourceSnapshot_IsUsedBeforeEnrichmentEvenWhenLegacyCacheExpired()
    {
        _windows[("stints", "100")] = new ImportWindow
        {
            Dataset = "stints", WindowKey = "100", IsComplete = true,
            CompletedAtUtc = _session.DateEnd.AddDays(1), ExpiresAtUtc = _session.DateEnd.AddDays(1).AddMinutes(5),
            Payload = JsonSerializer.Serialize(new List<Stint>
            {
                new() { SessionKey = 100, MeetingKey = 10, DriverNumber = 1, LapStart = 1, LapEnd = 3, Compound = "MEDIUM" }
            })
        };

        SingleResponse<LapCatalogDto> response = await Get(import: true);

        Assert.That(response.Item.Drivers[0].Laps[0].Compound, Is.EqualTo("MEDIUM"));
        Assert.That(_stints.Invocations, Is.Empty);
    }

    [Test]
    public async Task Preview_MissingLapRejectsSelectionWithoutPersistingPreview()
    {
        await Get(import: true);
        LapSelectionRequest request = new()
        {
            Selections = [new() { DriverNumber = 1, LapNumber = 99 }, new() { DriverNumber = 2, LapNumber = 2 }, new() { DriverNumber = 3, LapNumber = 3 }]
        };

        SingleResponse<LapSelectionDto> response = await _service.Preview(100, request, default);

        AssertFailure(response, 400);
        Assert.That(_windows.Keys.Any(key => key.Dataset == "lap_preview"), Is.False);
    }

    [Test]
    public async Task Suggestion_ExcludesOnlyIdentifiedPitAndDeletion_AndKeepsOtherAlertsEligible()
    {
        _source[1][1].IsPitOutLap = true;
        _controls.Setup(service => service.GetRaceControlsBySessionFlagsApi(100, It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success(new List<RaceControl>
            {
                Control(1, 1, "YELLOW FLAG"),
                Control(2, 2, "CAR 2 LAP 2 TIME DELETED"),
                Control(3, 2, "CAR 3 LAP TIME DELETED")
            }));
        SingleResponse<LapCatalogDto> response = await Get(import: true);
        Assert.Multiple(() =>
        {
            Assert.That(response.Item.Drivers[0].SuggestedLapNumber, Is.EqualTo(1));
            Assert.That(response.Item.Drivers[1].SuggestedLapNumber, Is.EqualTo(1));
            Assert.That(response.Item.Drivers[2].SuggestedLapNumber, Is.EqualTo(2));
            Assert.That(response.Item.Drivers[2].Laps[1].UnavailableData, Does.Contain("time_deletion_lap_not_identified"));
        });
        SingleResponse<LapCatalogDto> alerted = await Get(hasAlerts: true);
        Assert.That(alerted.Item.Drivers[0].SuggestedLapNumber, Is.EqualTo(1));
    }

    [Test]
    public async Task Reinstatement_RestoresEligibilityButPreservesRaceControlHistory()
    {
        RaceControl deleted = Control(1, 3, "CAR 1 LAP 2 TIME DELETED");
        RaceControl restored = Control(1, 3, "CAR 1 LAP 2 TIME REINSTATED");
        restored.Date = deleted.Date.AddSeconds(1);
        _controls.Setup(service => service.GetRaceControlsBySessionFlagsApi(100, It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success(new List<RaceControl> { deleted, restored }));
        SingleResponse<LapCatalogDto> response = await Get(import: true);
        CatalogLapDto lap = response.Item.Drivers[0].Laps[1];
        Assert.That(lap.SuggestedEligible, Is.True);
        Assert.That(lap.Alerts, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task UnidentifiedDriverAlert_IsNotAppliedToEveryCarAtTheSameTime()
    {
        RaceControl unidentified = new()
        {
            SessionKey = 100, MeetingKey = 10, Scope = "Driver", DriverNumber = null,
            Date = _source[1][0].DateStart!.Value.AddSeconds(20), Message = "DRIVER WARNING"
        };
        RaceControl track = new()
        {
            SessionKey = 100, MeetingKey = 10, Scope = "Track", DriverNumber = null,
            Date = _source[1][1].DateStart!.Value.AddSeconds(20), Message = "YELLOW FLAG"
        };
        _controls.Setup(service => service.GetRaceControlsBySessionFlagsApi(100, It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success(new List<RaceControl> { unidentified, track }));

        SingleResponse<LapCatalogDto> response = await Get(import: true);

        Assert.That(response.Item.Drivers[0].Laps[0].Alerts, Is.Empty);
        Assert.That(response.Item.Drivers[0].Laps[0].UnavailableData, Does.Contain("race_control_driver_or_scope_not_identified"));
        Assert.That(response.Item.Drivers[0].Laps[1].Alerts.Single().Message, Is.EqualTo("YELLOW FLAG"));
    }

    [Test]
    public async Task PreviewAndConfirm_PreserveIndependentManualChoicesAndAlerts_EvenIfCatalogLaterChanges()
    {
        _source[1][0].IsPitOutLap = true;
        await Get(import: true);
        LapSelectionRequest request = new()
        {
            Selections = [new() { DriverNumber = 1, LapNumber = 1 }, new() { DriverNumber = 2, LapNumber = 2 }, new() { DriverNumber = 3, LapNumber = 3 }]
        };
        SingleResponse<LapSelectionDto> preview = await _service.Preview(100, request, default);
        Assert.That(preview.HasSuccess, Is.True, preview.Message);
        Assert.That(preview.Item.Confirmed, Is.False);
        _windows.Remove(("lap_catalog_v2", "100:1"));
        ClearExternalCalls();

        SingleResponse<LapSelectionDto> confirmed = await _service.Confirm(100, preview.Item.PreviewId, default);
        SingleResponse<LapSelectionDto> reopened = await _service.GetSelection(100, null, default);
        Assert.That(confirmed.HasSuccess, Is.True, confirmed.Message);
        Assert.Multiple(() =>
        {
            Assert.That(reopened.Item.Confirmed, Is.True);
            Assert.That(reopened.Item.Selections.Select(lap => lap.LapNumber), Is.EqualTo(new[] { 1, 2, 3 }));
            Assert.That(reopened.Item.Selections[0].Alerts.Select(alert => alert.Code), Does.Contain("pit_out"));
            Assert.That(reopened.Item.Selections[2].Duration, Is.Null);
            Assert.That(reopened.Item.ComparisonAvailable, Is.False);
        });
        _unit.Verify(unit => unit.BeginImport("lap_selection", "100", It.IsAny<CancellationToken>()), Times.Once);
        VerifyNoExternalCalls();
    }

    [Test]
    public async Task Preview_RejectsNullArrayElementsBeforeAccessingProperties()
    {
        LapSelectionRequest request = JsonSerializer.Deserialize<LapSelectionRequest>("{\"selections\":[null,{\"driver_number\":2,\"lap_number\":1},{\"driver_number\":3,\"lap_number\":1}]}")!;
        SingleResponse<LapSelectionDto> response = await _service.Preview(100, request, default);
        AssertFailure(response, 400);
        VerifyNoExternalCalls();
    }

    [Test]
    public async Task Confirm_RequiresAnExistingPreview()
    {
        SingleResponse<LapSelectionDto> response = await _service.Confirm(100, Guid.NewGuid(), default);
        AssertFailure(response, 404);
        Assert.That(_windows, Is.Empty);
    }

    [TestCase(new[] { 1, 1, 3 })]
    [TestCase(new[] { 1, 2 })]
    [TestCase(new[] { 1, 2, 99 })]
    public async Task InvalidDrivers_AreRejectedWithoutSourceRequests(int[] drivers)
    {
        SingleResponse<LapCatalogDto> response = await _service.GetCatalog(100, drivers, null, null, null, true, default);
        AssertFailure(response, 400);
        VerifyNoExternalCalls();
    }

    [Test]
    public async Task LiveSession_IsRejectedWithoutSourceRequests()
    {
        _session.DateEnd = DateTimeOffset.UtcNow.AddMinutes(-10);
        SingleResponse<LapCatalogDto> response = await Get(import: true);
        AssertFailure(response, 400);
        VerifyNoExternalCalls();
    }

    [Test]
    public async Task FailedSource_IsNotRecordedAsAnEmptyCompleteCatalog()
    {
        _laps.Setup(client => client.GetAllLapsSessionByDriver(100, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DataResponse<LapListDto> { HasSuccess = false, Message = "429" });
        SingleResponse<LapCatalogDto> response = await Get(import: true);
        AssertFailure(response, 503);
        Assert.That(_windows, Is.Empty);
    }

    [Test]
    public async Task DuplicateLapIdentity_IsRejectedWithoutPersistingTheWindow()
    {
        _source[1].Add(SourceLap(1, 1, 92));
        SingleResponse<LapCatalogDto> response = await Get(import: true);
        AssertFailure(response, 502);
        Assert.That(_stored, Is.Empty);
    }

    [Test]
    public async Task DatabaseReadFailure_DoesNotFallBackToOpenF1()
    {
        _unit.Setup(unit => unit.LapDao.GetSessionLaps(100, It.IsAny<int[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DataResponse<Lap> { HasSuccess = false });
        SingleResponse<LapCatalogDto> response = await Get(import: true);
        AssertFailure(response, 503);
        VerifyNoExternalCalls();
    }

    private Task<SingleResponse<LapCatalogDto>> Get(bool import = false, int? lapNumber = null, string? compound = null, bool? hasAlerts = null)
    {
        return _service.GetCatalog(100, [1, 2, 3], lapNumber, compound, hasAlerts, import, default);
    }

    private static LapListDto SourceLap(int driver, int number, double? duration)
    {
        return new LapListDto
        {
            SessionKey = 100, MeetingKey = 10, DriverNumber = driver, LapNumber = number,
            LapDuration = duration, IsPitOutLap = false, DateStart = new DateTime(2024, 1, 1, 12, number * 2, 0, DateTimeKind.Utc)
        };
    }

    private static RaceControl Control(int driver, int lap, string message)
    {
        return new RaceControl { SessionKey = 100, MeetingKey = 10, DriverNumber = driver, LapNumber = lap, Message = message };
    }

    private static DataResponse<T> Success<T>(List<T> items) => new() { HasSuccess = true, Itens = items };

    private static void AssertFailure<T>(SingleResponse<T> response, int status)
    {
        Assert.That(response.HasSuccess, Is.False);
        Assert.That(response.Exception, Is.TypeOf<LapCatalogException>(), response.Message);
        Assert.That(((LapCatalogException)response.Exception).StatusCode, Is.EqualTo(status));
    }

    private void ClearExternalCalls()
    {
        _laps.Invocations.Clear();
        _stints.Invocations.Clear();
        _pits.Invocations.Clear();
        _controls.Invocations.Clear();
    }

    private void VerifyNoExternalCalls()
    {
        Assert.That(_laps.Invocations, Is.Empty);
        Assert.That(_stints.Invocations, Is.Empty);
        Assert.That(_pits.Invocations, Is.Empty);
        Assert.That(_controls.Invocations, Is.Empty);
    }
}
