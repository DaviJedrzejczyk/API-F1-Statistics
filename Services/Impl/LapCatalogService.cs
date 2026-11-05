using Dao.Interface;
using Entities.Class;
using ExternalApi.Interfaces;
using Services.Interfaces;
using Services.Models;
using Shared.Common.Atrributes;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Services.Impl;

[IncludeDependencyInjection]
public sealed class LapCatalogService : ILapCatalogService
{
    private readonly IUnityOfWork _unit;
    private readonly ILapClient _laps;
    private readonly IStintService _stints;
    private readonly IPitService _pits;
    private readonly IRaceControlService _controls;

    public LapCatalogService(IUnityOfWork unit, ILapClient laps, IStintService stints,
        IPitService pits, IRaceControlService controls)
    {
        _unit = unit;
        _laps = laps;
        _stints = stints;
        _pits = pits;
        _controls = controls;
    }

    public async Task<LapCatalogResponse> Get(int sessionKey, int[] drivers, int? lapNumber,
        string? compound, bool? hasAlerts, bool import, CancellationToken cancellationToken)
    {
        if (sessionKey <= 0 || drivers == null || drivers.Length != 3 ||
            drivers.Any(x => x <= 0) || drivers.Distinct().Count() != 3 || lapNumber is <= 0)
            throw new LapCatalogException(400, "A session and exactly three distinct positive driver numbers are required.");
        var sessionResult = await _unit.SessionDao.GetSessionsByKeys([sessionKey], cancellationToken);
        if (!sessionResult.HasSuccess) throw new LapCatalogException(503, "Session lookup is unavailable.");
        var session = sessionResult.Itens?.SingleOrDefault(x => x.SessionKey == sessionKey);
        if (session == null) throw new LapCatalogException(404, "Session not found in the database.");
        if (session.DateEnd > DateTimeOffset.UtcNow || session.IsCancelled)
            throw new LapCatalogException(400, "A completed historical session is required.");

        var key = sessionKey.ToString(CultureInfo.InvariantCulture) + ":" +
            string.Join(",", drivers.OrderBy(x => x));
        var marker = await _unit.GetImportWindow("lap_catalog", key, cancellationToken);
        if (import && (marker == null || !marker.IsComplete))
        {
            var incoming = new List<Lap>();
            foreach (var driver in drivers)
            {
                var response = await _laps.GetAllLapsSessionByDriver(sessionKey, driver, cancellationToken);
                if (!response.HasSuccess || response.Itens == null)
                    throw new LapCatalogException(503, "Lap data is unavailable from OpenF1.");
                if (response.Itens.Any(x => x.SessionKey != sessionKey || x.MeetingKey != session.MeetingKey ||
                    x.DriverNumber != driver || x.LapNumber <= 0) ||
                    response.Itens.Select(x => x.LapNumber).Distinct().Count() != response.Itens.Count)
                    throw new LapCatalogException(502, "Invalid lap window returned by OpenF1.");
                incoming.AddRange(response.Itens.Select(x => new Lap
                {
                    DateStart = x.DateStart, DriverNumber = x.DriverNumber,
                    DurationSector1 = x.DurationSector1, DurationSector2 = x.DurationSector2,
                    DurationSector3 = x.DurationSector3, I1Speed = x.I1Speed, I2Speed = x.I2Speed,
                    IsPitOutLap = x.IsPitOutLap, LapDuration = x.LapDuration,
                    LapNumber = x.LapNumber, MeetingKey = x.MeetingKey,
                    SessionKey = x.SessionKey, StSpeed = x.StSpeed
                }));
            }
            // These enrichment imports use the existing persistent cache and retry policy.
            var unavailable = new List<string>();
            try { var r = await _stints.GetStintsBySessionKey(sessionKey, cancellationToken); if (!r.HasSuccess) unavailable.Add("stints"); }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch { unavailable.Add("stints"); }
            try { var r = await _pits.GetPitsBySessionKeyApi(sessionKey, cancellationToken); if (!r.HasSuccess) unavailable.Add("pits"); }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch { unavailable.Add("pits"); }
            try { var r = await _controls.GetRaceControlsBySessionFlagsApi(sessionKey, [], cancellationToken); if (!r.HasSuccess) unavailable.Add("race_control"); }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch { unavailable.Add("race_control"); }

            await using var transaction = await _unit.BeginImport("lap_catalog", key, cancellationToken);
            marker = await _unit.GetImportWindow("lap_catalog", key, cancellationToken);
            if (marker == null || !marker.IsComplete)
            {
                foreach (var lap in incoming)
                {
                    var saved = await _unit.LapDao.UpsertLap(lap, cancellationToken);
                    if (!saved.HasSuccess) throw new LapCatalogException(503, "Unable to store the lap catalog.");
                }
                await ImportWindowCache.Stage(_unit, "lap_catalog", key, unavailable, cancellationToken);
                var committed = await _unit.Commit(cancellationToken);
                if (!committed.HasSuccess) throw new LapCatalogException(503, "Unable to store the lap catalog.");
                await transaction.CommitAsync(cancellationToken);
            }
        }
        else if (marker == null || !marker.IsComplete)
            throw new LapCatalogException(404, "Lap catalog has not been imported for this session.");

        marker = await _unit.GetImportWindow("lap_catalog", key, cancellationToken);
        var unavailableSources = marker == null ? [] : JsonSerializer.Deserialize<List<string>>(marker.Payload) ?? [];
        var lapResult = await _unit.LapDao.GetSessionLaps(sessionKey, drivers, cancellationToken);
        var stintResult = await _unit.StintDao.GetStintsBySessionKey(sessionKey, cancellationToken);
        var pitResult = await _unit.PitDao.GetAllPitsBySessionKey(sessionKey, cancellationToken);
        var controlResult = await _unit.RaceControlDao.GetRaceControlsBySession(sessionKey, cancellationToken);
        if (!lapResult.HasSuccess || lapResult.Itens == null)
            throw new LapCatalogException(503, "Stored laps are unavailable.");
        if (!stintResult.HasSuccess) unavailableSources.Add("stints");
        if (!pitResult.HasSuccess) unavailableSources.Add("pits");
        if (!controlResult.HasSuccess) unavailableSources.Add("race_control");
        var all = lapResult.Itens.Select(lap => Describe(lap, stintResult.Itens ?? [],
            pitResult.Itens ?? [], controlResult.Itens ?? [], unavailableSources)).ToList();
        var filtered = all.Where(x => (lapNumber == null || x.LapNumber == lapNumber) &&
            (compound == null || string.Equals(x.Compound, compound, StringComparison.OrdinalIgnoreCase)) &&
            (hasAlerts == null || (x.Alerts.Count > 0) == hasAlerts)).ToList();
        var result = drivers.Select(driver =>
        {
            var laps = filtered.Where(x => x.DriverNumber == driver).OrderBy(x => x.LapNumber).ToList();
            var fastest = laps.Where(x => x.SuggestedEligible).OrderBy(x => x.Duration).ThenBy(x => x.LapNumber).FirstOrDefault();
            return new DriverLapCatalog(driver, laps, fastest?.LapNumber,
                fastest == null ? "No positive duration without an identified pit in/out or time deletion among filtered laps." : null,
                true);
        }).ToList();
        return new LapCatalogResponse(sessionKey, session.MeetingKey, result, false,
            "Telemetry comparison is unavailable in this release.");
    }

