using System.Text.Json;
using System.Text.Json.Nodes;
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
public sealed class LapCatalogIndependentTests
{
    private Mock<IUnityOfWork> _unit = null!;
    private Mock<ILapClient> _laps = null!;
    private Mock<IStintService> _stints = null!;
    private Mock<IPitService> _pits = null!;
    private Mock<IRaceControlService> _controls = null!;
    private Mock<IImportTransaction> _transaction = null!;
    private Dictionary<(string Dataset, string Key), ImportWindow> _windows = null!;
    private Dictionary<int, List<LapListDto>> _sources = null!;
    private List<Lap> _rows = null!;
    private List<Stint> _stintRows = null!;
    private List<Pit> _pitRows = null!;
    private List<RaceControl> _controlRows = null!;
    private LapCatalogService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _unit = new Mock<IUnityOfWork> { DefaultValue = DefaultValue.Mock };
        _laps = new Mock<ILapClient>(MockBehavior.Strict);
        _stints = new Mock<IStintService>(MockBehavior.Strict);
        _pits = new Mock<IPitService>(MockBehavior.Strict);
        _controls = new Mock<IRaceControlService>(MockBehavior.Strict);
        _transaction = new Mock<IImportTransaction>();
        _transaction.Setup(transaction => transaction.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _windows = [];
        _rows = [];
        _sources = [];
        _stintRows =
        [
            new() { SessionKey = 2200, MeetingKey = 1100, DriverNumber = 11, LapStart = 1, LapEnd = 3, Compound = "MEDIUM" },
            new() { SessionKey = 2200, MeetingKey = 1100, DriverNumber = 33, LapStart = 1, LapEnd = 2, Compound = "HARD" }
        ];
        _pitRows = [new() { SessionKey = 2200, MeetingKey = 1100, DriverNumber = 11, LapNumber = 3 }];
        _controlRows = [];

        foreach (int driver in new[] { 11, 22, 33 })
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "OpenF1", $"laps-{driver}.json");
            _sources[driver] = JsonSerializer.Deserialize<List<LapListDto>>(File.ReadAllText(path))!;
        }

        Session session = new()
        {
            SessionKey = 2200,
            MeetingKey = 1100,
            DateStart = new DateTimeOffset(2024, 7, 7, 14, 0, 0, TimeSpan.Zero),
            DateEnd = new DateTimeOffset(2024, 7, 7, 16, 0, 0, TimeSpan.Zero)
        };
        List<Driver> participants = new[] { 11, 22, 33 }
            .Select(driver => new Driver { SessionKey = 2200, MeetingKey = 1100, DriverNumber = driver }).ToList();
        _unit.Setup(unit => unit.SessionDao.GetSessionsByKeys(It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success(new List<Session> { session }));
        _unit.Setup(unit => unit.DriverDao.GetAllDriversSession(2200, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success(participants));
        _unit.Setup(unit => unit.LapDao.GetSessionLaps(2200, It.IsAny<int[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int key, int[] drivers, CancellationToken token) => Success(_rows.Where(lap => drivers.Contains(lap.DriverNumber)).ToList()));
        // Record every write, without deduplicating in the fake: repeated writes remain observable.
        _unit.Setup(unit => unit.LapDao.UpsertLap(It.IsAny<Lap>(), It.IsAny<CancellationToken>()))
            .Callback((Lap lap, CancellationToken token) => _rows.Add(lap))
            .ReturnsAsync(new Response { HasSuccess = true });
        _unit.Setup(unit => unit.GetImportWindow(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string dataset, string key, CancellationToken token) => _windows.GetValueOrDefault((dataset, key)));
        _unit.Setup(unit => unit.StageImportWindow(It.IsAny<ImportWindow>(), It.IsAny<CancellationToken>()))
            .Callback((ImportWindow window, CancellationToken token) => _windows[(window.Dataset, window.WindowKey)] = window)
            .Returns(Task.CompletedTask);
        _unit.Setup(unit => unit.BeginImport(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_transaction.Object);
        _unit.Setup(unit => unit.Commit(It.IsAny<CancellationToken>())).ReturnsAsync(new Response { HasSuccess = true });
        _laps.Setup(client => client.GetAllLapsSessionByDriver(2200, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int key, int driver, CancellationToken token) => Success(_sources[driver]));
        _stints.Setup(service => service.GetStintsBySessionKey(2200, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => Success(_stintRows));
        _pits.Setup(service => service.GetPitsBySessionKeyApi(2200, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => Success(_pitRows));
        _controls.Setup(service => service.GetRaceControlsBySessionFlagsApi(2200, It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => Success(_controlRows));
        _service = CreateService();
    }

    [Test]
    public async Task Catalog_ThreeIndependentLapCountsAndSuggestions_SurviveAServiceRestart()
    {
        SingleResponse<LapCatalogDto> imported = await Catalog(import: true);
        Assert.That(imported.HasSuccess, Is.True, imported.Message);
        ClearSourceCalls();
        _service = CreateService();

        SingleResponse<LapCatalogDto> reopened = await Catalog();

        Assert.That(reopened.HasSuccess, Is.True, reopened.Message);
        Assert.Multiple(() =>
        {
            Assert.That(reopened.Item.Drivers.Select(driver => driver.DriverNumber), Is.EqualTo(new[] { 11, 22, 33 }));
            Assert.That(reopened.Item.Drivers.Select(driver => driver.TotalLaps), Is.EqualTo(new[] { 3, 4, 2 }));
            Assert.That(reopened.Item.Drivers.Select(driver => driver.SuggestedLapNumber), Is.EqualTo(new int?[] { 2, 2, 1 }));
            Assert.That(reopened.Item.Drivers.SelectMany(driver => driver.Laps).Count(), Is.EqualTo(9));
            Assert.That(reopened.Item.ComparisonAvailable, Is.False);
        });
        AssertNoSourceCalls();
    }

    [Test]
    public async Task Filters_IntersectBeforeSuggestion_AndKeepUnfilteredTotals()
    {
        await Catalog(import: true);

        SingleResponse<LapCatalogDto> response = await Catalog(lap: 3, compound: " medium ", alerts: true);

        Assert.That(response.HasSuccess, Is.True, response.Message);
        DriverLapCatalogDto driver = response.Item.Drivers[0];
        Assert.Multiple(() =>
        {
            Assert.That(driver.Laps.Select(lap => lap.LapNumber), Is.EqualTo(new[] { 3 }));
            Assert.That(driver.TotalLaps, Is.EqualTo(3));
            Assert.That(driver.SuggestedLapNumber, Is.Null);
            Assert.That(driver.NoSuggestionReason, Is.Not.Empty);
            Assert.That(driver.ManualSelectionAllowed, Is.True);
            Assert.That(response.Item.Drivers.Skip(1).SelectMany(item => item.Laps), Is.Empty);
        });
    }

    [Test]
    public async Task Suggestion_TiedDurations_SelectsLowestLapNumberRegardlessOfSourceOrder()
    {
        _sources[22][3].LapDuration = 89.75;
        _sources[22].Reverse();

        SingleResponse<LapCatalogDto> response = await Catalog(import: true);

        Assert.That(response.HasSuccess, Is.True, response.Message);
        Assert.That(response.Item.Drivers[1].SuggestedLapNumber, Is.EqualTo(2));
        Assert.That(response.Item.Drivers[1].Laps.Select(lap => lap.LapNumber), Is.EqualTo(new[] { 1, 2, 3, 4 }));
    }

    [Test]
    public async Task Duration_NullAndZeroRemainDistinctAndNeitherIsSuggested()
    {
        await Catalog(import: true);

        SingleResponse<LapCatalogDto> response = await Catalog(lap: 1, alerts: false);

        CatalogLapDto zero = response.Item.Drivers[1].Laps.Single();
        Assert.Multiple(() =>
        {
            Assert.That(zero.Duration, Is.Zero);
            Assert.That(zero.SuggestedEligible, Is.False);
            Assert.That(zero.UnavailableData, Does.Not.Contain("lap_duration"));
            Assert.That(response.Item.Drivers[1].SuggestedLapNumber, Is.Null);
        });
        SingleResponse<LapCatalogDto> missing = await Catalog(lap: 3);
        CatalogLapDto unknown = missing.Item.Drivers[1].Laps.Single();
        Assert.That(unknown.Duration, Is.Null);
        Assert.That(unknown.UnavailableData, Does.Contain("lap_duration"));
        Assert.That(unknown.SuggestedEligible, Is.False);
    }

    [Test]
    public async Task Alerts_PitInPitOutAndExplicitDeletionStayOnTheirOwnDriverAndLap()
    {
        _controlRows.Add(Control(22, 4, "CAR 22 LAP 2 TIME DELETED"));

        SingleResponse<LapCatalogDto> response = await Catalog(import: true);

        Assert.That(response.HasSuccess, Is.True, response.Message);
        CatalogLapDto deleted = response.Item.Drivers[1].Laps[1];
        Assert.Multiple(() =>
        {
            Assert.That(response.Item.Drivers[0].Laps[0].Alerts.Select(alert => alert.Code), Is.EqualTo(new[] { "pit_out" }));
            Assert.That(response.Item.Drivers[0].Laps[2].Alerts.Select(alert => alert.Code), Is.EqualTo(new[] { "pit_in" }));
            Assert.That(deleted.Alerts.Select(alert => alert.Code), Does.Contain("time_deleted"));
            Assert.That(deleted.SuggestedEligible, Is.False);
            Assert.That(response.Item.Drivers[1].SuggestedLapNumber, Is.EqualTo(4));
            Assert.That(response.Item.Drivers[1].Laps[3].Alerts, Is.Empty);
            Assert.That(response.Item.Drivers[0].Laps[1].Alerts, Is.Empty);
            Assert.That(response.Item.Drivers[2].Laps[1].Alerts.Select(alert => alert.Code), Is.EqualTo(new[] { "pit_out" }));
        });
    }

    [Test]
    public async Task Deletion_WithoutExplicitLap_DoesNotUseRaceCurrentLapAsDeletedLap()
    {
        _controlRows.Add(Control(22, 2, "LAP TIME DELETED - TRACK LIMITS"));

        SingleResponse<LapCatalogDto> response = await Catalog(import: true);

        CatalogLapDto lap = response.Item.Drivers[1].Laps[1];
        Assert.That(lap.Alerts, Is.Empty);
        Assert.That(lap.UnavailableData, Does.Contain("time_deletion_lap_not_identified"));
        Assert.That(response.Item.Drivers[1].SuggestedLapNumber, Is.EqualTo(2));
    }

    [Test]
    public async Task UnavailableAlertSources_ArePreservedWithoutInventingAnAlertOrOfficialValidity()
    {
        _pits.Setup(service => service.GetPitsBySessionKeyApi(2200, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("fixture source unavailable"));
        _controls.Setup(service => service.GetRaceControlsBySessionFlagsApi(2200, It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DataResponse<RaceControl> { HasSuccess = false });
        await Catalog(import: true);
        ClearSourceCalls();

        SingleResponse<LapCatalogDto> response = await Catalog(lap: 2, alerts: false);

        CatalogLapDto lap = response.Item.Drivers[0].Laps.Single();
        Assert.Multiple(() =>
        {
            Assert.That(lap.Alerts, Is.Empty);
            Assert.That(lap.UnavailableData, Does.Contain("pit").And.Contain("race_control").And.Contain("official_lap_validity_not_provided"));
            Assert.That(lap.SuggestedEligible, Is.True);
        });
        AssertNoSourceCalls();
    }

    [Test]
    public async Task PersistedCatalogsAndHistoricalEnrichment_AreUsedBeforeEveryExternalSourceOnImport()
    {
        SeedCatalogs();
        SeedWindow("stints", "2200", _stintRows);
        SeedWindow("pits", "2200", _pitRows);
        const string flagsHash = "4F53CDA18C2BAA0C0354BB5F9A3ECBE5ED12AB4D8E11BA873C2F11161202B945";
        SeedWindow("race_control", "2200:" + flagsHash, _controlRows);

        SingleResponse<LapCatalogDto> response = await Catalog(import: true);

        Assert.That(response.HasSuccess, Is.True, response.Message);
        Assert.That(response.Item.Drivers[0].Laps[1].Compound, Is.EqualTo("MEDIUM"));
        AssertNoSourceCalls();
        _unit.Verify(unit => unit.LapDao.UpsertLap(It.IsAny<Lap>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Reimport_CompletedCatalogsDoNotCallSourceOrWriteAnyLapAgain()
    {
        await Catalog(import: true);
        ClearSourceCalls();
        Mock.Get(_unit.Object.LapDao).Invocations.Clear();

        SingleResponse<LapCatalogDto> response = await Catalog(import: true);

        Assert.That(response.HasSuccess, Is.True, response.Message);
        AssertNoSourceCalls();
        Assert.That(_rows, Has.Count.EqualTo(9));
        _unit.Verify(unit => unit.LapDao.UpsertLap(It.IsAny<Lap>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task PartialCatalog_ReopensWithoutFetchingAndRetriesOnlyItsDriverOnImport()
    {
        _sources[11].RemoveAt(1);
        await Catalog(import: true);
        ClearSourceCalls();

        SingleResponse<LapCatalogDto> reopened = await Catalog();

        Assert.That(reopened.HasSuccess, Is.True, reopened.Message);
        Assert.That(reopened.Item.Drivers[0].ImportComplete, Is.False);
        Assert.That(reopened.Item.Drivers[0].IncompleteReason, Does.Contain("gaps"));
        AssertNoSourceCalls();
        await Catalog(import: true);
        _laps.Verify(client => client.GetAllLapsSessionByDriver(2200, 11, It.IsAny<CancellationToken>()), Times.Once);
        _laps.Verify(client => client.GetAllLapsSessionByDriver(2200, 22, It.IsAny<CancellationToken>()), Times.Never);
        _laps.Verify(client => client.GetAllLapsSessionByDriver(2200, 33, It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestCase("{")]
    [TestCase("null")]
    public async Task CorruptCatalog_ReadReturnsConflictWithoutFetchingSource(string payload)
    {
        SeedCatalogs();
        _windows[("lap_catalog_v2", "2200:11")].Payload = payload;

        SingleResponse<LapCatalogDto> response = await Catalog();

        AssertFailure(response, 409);
        AssertNoSourceCalls();
    }

    [TestCase("session")]
    [TestCase("drivers")]
    [TestCase("laps")]
    public async Task PersistedDataFailure_StopsBeforeOpenF1(string dataset)
    {
        if (dataset == "session")
        {
            _unit.Setup(unit => unit.SessionDao.GetSessionsByKeys(It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DataResponse<Session> { HasSuccess = false });
        }
        else if (dataset == "drivers")
        {
            _unit.Setup(unit => unit.DriverDao.GetAllDriversSession(2200, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DataResponse<Driver> { HasSuccess = false });
        }
        else
        {
            _unit.Setup(unit => unit.LapDao.GetSessionLaps(2200, It.IsAny<int[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DataResponse<Lap> { HasSuccess = false });
        }

        SingleResponse<LapCatalogDto> response = await Catalog(import: true);

        AssertFailure(response, 503);
        AssertNoSourceCalls();
    }

    [TestCase("session")]
    [TestCase("meeting")]
    [TestCase("driver")]
    [TestCase("lap")]
    public async Task SourceWrongIdentity_IsRejectedBeforeAnyWrite(string field)
    {
        LapListDto lap = _sources[11][0];
        switch (field)
        {
            case "session":
                lap.SessionKey = 999;
                break;
            case "meeting":
                lap.MeetingKey = 999;
                break;
            case "driver":
                lap.DriverNumber = 99;
                break;
            case "lap":
                lap.LapNumber = 0;
                break;
        }

        SingleResponse<LapCatalogDto> response = await Catalog(import: true);

        AssertFailure(response, 502);
        Assert.That(_rows, Is.Empty);
        Assert.That(_windows, Is.Empty);
        _transaction.Verify(transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task SourceFailure_OrNullSuccessfulPayload_NeverBecomesACompletedEmptyCatalog(bool hasSuccess)
    {
        _laps.Setup(client => client.GetAllLapsSessionByDriver(2200, 11, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DataResponse<LapListDto> { HasSuccess = hasSuccess, Itens = null! });

        SingleResponse<LapCatalogDto> response = await Catalog(import: true);

        AssertFailure(response, 503);
        Assert.That(_rows, Is.Empty);
        Assert.That(_windows, Is.Empty);
        _transaction.Verify(transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void SourceCancellation_IsPropagatedAndDoesNotCommitTheImport()
    {
        using CancellationTokenSource cancellation = new();
        _laps.Setup(client => client.GetAllLapsSessionByDriver(2200, 11, cancellation.Token))
            .Returns(() =>
            {
                cancellation.Cancel();
                return Task.FromCanceled<DataResponse<LapListDto>>(cancellation.Token);
            });

        Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await _service.GetCatalog(2200, [11, 22, 33], null, null, null, true, cancellation.Token));

        Assert.That(_windows, Is.Empty);
        _transaction.Verify(transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ManualPreviewAndConfirmation_KeepZeroPitOutAndDeletedTimeAcrossSerializedReloads()
    {
        _controlRows.Add(Control(11, 3, "CAR 11 LAP 2 TIME DELETED"));
        await Catalog(import: true);
        LapSelectionRequest request = Request((11, 2), (22, 1), (33, 2));
        SingleResponse<LapSelectionDto> preview = await _service.Preview(2200, request, default);
        Assert.That(preview.HasSuccess, Is.True, preview.Message);
        Guid identifier = preview.Item.PreviewId;
        string expectedLaps = JsonSerializer.Serialize(preview.Item.Selections);
        Assert.That(_windows.ContainsKey(("lap_selection", "2200")), Is.False);
        preview.Item.Selections.Clear();
        ClearSourceCalls();
        _service = CreateService();

        SingleResponse<LapSelectionDto> confirmed = await _service.Confirm(2200, identifier, default);
        _service = CreateService();
        SingleResponse<LapSelectionDto> reopened = await _service.GetSelection(2200, null, default);
        SingleResponse<LapSelectionDto> savedPreview = await _service.GetSelection(2200, identifier, default);

        Assert.That(confirmed.HasSuccess, Is.True, confirmed.Message);
        Assert.That(reopened.HasSuccess, Is.True, reopened.Message);
        Assert.Multiple(() =>
        {
            Assert.That(JsonSerializer.Serialize(reopened.Item.Selections), Is.EqualTo(expectedLaps));
            Assert.That(savedPreview.Item.Confirmed, Is.True);
            Assert.That(reopened.Item.PreviewId, Is.EqualTo(identifier));
            Assert.That(reopened.Item.Selections[0].Alerts.Select(alert => alert.Code), Does.Contain("time_deleted"));
            Assert.That(reopened.Item.Selections[1].Duration, Is.Zero);
            Assert.That(reopened.Item.Selections[2].Alerts.Select(alert => alert.Code), Does.Contain("pit_out"));
            Assert.That(reopened.Item.ComparisonAvailable, Is.False);
        });
        AssertNoSourceCalls();
    }

    [Test]
    public async Task Preview_NullDurationRemainsManuallySelectable_AndDoesNotReplaceConfirmedChoice()
    {
        await Catalog(import: true);
        SingleResponse<LapSelectionDto> first = await _service.Preview(2200, Request((11, 2), (22, 2), (33, 1)), default);
        await _service.Confirm(2200, first.Item.PreviewId, default);

        SingleResponse<LapSelectionDto> second = await _service.Preview(2200, Request((11, 1), (22, 3), (33, 2)), default);
        SingleResponse<LapSelectionDto> latest = await _service.GetSelection(2200, null, default);

        Assert.That(second.HasSuccess, Is.True, second.Message);
        Assert.That(second.Item.Confirmed, Is.False);
        Assert.That(second.Item.Selections[0].Duration, Is.Null);
        Assert.That(second.Item.Selections[1].Duration, Is.Null);
        Assert.That(latest.Item.PreviewId, Is.EqualTo(first.Item.PreviewId));
    }

    [Test]
    public async Task Confirm_RepeatingTheSamePreviewIsIdempotent_AndUsesTheSessionLock()
    {
        await Catalog(import: true);
        SingleResponse<LapSelectionDto> preview = await _service.Preview(2200, Request((11, 2), (22, 2), (33, 1)), default);
        await _service.Confirm(2200, preview.Item.PreviewId, default);

        SingleResponse<LapSelectionDto> again = await _service.Confirm(2200, preview.Item.PreviewId, default);

        Assert.That(again.HasSuccess, Is.True, again.Message);
        Assert.That(_windows.Keys.Count(key => key.Dataset == "lap_selection"), Is.EqualTo(1));
        _unit.Verify(unit => unit.BeginImport("lap_selection", "2200", It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Test]
    public async Task Confirm_CommitFailureNeverReportsSuccessOrCommitsTransaction()
    {
        await Catalog(import: true);
        SingleResponse<LapSelectionDto> preview = await _service.Preview(2200, Request((11, 2), (22, 2), (33, 1)), default);
        _transaction.Invocations.Clear();
        _unit.Setup(unit => unit.Commit(It.IsAny<CancellationToken>())).ReturnsAsync(new Response { HasSuccess = false });

        SingleResponse<LapSelectionDto> response = await _service.Confirm(2200, preview.Item.PreviewId, default);

        AssertFailure(response, 503);
        _transaction.Verify(transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        _transaction.Verify(transaction => transaction.DisposeAsync(), Times.Once);
    }

    [Test]
    public async Task Confirm_CannotUseAPreviewFromAnotherSession()
    {
        await Catalog(import: true);
        SingleResponse<LapSelectionDto> preview = await _service.Preview(2200, Request((11, 2), (22, 2), (33, 1)), default);

        SingleResponse<LapSelectionDto> response = await _service.Confirm(2201, preview.Item.PreviewId, default);

        AssertFailure(response, 404);
        Assert.That(_windows.ContainsKey(("lap_selection", "2201")), Is.False);
    }

    [Test]
    public async Task SourceFailure_PreservesExistingLapsAndDoesNotPublishAReplacementSnapshot()
    {
        SeedCatalogs();
        _windows.Remove(("lap_catalog_v2", "2200:11"));
        _laps.Setup(client => client.GetAllLapsSessionByDriver(2200, 11, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DataResponse<LapListDto> { HasSuccess = false, Message = "429" });
        List<Lap> existingRows = _rows.ToList();

        SingleResponse<LapCatalogDto> response = await Catalog(import: true);

        AssertFailure(response, 503);
        Assert.That(_rows, Is.EqualTo(existingRows));
        Assert.That(_windows.ContainsKey(("lap_catalog_v2", "2200:11")), Is.False);
        _unit.Verify(unit => unit.LapDao.UpsertLap(It.IsAny<Lap>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Confirm_ReadsAndStagesPreviewAndLatestSelectionInsideTheSessionTransaction()
    {
        Guid previewId = Guid.Parse("806016b9-e597-4402-80cf-b8c271d8e7cd");
        string previewKey = "2200:" + previewId.ToString("N");
        LapSelectionDto preview = new()
        {
            SessionKey = 2200,
            PreviewId = previewId,
            Selections =
            [
                new() { DriverNumber = 11, LapNumber = 2 },
                new() { DriverNumber = 22, LapNumber = 2 },
                new() { DriverNumber = 33, LapNumber = 1 }
            ]
        };
        MockSequence sequence = new();
        Mock<IUnityOfWork> unit = new(MockBehavior.Strict);
        Mock<IImportTransaction> transaction = new(MockBehavior.Strict);
        unit.InSequence(sequence).Setup(work => work.BeginImport("lap_selection", "2200", It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction.Object);
        unit.InSequence(sequence).Setup(work => work.GetImportWindow("lap_preview", previewKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ImportWindow { IsComplete = true, Payload = JsonSerializer.Serialize(preview) });
        unit.InSequence(sequence).Setup(work => work.StageImportWindow(
                It.Is<ImportWindow>(window => window.Dataset == "lap_preview" && window.WindowKey == previewKey), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        unit.InSequence(sequence).Setup(work => work.StageImportWindow(
                It.Is<ImportWindow>(window => window.Dataset == "lap_selection" && window.WindowKey == "2200"), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        unit.InSequence(sequence).Setup(work => work.Commit(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Response { HasSuccess = true });
        transaction.InSequence(sequence).Setup(scope => scope.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        transaction.InSequence(sequence).Setup(scope => scope.DisposeAsync()).Returns(ValueTask.CompletedTask);
        LapCatalogService service = new(unit.Object, _laps.Object, _stints.Object, _pits.Object, _controls.Object);

        SingleResponse<LapSelectionDto> response = await service.Confirm(2200, previewId, default);

        Assert.That(response.HasSuccess, Is.True, response.Message);
        Assert.That(response.Item.Confirmed, Is.True);
        unit.VerifyAll();
        transaction.VerifyAll();
        AssertNoSourceCalls();
    }

    private LapCatalogService CreateService() => new(_unit.Object, _laps.Object, _stints.Object, _pits.Object, _controls.Object);

    private Task<SingleResponse<LapCatalogDto>> Catalog(bool import = false, int? lap = null, string? compound = null, bool? alerts = null)
    {
        return _service.GetCatalog(2200, [11, 22, 33], lap, compound, alerts, import, default);
    }

    private void SeedCatalogs()
    {
        foreach (KeyValuePair<int, List<LapListDto>> driver in _sources)
        {
            JsonObject snapshot = new()
            {
                ["Version"] = 1,
                ["SessionKey"] = 2200,
                ["DriverNumber"] = driver.Key,
                ["Complete"] = true,
                ["Laps"] = JsonSerializer.SerializeToNode(driver.Value)
            };
            SeedWindow("lap_catalog_v2", $"2200:{driver.Key}", snapshot);
            foreach (LapListDto lap in driver.Value)
            {
                _rows.Add(new Lap { SessionKey = 2200, MeetingKey = 1100, DriverNumber = driver.Key, LapNumber = lap.LapNumber });
            }
        }
    }

    private void SeedWindow<T>(string dataset, string key, T payload)
    {
        _windows[(dataset, key)] = new ImportWindow
        {
            Dataset = dataset,
            WindowKey = key,
            IsComplete = true,
            CompletedAtUtc = new DateTimeOffset(2024, 7, 8, 0, 0, 0, TimeSpan.Zero),
            ExpiresAtUtc = new DateTimeOffset(2024, 7, 8, 0, 5, 0, TimeSpan.Zero),
            Payload = JsonSerializer.Serialize(payload)
        };
    }

    private static LapSelectionRequest Request(params (int Driver, int Lap)[] choices)
    {
        return new LapSelectionRequest
        {
            Selections = choices.Select(choice => (LapSelectionItem?)new LapSelectionItem
            {
                DriverNumber = choice.Driver,
                LapNumber = choice.Lap
            }).ToList()
        };
    }

    private static RaceControl Control(int driver, int currentLap, string message)
    {
        return new RaceControl
        {
            SessionKey = 2200, MeetingKey = 1100, DriverNumber = driver,
            LapNumber = currentLap, Message = message,
            Date = new DateTime(2024, 7, 7, 14, 8, 0, DateTimeKind.Utc)
        };
    }

    private static DataResponse<T> Success<T>(List<T> items) => new() { HasSuccess = true, Itens = items };

    private static void AssertFailure<T>(SingleResponse<T> response, int status)
    {
        Assert.That(response.HasSuccess, Is.False);
        Assert.That(response.Exception, Is.TypeOf<LapCatalogException>(), response.Message);
        Assert.That(((LapCatalogException)response.Exception).StatusCode, Is.EqualTo(status));
    }

    private void ClearSourceCalls()
    {
        _laps.Invocations.Clear();
        _stints.Invocations.Clear();
        _pits.Invocations.Clear();
        _controls.Invocations.Clear();
    }

    private void AssertNoSourceCalls()
    {
        Assert.That(_laps.Invocations, Is.Empty);
        Assert.That(_stints.Invocations, Is.Empty);
        Assert.That(_pits.Invocations, Is.Empty);
        Assert.That(_controls.Invocations, Is.Empty);
    }
}
