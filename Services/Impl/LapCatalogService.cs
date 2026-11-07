using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Dao.Cache;
using Dao.Interface;
using Entities.Class;
using Entities.Dtos;
using ExternalApi.Interfaces;
using Services.Interfaces;
using Services.Models;
using Shared.Common.Atrributes;
using Shared.Responses;

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

    public Task<SingleResponse<LapCatalogDto>> GetCatalog(int sessionKey, int[] drivers, int? lapNumber,
        string? compound, bool? hasAlerts, bool import, CancellationToken cancellationToken)
    {
        return Execute(() => ReadCatalog(sessionKey, drivers, lapNumber, compound, hasAlerts, import, cancellationToken), cancellationToken);
    }

    private async Task<LapCatalogDto> ReadCatalog(int sessionKey, int[] drivers, int? lapNumber,
        string? compound, bool? hasAlerts, bool import, CancellationToken cancellationToken)
    {
        if (sessionKey <= 0 || drivers == null || drivers.Length != 3 ||
            drivers.Any(driver => driver <= 0) || drivers.Distinct().Count() != 3 || lapNumber is <= 0)
        {
            throw new LapCatalogException(400, "A session and exactly three distinct positive driver numbers are required; lap numbers must be positive.");
        }

        Session session = await ValidateSession(sessionKey, drivers, cancellationToken);
        List<Lap> stored = await ReadStoredLaps(sessionKey, drivers, cancellationToken);
        List<LapCatalogSnapshot> snapshots = [];
        foreach (int driver in drivers)
        {
            LapCatalogSnapshot? snapshot = await ReadSnapshot<LapCatalogSnapshot>("lap_catalog_v2", DriverKey(sessionKey, driver), cancellationToken);
            bool intact = IsIntact(snapshot, session, driver, stored);
            if (import && (!intact || snapshot?.Complete != true))
            {
                snapshot = await ImportDriver(session, driver, cancellationToken);
            }
            else if (!intact)
            {
                throw new LapCatalogException(409, "The persisted lap catalog is absent or incomplete; import this catalog first.");
            }
            snapshots.Add(snapshot!);
        }

        LapCatalogEnrichment enrichment = await ReadEnrichment(session, import, cancellationToken);
        LapCatalogDto catalog = new() { SessionKey = sessionKey, MeetingKey = session.MeetingKey };
        foreach (LapCatalogSnapshot snapshot in snapshots)
        {
            List<CatalogLapDto> described = snapshot.Laps.Select(lap => LapCatalogProjection.Describe(lap, enrichment)).ToList();
            List<CatalogLapDto> filtered = described.Where(lap =>
                (!lapNumber.HasValue || lap.LapNumber == lapNumber) &&
                (string.IsNullOrWhiteSpace(compound) || string.Equals(lap.Compound, compound.Trim(), StringComparison.OrdinalIgnoreCase)) &&
                (!hasAlerts.HasValue || (lap.Alerts.Count > 0) == hasAlerts.Value))
                .OrderBy(lap => lap.LapNumber).ToList();
            CatalogLapDto? suggested = filtered.Where(lap => lap.SuggestedEligible)
                .OrderBy(lap => lap.Duration).ThenBy(lap => lap.LapNumber).FirstOrDefault();

            catalog.Drivers.Add(new DriverLapCatalogDto
            {
                DriverNumber = snapshot.DriverNumber,
                ImportComplete = snapshot.Complete,
                IncompleteReason = snapshot.IncompleteReason,
                TotalLaps = described.Count,
                Laps = filtered,
                SuggestedLapNumber = suggested?.LapNumber,
                NoSuggestionReason = suggested == null
                    ? "No filtered lap has a positive duration without an identified pit in/out or time deletion; manual selection remains available."
                    : null
            });
        }
        return catalog;
    }

    private async Task<Session> ValidateSession(int sessionKey, int[] drivers, CancellationToken cancellationToken)
    {
        DataResponse<Session> sessions = await _unit.SessionDao.GetSessionsByKeys([sessionKey], cancellationToken);
        if (!sessions.HasSuccess || sessions.Itens == null)
        {
            throw new LapCatalogException(503, "Stored session data is unavailable.");
        }
        Session? session = sessions.Itens.SingleOrDefault(session => session.SessionKey == sessionKey);
        if (session == null)
        {
            throw new LapCatalogException(404, "Session not found; import the meeting's sessions first.");
        }
        if (session.IsCancelled || session.DateStart == default || session.DateEnd <= session.DateStart ||
            session.DateEnd > DateTimeOffset.UtcNow.AddMinutes(-30))
        {
            throw new LapCatalogException(400, "A historical session ending at least 30 minutes ago is required.");
        }

        DataResponse<Driver> participants = await _unit.DriverDao.GetAllDriversSession(sessionKey, cancellationToken);
        if (!participants.HasSuccess || participants.Itens == null)
        {
            throw new LapCatalogException(503, "Stored driver data is unavailable.");
        }
        if (drivers.Any(driver => !participants.Itens.Any(participant => participant.DriverNumber == driver &&
            participant.SessionKey == sessionKey && participant.MeetingKey == session.MeetingKey)))
        {
            throw new LapCatalogException(400, "Each driver must participate in the session; import session drivers before requesting their catalog.");
        }
        return session;
    }

    private async Task<List<Lap>> ReadStoredLaps(int sessionKey, int[] drivers, CancellationToken cancellationToken)
    {
        DataResponse<Lap> response = await _unit.LapDao.GetSessionLaps(sessionKey, drivers, cancellationToken);
        if (!response.HasSuccess || response.Itens == null)
        {
            throw new LapCatalogException(503, "Stored laps are unavailable.");
        }
        return response.Itens;
    }

    private static bool IsIntact(LapCatalogSnapshot? snapshot, Session session, int driver, List<Lap> stored)
    {
        if (snapshot == null || snapshot.Version != 1 || snapshot.SessionKey != session.SessionKey ||
            snapshot.DriverNumber != driver || snapshot.Laps == null || !ValidLaps(snapshot.Laps, session, driver))
        {
            return false;
        }

        List<Lap> driverLaps = stored.Where(lap => lap.DriverNumber == driver).ToList();
        HashSet<int> numbers = driverLaps.Select(lap => lap.LapNumber).ToHashSet();
        return driverLaps.All(lap => lap.MeetingKey == session.MeetingKey) &&
            driverLaps.Count == numbers.Count && numbers.SetEquals(snapshot.Laps.Select(lap => lap.LapNumber));
    }

    private static bool ValidLaps(List<LapListDto> laps, Session session, int driver)
    {
        return laps.All(lap => lap != null && lap.SessionKey == session.SessionKey && lap.MeetingKey == session.MeetingKey &&
            lap.DriverNumber == driver && lap.LapNumber > 0) &&
            laps.Select(lap => lap.LapNumber).Distinct().Count() == laps.Count;
    }

    private async Task<LapCatalogSnapshot> ImportDriver(Session session, int driver, CancellationToken cancellationToken)
    {
        // Share the existing fastest-lap writer's session lock, then check storage again.
        await using IImportTransaction transaction = await _unit.BeginImport("fast_lap", SessionKey(session.SessionKey), cancellationToken);
        List<Lap> stored = await ReadStoredLaps(session.SessionKey, [driver], cancellationToken);
        LapCatalogSnapshot? previous = await ReadSnapshot<LapCatalogSnapshot>("lap_catalog_v2", DriverKey(session.SessionKey, driver), cancellationToken);
        if (IsIntact(previous, session, driver, stored) && previous!.Complete)
        {
            return previous;
        }
        if (stored.Any(lap => lap.MeetingKey != session.MeetingKey))
        {
            throw new LapCatalogException(409, "Stored lap identity conflicts with the session's meeting.");
        }

        DataResponse<LapListDto> response = await _laps.GetAllLapsSessionByDriver(session.SessionKey, driver, cancellationToken);
        if (!response.HasSuccess || response.Itens == null)
        {
            throw new LapCatalogException(503, "OpenF1 lap import failed; persisted laps have been retained.");
        }
        if (!ValidLaps(response.Itens, session, driver))
        {
            throw new LapCatalogException(502, "OpenF1 returned invalid or duplicate lap identities.");
        }

        Dictionary<int, LapListDto> incoming = response.Itens.ToDictionary(lap => lap.LapNumber);
        // Never erase previously stored laps when a source response shrinks.
        foreach (Lap lap in stored.Where(lap => !incoming.ContainsKey(lap.LapNumber)))
        {
            LapListDto? original = previous?.Laps?.FirstOrDefault(source => source.LapNumber == lap.LapNumber);
            incoming.Add(lap.LapNumber, original ?? FromStored(lap));
        }
        List<LapListDto> all = incoming.Values.OrderBy(lap => lap.LapNumber).ToList();
        bool shrank = response.Itens.Count != all.Count;
        bool gaps = all.Count > 0 && (all[0].LapNumber != 1 || all[^1].LapNumber != all.Count);
        LapCatalogSnapshot snapshot = new()
        {
            SessionKey = session.SessionKey,
            DriverNumber = driver,
            Laps = all,
            Complete = !shrank && !gaps,
            IncompleteReason = shrank ? "The source omitted previously persisted laps." :
                gaps ? "The source lap sequence has gaps; all available laps are retained." : null
        };

        foreach (LapListDto source in response.Itens)
        {
            Lap entity = new()
            {
                SessionKey = session.SessionKey, MeetingKey = session.MeetingKey, DriverNumber = driver,
                LapNumber = source.LapNumber, LapDuration = source.LapDuration,
                DateStart = source.DateStart ?? default, IsPitOutLap = source.IsPitOutLap ?? false,
                DurationSector1 = source.DurationSector1, DurationSector2 = source.DurationSector2,
                DurationSector3 = source.DurationSector3, I1Speed = source.I1Speed, I2Speed = source.I2Speed,
                StSpeed = source.StSpeed,
                IsFastLap = stored.Any(lap => lap.LapNumber == source.LapNumber && lap.IsFastLap)
            };
            Response saved = await _unit.LapDao.UpsertLap(entity, cancellationToken);
            if (!saved.HasSuccess)
            {
                throw new LapCatalogException(503, "Unable to store the lap catalog.");
            }
        }
        await Stage("lap_catalog_v2", DriverKey(session.SessionKey, driver), snapshot, cancellationToken);
        await Commit(transaction, cancellationToken);
        return snapshot;
    }

    private static LapListDto FromStored(Lap lap)
    {
        return new LapListDto
        {
            SessionKey = lap.SessionKey, MeetingKey = lap.MeetingKey, DriverNumber = lap.DriverNumber,
            LapNumber = lap.LapNumber, LapDuration = lap.LapDuration,
            DateStart = lap.DateStart == default ? null : lap.DateStart,
            // Legacy false does not establish that the source supplied the field.
            IsPitOutLap = lap.IsPitOutLap ? true : null,
            DurationSector1 = lap.DurationSector1, DurationSector2 = lap.DurationSector2,
            DurationSector3 = lap.DurationSector3, I1Speed = lap.I1Speed, I2Speed = lap.I2Speed, StSpeed = lap.StSpeed
        };
    }

    private async Task<LapCatalogEnrichment> ReadEnrichment(Session session, bool import, CancellationToken cancellationToken)
    {
        string key = SessionKey(session.SessionKey);
        LapCatalogEnrichment? stored = await ReadSnapshot<LapCatalogEnrichment>("lap_catalog_enrichment", key, cancellationToken);
        if (!import)
        {
            return stored ?? new LapCatalogEnrichment { UnavailableSources = ["stints", "pit", "race_control"] };
        }

        await using IImportTransaction transaction = await _unit.BeginImport("lap_catalog_enrichment", key, cancellationToken);
        LapCatalogEnrichment enrichment = await ReadSnapshot<LapCatalogEnrichment>("lap_catalog_enrichment", key, cancellationToken)
            ?? new LapCatalogEnrichment();
        enrichment.UnavailableSources.Clear();

        if (enrichment.Stints == null || enrichment.Stints.Any(stint => stint.LapStart is null or <= 0 || stint.LapEnd == null || stint.LapEnd < stint.LapStart))
        {
            enrichment.Stints = await GetSource("stints", key, session,
                token => _stints.GetStintsBySessionKey(session.SessionKey, token),
                stint => stint.SessionKey == session.SessionKey && stint.MeetingKey == session.MeetingKey,
                cancellationToken);
        }
        if (enrichment.Pits == null)
        {
            enrichment.Pits = await GetSource("pits", key, session,
                token => _pits.GetPitsBySessionKeyApi(session.SessionKey, token),
                pit => pit.SessionKey == session.SessionKey && pit.MeetingKey == session.MeetingKey,
                cancellationToken);
        }
        if (enrichment.RaceControls == null)
        {
            string flagsHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("[]")));
            enrichment.RaceControls = await GetSource("race_control", key + ":" + flagsHash, session,
                token => _controls.GetRaceControlsBySessionFlagsApi(session.SessionKey, [], token),
                control => control.SessionKey == session.SessionKey && control.MeetingKey == session.MeetingKey,
                cancellationToken);
        }
        if (enrichment.Stints == null)
        {
            enrichment.UnavailableSources.Add("stints");
        }
        else if (enrichment.Stints.Any(stint => stint.LapStart is null or <= 0 || stint.LapEnd == null || stint.LapEnd < stint.LapStart))
        {
            enrichment.UnavailableSources.Add("stint_intervals_incomplete");
        }
        if (enrichment.Pits == null)
        {
            enrichment.UnavailableSources.Add("pit");
        }
        if (enrichment.RaceControls == null)
        {
            enrichment.UnavailableSources.Add("race_control");
        }

        await Stage("lap_catalog_enrichment", key, enrichment, cancellationToken);
        await Commit(transaction, cancellationToken);
        return enrichment;
    }

    private async Task<List<T>?> GetSource<T>(string dataset, string key, Session session,
        Func<CancellationToken, Task<DataResponse<T>>> fetch, Func<T, bool> valid, CancellationToken cancellationToken)
    {
        // A previously completed historical response proves coverage; arbitrary DAO rows do not.
        ImportWindow? window = await _unit.GetImportWindow(dataset, key, cancellationToken);
        if (window?.IsComplete == true && window.CompletedAtUtc >= session.DateEnd.AddMinutes(30))
        {
            try
            {
                List<T>? cached = JsonSerializer.Deserialize<List<T>>(window.Payload);
                if (cached != null && cached.All(item => item != null && valid(item)))
                {
                    // An incomplete stint snapshot must be allowed to refresh through its service.
                    bool incompleteStints = cached.OfType<Stint>().Any(stint => stint.LapStart is null or <= 0 || stint.LapEnd == null || stint.LapEnd < stint.LapStart);
                    if (!incompleteStints)
                    {
                        return cached;
                    }
                }
            }
            catch (JsonException)
            {
                // Invalid persisted payload is demonstrable incompleteness.
            }
        }

        try
        {
            DataResponse<T> response = await fetch(cancellationToken);
            return response.HasSuccess && response.Itens != null && response.Itens.All(item => item != null && valid(item))
                ? response.Itens : null;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public Task<SingleResponse<LapSelectionDto>> Preview(int sessionKey, LapSelectionRequest request, CancellationToken cancellationToken)
    {
        return Execute(async () =>
        {
            if (request?.Selections == null || request.Selections.Count != 3 ||
                request.Selections.Any(selection => selection == null || selection.DriverNumber <= 0 || selection.LapNumber <= 0) ||
                request.Selections.Select(selection => selection!.DriverNumber).Distinct().Count() != 3)
            {
                throw new LapCatalogException(400, "Exactly three distinct driver and lap selections are required.");
            }
            int[] drivers = request.Selections.Select(selection => selection!.DriverNumber).ToArray();
            LapCatalogDto catalog = await ReadCatalog(sessionKey, drivers, null, null, null, false, cancellationToken);
            LapSelectionDto preview = new() { PreviewId = Guid.NewGuid(), SessionKey = sessionKey };
            foreach (LapSelectionItem selection in request.Selections.Select(selection => selection!))
            {
                CatalogLapDto? lap = catalog.Drivers.Single(driver => driver.DriverNumber == selection.DriverNumber)
                    .Laps.SingleOrDefault(lap => lap.LapNumber == selection.LapNumber);
                if (lap == null)
                {
                    throw new LapCatalogException(400, "The selected lap does not exist in this driver's persisted catalog.");
                }
                preview.Selections.Add(lap);
            }

            string key = PreviewKey(sessionKey, preview.PreviewId);
            await using IImportTransaction transaction = await _unit.BeginImport("lap_preview", key, cancellationToken);
            await Stage("lap_preview", key, preview, cancellationToken);
            await Commit(transaction, cancellationToken);
            return preview;
        }, cancellationToken);
    }

    public Task<SingleResponse<LapSelectionDto>> Confirm(int sessionKey, Guid previewId, CancellationToken cancellationToken)
    {
        return Execute(async () =>
        {
            ValidateSelectionKey(sessionKey, previewId);
            string key = SessionKey(sessionKey);
            await using IImportTransaction transaction = await _unit.BeginImport("lap_selection", key, cancellationToken);
            LapSelectionDto preview = await ReadSelection(sessionKey, previewId, cancellationToken);
            preview.Confirmed = true;
            await Stage("lap_preview", PreviewKey(sessionKey, previewId), preview, cancellationToken);
            await Stage("lap_selection", key, preview, cancellationToken);
            await Commit(transaction, cancellationToken);
            return preview;
        }, cancellationToken);
    }

    public Task<SingleResponse<LapSelectionDto>> GetSelection(int sessionKey, Guid? previewId, CancellationToken cancellationToken)
    {
        return Execute(() => ReadSelection(sessionKey, previewId, cancellationToken), cancellationToken);
    }

    private async Task<LapSelectionDto> ReadSelection(int sessionKey, Guid? previewId, CancellationToken cancellationToken)
    {
        ValidateSelectionKey(sessionKey, previewId);
        string dataset = previewId.HasValue ? "lap_preview" : "lap_selection";
        string key = previewId.HasValue ? PreviewKey(sessionKey, previewId.Value) : SessionKey(sessionKey);
        LapSelectionDto? selection = await ReadSnapshot<LapSelectionDto>(dataset, key, cancellationToken);
        if (selection == null)
        {
            throw new LapCatalogException(404, "The persisted selection was not found.");
        }
        if (selection.SessionKey != sessionKey || selection.Selections == null || selection.Selections.Count != 3 ||
            selection.Selections.Any(lap => lap == null || lap.DriverNumber <= 0 || lap.LapNumber <= 0) ||
            selection.Selections.Select(lap => lap.DriverNumber).Distinct().Count() != 3)
        {
            throw new LapCatalogException(409, "The persisted selection is incomplete.");
        }
        return selection;
    }

    private static void ValidateSelectionKey(int sessionKey, Guid? previewId)
    {
        if (sessionKey <= 0 || previewId == Guid.Empty)
        {
            throw new LapCatalogException(400, "A positive session key and a valid preview identifier are required.");
        }
    }

    private async Task<T?> ReadSnapshot<T>(string dataset, string key, CancellationToken cancellationToken) where T : class
    {
        ImportWindow? window = await _unit.GetImportWindow(dataset, key, cancellationToken);
        if (window?.IsComplete != true)
        {
            return null;
        }
        try
        {
            return JsonSerializer.Deserialize<T>(window.Payload);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private Task Stage<T>(string dataset, string key, T payload, CancellationToken cancellationToken)
    {
        return _unit.StageImportWindow(new ImportWindow
        {
            Dataset = dataset, WindowKey = key, CompletedAtUtc = DateTimeOffset.UtcNow,
            ExpiresAtUtc = DateTimeOffset.MaxValue, IsComplete = true, Payload = JsonSerializer.Serialize(payload)
        }, cancellationToken);
    }

    private async Task Commit(IImportTransaction transaction, CancellationToken cancellationToken)
    {
        Response response = await _unit.Commit(cancellationToken);
        if (!response.HasSuccess)
        {
            throw new LapCatalogException(503, "Unable to persist the catalog or selection.");
        }
        await transaction.CommitAsync(cancellationToken);
    }

    private static async Task<SingleResponse<T>> Execute<T>(Func<Task<T>> action, CancellationToken cancellationToken)
    {
        try
        {
            T result = await action();
            return ResponseFactory.CreateInstance().CreateSuccessSingleResponse(result);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            return ResponseFactory.CreateInstance().CreateFailureSingleResponse<T>(exception);
        }
    }

    private static string SessionKey(int sessionKey) => sessionKey.ToString(CultureInfo.InvariantCulture);
    private static string DriverKey(int sessionKey, int driver) => SessionKey(sessionKey) + ":" + driver.ToString(CultureInfo.InvariantCulture);
    private static string PreviewKey(int sessionKey, Guid preview) => SessionKey(sessionKey) + ":" + preview.ToString("N");
}