    private static CatalogLap Describe(Lap lap, List<Stint> stints, List<Pit> pits,
        List<RaceControl> controls, List<string> unavailableSources)
    {
        var matching = stints.Where(x => x.DriverNumber == lap.DriverNumber && x.SessionKey == lap.SessionKey &&
            x.MeetingKey == lap.MeetingKey && x.LapStart <= lap.LapNumber &&
            (x.LapEnd == null || x.LapEnd >= lap.LapNumber)).ToList();
        var compound = matching.Count == 1 && !string.IsNullOrWhiteSpace(matching[0].Compound)
            ? matching[0].Compound : null;
        var alerts = new List<LapAlert>();
        if (lap.IsPitOutLap) alerts.Add(new("pit_out", "OpenF1 identifies this as a pit out lap."));
        if (pits.Any(x => x.MeetingKey == lap.MeetingKey && x.SessionKey == lap.SessionKey &&
            x.DriverNumber == lap.DriverNumber && x.LapNumber == lap.LapNumber))
            alerts.Add(new("pit_in", "Pit entry is recorded on this lap."));
        foreach (var control in controls.Where(x => x.MeetingKey == lap.MeetingKey &&
            x.SessionKey == lap.SessionKey && x.LapNumber == lap.LapNumber &&
            (x.DriverNumber == null || x.DriverNumber == lap.DriverNumber)))
        {
            var targeted = control.DriverNumber == lap.DriverNumber ||
                (control.DriverNumber == null && Regex.IsMatch(control.Message,
                    $@"\bCAR\s+{lap.DriverNumber}\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));
            var deleted = targeted &&
                control.Message.Contains("DELET", StringComparison.OrdinalIgnoreCase) &&
                control.Message.Contains("TIME", StringComparison.OrdinalIgnoreCase);
            alerts.Add(new(deleted ? "time_deleted" : "race_control", control.Message));
        }
        var unavailable = unavailableSources.Distinct().ToList();
        if (compound == null && !unavailable.Contains("stints")) unavailable.Add("compound_not_identified");
        if (controls.Any(x => x.LapNumber <= 0) && !unavailable.Contains("race_control"))
            unavailable.Add("some_race_control_events_lack_lap_number");
        unavailable.Add("official_lap_validity_not_provided");
        var eligible = lap.LapDuration is > 0 && !alerts.Any(x =>
            x.Code is "pit_out" or "pit_in" or "time_deleted");
        return new CatalogLap(lap.DriverNumber, lap.LapNumber, lap.LapDuration, compound,
            lap.IsPitOutLap, alerts, unavailable, eligible);
    }

    public async Task<LapSelectionResponse> Select(int sessionKey, LapSelectionRequest request,
        bool confirm, CancellationToken cancellationToken)
    {
        if (request?.Selections == null || request.Selections.Count != 3 ||
            request.Selections.Any(x => x is null || x.DriverNumber <= 0 || x.LapNumber <= 0) ||
            request.Selections.Select(x => x.DriverNumber).Distinct().Count() != 3)
            throw new LapCatalogException(400, "Exactly three distinct driver and lap selections are required.");
        var drivers = request.Selections.Select(x => x.DriverNumber).ToArray();
        var catalog = await Get(sessionKey, drivers, null, null, null, false, cancellationToken);
        var selected = request.Selections.Select(x => catalog.Drivers.Single(d => d.DriverNumber == x.DriverNumber)
            .Laps.SingleOrDefault(lap => lap.LapNumber == x.LapNumber)
            ?? throw new LapCatalogException(400, $"Lap {x.LapNumber} is not cataloged for driver {x.DriverNumber}.")).ToList();
        if (confirm)
        {
            await _unit.StageImportWindow(new Dao.Cache.ImportWindow
            {
                Dataset = "lap_selection", WindowKey = sessionKey.ToString(CultureInfo.InvariantCulture),
                CompletedAtUtc = DateTimeOffset.UtcNow, ExpiresAtUtc = DateTimeOffset.UtcNow.AddYears(10),
                IsComplete = true, Payload = JsonSerializer.Serialize(request.Selections)
            }, cancellationToken);
            var committed = await _unit.Commit(cancellationToken);
            if (!committed.HasSuccess) throw new LapCatalogException(503, "Unable to save the selection.");
        }
        return new LapSelectionResponse(sessionKey, selected, confirm, false,
            "Telemetry comparison is unavailable in this release.");
    }

    public async Task<LapSelectionResponse?> GetConfirmed(int sessionKey, CancellationToken cancellationToken)
    {
        var marker = await _unit.GetImportWindow("lap_selection", sessionKey.ToString(CultureInfo.InvariantCulture), cancellationToken);
        if (marker == null || !marker.IsComplete) return null;
        var selections = JsonSerializer.Deserialize<List<LapSelectionItem>>(marker.Payload);
        if (selections == null) return null;
        return await Select(sessionKey, new LapSelectionRequest(selections), false, cancellationToken) is { } response
            ? response with { Confirmed = true } : null;
    }
}
