using System.Data.Common;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Dao;
using ExternalApi.Impls;
using ExternalApi.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UnitTests.WebApi;
using WebApi.Config;
using Services.Interfaces;
using Entities.Class;

namespace UnitTests.Integration;

[TestFixture, NonParallelizable]
public class SqlServerImportIntegrationTests
{
    private string connection = string.Empty;
    private ApiErrorTestHost host = null!;
    private FixtureTransport transport = null!;

    [OneTimeSetUp]
    public async Task Start()
    {
        var server = Environment.GetEnvironmentVariable("INTEGRATION_SQLSERVER_CONNECTION");
        if (string.IsNullOrWhiteSpace(server)) Assert.Ignore("INTEGRATION_SQLSERVER_CONNECTION is required for SQL Server integration tests.");
        var builder = new DbConnectionStringBuilder { ConnectionString = server! };
        builder["Initial Catalog"] = $"api_f1_import_{Guid.NewGuid():N}";
        connection = builder.ConnectionString;
        await using (var db = Open()) await db.Database.MigrateAsync();
        transport = new FixtureTransport();
        host = await ApiErrorTestHost.StartAsync(services =>
        {
            services.AddDbContext<ApiF1DB>(options => options.UseSqlServer(connection));
            services.AddAutoMapper(config => config.AddProfile<AutoMapperConfig>());
            services.AddApplicationServicesTransient();
            services.AddTransient<IF1ApiClient>(_ => new F1ApiClient(
                new HttpClient(transport, false) { BaseAddress = new Uri("https://fixtures.invalid/v1/") },
                TimeProvider.System, Task.Delay, token => { token.ThrowIfCancellationRequested(); return Task.CompletedTask; }));
        });
    }

    [SetUp]
    public async Task Reset()
    {
        await using var db = Open();
        await db.ImportWindows.ExecuteDeleteAsync();
        await db.Stints.ExecuteDeleteAsync();
        await db.CarDatas.ExecuteDeleteAsync();
        await db.RaceControls.ExecuteDeleteAsync();
        await db.Pits.ExecuteDeleteAsync();
        await db.Overtakes.ExecuteDeleteAsync();
        await db.SessionResults.ExecuteDeleteAsync();
        await db.SessionResultQualifyings.ExecuteDeleteAsync();
        await db.LapSegments.ExecuteDeleteAsync();
        await db.Laps.ExecuteDeleteAsync();
        await db.LapFastSectors.ExecuteDeleteAsync();
        await db.Sessions.ExecuteDeleteAsync();
        await db.Drivers.ExecuteDeleteAsync();
        transport.Reset();
    }

    [TearDown]
    public void VerifyTransport() => transport.Verify();

    [OneTimeTearDown]
    public async Task Stop()
    {
        if (host != null) await host.DisposeAsync();
        transport?.Dispose();
        if (!string.IsNullOrEmpty(connection)) { await using var db = Open(); await db.Database.EnsureDeletedAsync(); }
    }

    [Test]
    public async Task SessionImport_PersistsMappedDataAndReadsBothKeys()
    {
        transport.Expect("/v1/sessions?meeting_key=1234", HttpStatusCode.OK, "[{\"session_key\":9056,\"meeting_key\":1234,\"country_code\":\"BEL\",\"session_name\":\"Race\",\"date_start\":\"2024-07-28T13:00:00Z\"}]");
        using var response = await host.Client.PostAsJsonAsync("/api/Session/insert-sessions", new { meeting_key = 1234 });
        await AssertEnvelope(response, HttpStatusCode.OK);
        await using (var db = Open())
        {
            var saved = await db.Sessions.SingleAsync();
            Assert.That((saved.SessionKey, saved.MeetingKey, saved.CountryCode, saved.SessionName), Is.EqualTo((9056, 1234, "BEL", "Race")));
            Assert.That(saved.DateStart, Is.EqualTo(DateTimeOffset.Parse("2024-07-28T13:00:00Z")));
        }
        using var found = await host.Client.GetAsync("/api/Session/9056?meetingkey=1234");
        await AssertEnvelope(found, HttpStatusCode.OK);
        using var missing = await host.Client.GetAsync("/api/Session/9056?meetingkey=9999");
        await AssertEnvelope(missing, HttpStatusCode.NotFound);
    }

    [TestCase("[]", HttpStatusCode.OK, HttpStatusCode.OK)]
    [TestCase("upstream failure", HttpStatusCode.ServiceUnavailable, HttpStatusCode.BadRequest)]
    [TestCase("{invalid", HttpStatusCode.OK, HttpStatusCode.BadRequest)]
    public async Task SessionImport_EmptyOrFailure_DoesNotWrite(string body, HttpStatusCode upstream, HttpStatusCode expected)
    {
        transport.Expect("/v1/sessions?meeting_key=1234", upstream, body);
        using var response = await host.Client.PostAsJsonAsync("/api/Session/insert-sessions", new { meeting_key = 1234 });
        await AssertEnvelope(response, expected);
        await using var db = Open();
        Assert.That(await db.Sessions.CountAsync(), Is.Zero);
    }

    [Test]
    public async Task SessionImport_LegacyPartialRowsAreRecoveredIdempotently()
    {
        await using (var db = Open())
        {
            db.Sessions.Add(new Entities.Class.Session { MeetingKey = 1234, SessionKey = 9056 });
            await db.SaveChangesAsync();
        }
        const string body = "[{\"meeting_key\":1234,\"session_key\":9056},{\"meeting_key\":1234,\"session_key\":9057}]";
        transport.Expect("/v1/sessions?meeting_key=1234", HttpStatusCode.OK, body);
        using (var first = await host.Client.PostAsJsonAsync("/api/Session/insert-sessions", new { meeting_key = 1234 }))
            await AssertEnvelope(first, HttpStatusCode.OK);
        transport.Verify();
        transport.Reset();
        using (var cached = await host.Client.PostAsJsonAsync("/api/Session/insert-sessions", new { meeting_key = 1234 }))
            await AssertEnvelope(cached, HttpStatusCode.OK);
        transport.Verify();
        await using (var db = Open())
        {
            Assert.That(await db.Sessions.CountAsync(), Is.EqualTo(2));
            var window = await db.ImportWindows.SingleAsync();
            window.IsComplete = false;
            await db.SaveChangesAsync();
        }
        transport.Reset();
        transport.Expect("/v1/sessions?meeting_key=1234", HttpStatusCode.OK, body);
        using var retry = await host.Client.PostAsJsonAsync("/api/Session/insert-sessions", new { meeting_key = 1234 });
        await AssertEnvelope(retry, HttpStatusCode.OK);
        await using var check = Open();
        Assert.That(await check.Sessions.CountAsync(), Is.EqualTo(2));
        Assert.That((await check.ImportWindows.SingleAsync()).IsComplete, Is.True);
    }

    [Test]
    public async Task DriverImport_PersistsMappedDriverWithSqlIdentity()
    {
        transport.Expect("/v1/drivers?session_key=9056", HttpStatusCode.OK, "[{\"driver_number\":1,\"meeting_key\":1234,\"session_key\":9056,\"full_name\":\"Fixture Driver\"}]");
        using var response = await host.Client.GetAsync("/api/Driver/drivers-session?sessionKey=9056");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.That(json.RootElement[0].GetProperty("full_name").GetString(), Is.EqualTo("Fixture Driver"));
        await using var db = Open();
        var driver = await db.Drivers.SingleAsync();
        Assert.That(driver.DriverKey, Is.GreaterThan(0));
        Assert.That((driver.DriverNumber, driver.SessionKey, driver.FullName), Is.EqualTo((1, 9056, "Fixture Driver")));
    }

    [Test]
    public async Task DriverImport_LegacyPartialRowsAreNotACompleteWindow_AndWindowExpires()
    {
        await using (var db = Open())
        {
            db.Drivers.AddRange(Enumerable.Range(1, 22).Select(n => new Entities.Class.Driver
                { SessionKey = 9056, MeetingKey = 1234, DriverNumber = n }));
            await db.SaveChangesAsync();
        }
        const string body = "[{\"driver_number\":1,\"meeting_key\":1234,\"session_key\":9056,\"full_name\":\"Current\"},{\"driver_number\":23,\"meeting_key\":1234,\"session_key\":9056,\"full_name\":\"New\"}]";
        transport.Expect("/v1/drivers?session_key=9056", HttpStatusCode.OK, body);
        using (var first = await host.Client.GetAsync("/api/Driver/drivers-session?sessionKey=9056"))
        {
            Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            using var json = JsonDocument.Parse(await first.Content.ReadAsStringAsync());
            Assert.That(json.RootElement.GetArrayLength(), Is.EqualTo(2));
        }
        transport.Verify();
        transport.Reset();
        using (var cached = await host.Client.GetAsync("/api/Driver/drivers-session?sessionKey=9056"))
            Assert.That(cached.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        transport.Verify();
        await using (var db = Open())
        {
            var window = await db.ImportWindows.SingleAsync();
            Assert.That(window.IsComplete, Is.True);
            Assert.That(await db.Drivers.CountAsync(), Is.EqualTo(23));
            window.IsComplete = false;
            await db.SaveChangesAsync();
        }
        transport.Reset();
        transport.Expect("/v1/drivers?session_key=9056", HttpStatusCode.OK, body);
        using (var recovered = await host.Client.GetAsync("/api/Driver/drivers-session?sessionKey=9056"))
            Assert.That(recovered.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        transport.Verify();
        await using (var db = Open())
        {
            var window = await db.ImportWindows.SingleAsync();
            Assert.That(window.IsComplete, Is.True);
            window.ExpiresAtUtc = DateTimeOffset.UtcNow.AddSeconds(-1);
            await db.SaveChangesAsync();
        }
        transport.Reset();
        transport.Expect("/v1/drivers?session_key=9056", HttpStatusCode.OK, body);
        using var refreshed = await host.Client.GetAsync("/api/Driver/drivers-session?sessionKey=9056");
        Assert.That(refreshed.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        await using var check = Open();
        Assert.That(await check.Drivers.CountAsync(), Is.EqualTo(23));
        Assert.That((await check.ImportWindows.SingleAsync()).ExpiresAtUtc, Is.GreaterThan(DateTimeOffset.UtcNow));
    }

    [Test]
    public async Task DriverImport_DatabaseFailureDoesNotMarkWindowComplete()
    {
        var body = JsonSerializer.Serialize(new[] { new { driver_number = 1, meeting_key = 1234, session_key = 9056, full_name = new string('x', 201) } });
        transport.Expect("/v1/drivers?session_key=9056", HttpStatusCode.OK, body);
        using var response = await host.Client.GetAsync("/api/Driver/drivers-session?sessionKey=9056");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        await using var db = Open();
        Assert.That(await db.ImportWindows.CountAsync(), Is.Zero);
        Assert.That(await db.Drivers.CountAsync(), Is.Zero);
    }

    [Test]
    public async Task DriverImport_EmptyCompleteWindowKeepsExistingHttpFailureAndAvoidsRefetch()
    {
        transport.Expect("/v1/drivers?session_key=9056", HttpStatusCode.OK, "[]");
        using (var first = await host.Client.GetAsync("/api/Driver/drivers-session?sessionKey=9056"))
            Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        transport.Verify();
        transport.Reset();
        using (var cached = await host.Client.GetAsync("/api/Driver/drivers-session?sessionKey=9056"))
            Assert.That(cached.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        transport.Verify();
        await using var check = Open();
        Assert.That((await check.ImportWindows.SingleAsync()).IsComplete, Is.True);
        Assert.That(await check.Drivers.CountAsync(), Is.Zero);
    }

    [Test]
    public async Task DriverImport_ConcurrentMissesCommitOnlyOneCopy()
    {
        const string body = "[{\"driver_number\":1,\"meeting_key\":1234,\"session_key\":9056,\"full_name\":\"Fixture\"}]";
        transport.ExpectConcurrent("/v1/drivers?session_key=9056", body);
        var requests = new[]
        {
            host.Client.GetAsync("/api/Driver/drivers-session?sessionKey=9056"),
            host.Client.GetAsync("/api/Driver/drivers-session?sessionKey=9056")
        };
        var responses = await Task.WhenAll(requests);
        try { Assert.That(responses.Select(x => x.StatusCode), Is.All.EqualTo(HttpStatusCode.OK)); }
        finally { foreach (var response in responses) response.Dispose(); }
        await using var db = Open();
        Assert.That(await db.Drivers.CountAsync(), Is.EqualTo(1));
        Assert.That(await db.ImportWindows.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task StintImport_PartialLegacyRowsAndExpiredWindowAreRefreshed()
    {
        await using (var db = Open())
        {
            db.Stints.Add(new Stint { MeetingKey = 1234, SessionKey = 9056, DriverNumber = 1, StintNumber = 1, Compound = "OLD" });
            await db.SaveChangesAsync();
        }
        const string body = "[{\"meeting_key\":1234,\"session_key\":9056,\"driver_number\":1,\"stint_number\":1,\"compound\":\"SOFT\"},{\"meeting_key\":1234,\"session_key\":9056,\"driver_number\":2,\"stint_number\":1,\"compound\":\"MEDIUM\"}]";
        transport.Expect("/v1/stints?session_key=9056", HttpStatusCode.OK, body);
        async Task<Shared.Responses.DataResponse<Stint>> GetStints()
        {
            using var scope = host.Services.CreateScope();
            return await scope.ServiceProvider.GetRequiredService<IStintService>().GetStintsBySessionKey(9056);
        }
        var first = await GetStints();
        Assert.That(first.HasSuccess, Is.True, first.Message);
        Assert.That(first.Itens.Select(x => x.Compound), Is.EqualTo(new[] { "SOFT", "MEDIUM" }));
        transport.Verify();
        transport.Reset();
        Assert.That((await GetStints()).Itens.Count, Is.EqualTo(2));
        transport.Verify();
        await using (var db = Open())
        {
            Assert.That(await db.Stints.CountAsync(), Is.EqualTo(2));
            var window = await db.ImportWindows.SingleAsync();
            window.ExpiresAtUtc = DateTimeOffset.UtcNow.AddSeconds(-1);
            await db.SaveChangesAsync();
        }
        transport.Reset();
        transport.Expect("/v1/stints?session_key=9056", HttpStatusCode.OK, body);
        Assert.That((await GetStints()).HasSuccess, Is.True);
        await using var check = Open();
        Assert.That(await check.Stints.CountAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task CarDataImport_ThresholdsUseIndependentWindowsAndRecoverWithoutDuplicates()
    {
        const string body = "[{\"date\":\"2024-01-01T00:00:00Z\",\"meeting_key\":1234,\"session_key\":9056,\"driver_number\":1,\"speed\":310}]";
        async Task<Shared.Responses.DataResponse<CarData>> GetData(int minimum)
        {
            using var scope = host.Services.CreateScope();
            return await scope.ServiceProvider.GetRequiredService<ICarDataService>().GetHighSpeedsSessionApi(9056, minimum);
        }
        transport.Expect("/v1/car_data?session_key=9056&speed%3E=300", HttpStatusCode.OK, body);
        var first = await GetData(300);
        Assert.That(first.HasSuccess, Is.True, first.Message);
        transport.Verify();
        transport.Reset();
        transport.Expect("/v1/car_data?session_key=9056&speed%3E=305", HttpStatusCode.OK, body);
        Assert.That((await GetData(305)).HasSuccess, Is.True);
        transport.Verify();
        transport.Reset();
        Assert.That((await GetData(300)).Itens.Single().Speed, Is.EqualTo(310));
        transport.Verify();
        await using var db = Open();
        Assert.That(await db.CarDatas.CountAsync(), Is.EqualTo(1));
        Assert.That(await db.ImportWindows.CountAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task RaceControlImport_DoesNotTreatLegacyRowsAsComplete_AndReusesWindowForFlagOrder()
    {
        var now = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        await using (var db = Open())
        {
            db.RaceControls.Add(new RaceControl { SessionKey = 9056, MeetingKey = 1234, Date = now, Flag = "YELLOW" });
            await db.SaveChangesAsync();
        }
        const string body = "[{\"date\":\"2024-01-01T00:00:00Z\",\"meeting_key\":1234,\"session_key\":9056,\"flag\":\"YELLOW\",\"sector\":1},{\"date\":\"2024-01-01T00:00:10Z\",\"meeting_key\":1234,\"session_key\":9056,\"flag\":\"CLEAR\",\"sector\":1}]";
        transport.Expect("/v1/race_control?session_key=9056&flag=CLEAR&flag=YELLOW", HttpStatusCode.OK, body);
        async Task<Shared.Responses.DataResponse<RaceControl>> Get(params string[] flags)
        {
            using var scope = host.Services.CreateScope();
            return await scope.ServiceProvider.GetRequiredService<IRaceControlService>().GetRaceControlsBySessionFlagsApi(9056, flags);
        }
        var first = await Get("YELLOW", "CLEAR");
        Assert.That(first.HasSuccess, Is.True, first.Message);
        Assert.That(first.Itens.Count, Is.EqualTo(2));
        transport.Verify();
        transport.Reset();
        Assert.That((await Get("CLEAR", "YELLOW")).Itens.Count, Is.EqualTo(2));
        transport.Verify();
        await using var check = Open();
        Assert.That(await check.RaceControls.CountAsync(), Is.EqualTo(2));
        Assert.That(await check.ImportWindows.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task PitImport_RecoversPartialRowsAndCachesCompleteWindow()
    {
        await using (var db = Open())
        {
            db.Pits.Add(new Pit { SessionKey = 9056, MeetingKey = 1234, DriverNumber = 1, Date = new DateTime(2024, 1, 1) });
            await db.SaveChangesAsync();
        }
        const string body = "[{\"date\":\"2024-01-01T00:00:00Z\",\"meeting_key\":1234,\"session_key\":9056,\"driver_number\":1},{\"date\":\"2024-01-01T00:00:10Z\",\"meeting_key\":1234,\"session_key\":9056,\"driver_number\":2}]";
        transport.Expect("/v1/pit?session_key=9056", HttpStatusCode.OK, body);
        async Task<Shared.Responses.DataResponse<Pit>> Get()
        {
            using var scope = host.Services.CreateScope();
            return await scope.ServiceProvider.GetRequiredService<IPitService>().GetPitsBySessionKeyApi(9056);
        }
        var first = await Get();
        Assert.That(first.HasSuccess, Is.True, first.Message);
        Assert.That(first.Itens.Count, Is.EqualTo(2));
        transport.Verify();
        transport.Reset();
        Assert.That((await Get()).Itens.Count, Is.EqualTo(2));
        transport.Verify();
        await using var check = Open();
        Assert.That(await check.Pits.CountAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task OvertakeImport_DependencyFailureRollsBackPitsAndRetrySucceeds()
    {
        const string overtake = "[{\"date\":\"2024-01-01T00:01:00Z\",\"meeting_key\":1234,\"session_key\":9056,\"overtaking_driver_number\":1,\"overtaken_driver_number\":2,\"position\":1}]";
        const string pit = "[{\"date\":\"2024-01-01T00:00:00Z\",\"meeting_key\":1234,\"session_key\":9056,\"driver_number\":1}]";
        const string controls = "/v1/race_control?session_key=9056&flag=CLEAR&flag=DOUBLE%20YELLOW&flag=YELLOW";
        transport.ExpectSequence(("/v1/overtakes?session_key=9056", HttpStatusCode.OK, overtake), ("/v1/pit?session_key=9056", HttpStatusCode.OK, pit), (controls, HttpStatusCode.OK, "{invalid"));
        async Task<Shared.Responses.DataResponse<Overtake>> Get()
        {
            using var scope = host.Services.CreateScope();
            return await scope.ServiceProvider.GetRequiredService<IOvertakeService>().GetOvertakesSessionApi(9056);
        }
        var failed = await Get();
        Assert.That(failed.HasSuccess, Is.False);
        transport.Verify();
        await using (var db = Open())
        {
            Assert.That(await db.Pits.CountAsync(), Is.Zero);
            Assert.That(await db.ImportWindows.CountAsync(), Is.Zero);
            Assert.That(await db.Overtakes.CountAsync(), Is.Zero);
        }
        transport.Reset();
        transport.ExpectSequence(("/v1/overtakes?session_key=9056", HttpStatusCode.OK, overtake), ("/v1/pit?session_key=9056", HttpStatusCode.OK, pit), (controls, HttpStatusCode.OK, "[]"));
        var recovered = await Get();
        Assert.That(recovered.HasSuccess, Is.True, recovered.Message);
        Assert.That(recovered.Itens.Count, Is.EqualTo(1));
        transport.Verify();
        await using var check = Open();
        Assert.That(await check.Pits.CountAsync(), Is.EqualTo(1));
        Assert.That(await check.Overtakes.CountAsync(), Is.EqualTo(1));
        Assert.That(await check.ImportWindows.CountAsync(), Is.EqualTo(3));
    }

    [Test]
    public async Task QualifyingImport_ResultFailureRollsBackStintsAndQualifyingThenRecovers()
    {
        const string resultJson = "[{\"meeting_key\":1234,\"session_key\":9056,\"driver_number\":1,\"position\":1,\"number_of_laps\":10,\"duration\":[70,68,0],\"gap_to_leader\":[0,0,0]}]";
        const string invalidStint = "[{\"meeting_key\":1234,\"session_key\":9056,\"driver_number\":1,\"stint_number\":1,\"lap_end\":10,\"compound\":\"XXXXXXXXXXX\"}]";
        const string validStint = "[{\"meeting_key\":1234,\"session_key\":9056,\"driver_number\":1,\"stint_number\":1,\"lap_end\":10,\"compound\":\"SOFT\"}]";
        async Task<Shared.Responses.DataResponse<SessionResult>> Get()
        {
            using var scope = host.Services.CreateScope();
            return await scope.ServiceProvider.GetRequiredService<ISessionResultService>().GetSessionResultBySessionKeyApi(9056);
        }
        transport.ExpectSequence(("/v1/session_result?session_key=9056", HttpStatusCode.OK, resultJson), ("/v1/stints?session_key=9056", HttpStatusCode.OK, invalidStint));
        Assert.That((await Get()).HasSuccess, Is.False);
        transport.Verify();
        await using (var db = Open())
        {
            Assert.That(await db.Stints.CountAsync(), Is.Zero);
            Assert.That(await db.SessionResultQualifyings.CountAsync(), Is.Zero);
            Assert.That(await db.SessionResults.CountAsync(), Is.Zero);
            Assert.That(await db.ImportWindows.CountAsync(), Is.Zero);
        }
        transport.Reset();
        transport.ExpectSequence(("/v1/session_result?session_key=9056", HttpStatusCode.OK, resultJson), ("/v1/stints?session_key=9056", HttpStatusCode.OK, validStint));
        var recovered = await Get();
        Assert.That(recovered.HasSuccess, Is.True, recovered.Message);
        Assert.That(recovered.Itens.Single().Compound, Is.EqualTo("SOFT"));
        transport.Verify();
        await using var check = Open();
        Assert.That(await check.Stints.CountAsync(), Is.EqualTo(1));
        Assert.That(await check.SessionResultQualifyings.CountAsync(), Is.EqualTo(1));
        Assert.That(await check.SessionResults.CountAsync(), Is.EqualTo(1));
        Assert.That(await check.ImportWindows.CountAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task QualifyingImport_EmptyFirstDurationDoesNotMisclassifyWindow()
    {
        const string results = "[{\"meeting_key\":1234,\"session_key\":9056,\"driver_number\":1,\"position\":2,\"duration\":[],\"gap_to_leader\":[]},{\"meeting_key\":1234,\"session_key\":9056,\"driver_number\":2,\"position\":1,\"duration\":[70,68],\"gap_to_leader\":[0,0]}]";
        transport.ExpectSequence(("/v1/session_result?session_key=9056", HttpStatusCode.OK, results), ("/v1/stints?session_key=9056", HttpStatusCode.OK, "[]"));
        using var scope = host.Services.CreateScope();
        var response = await scope.ServiceProvider.GetRequiredService<ISessionResultService>().GetSessionResultBySessionKeyApi(9056);
        Assert.That(response.HasSuccess, Is.True, response.Message);
        Assert.That(response.Itens, Has.Count.EqualTo(2));
        Assert.That(response.Itens.All(x => x.IsQualy), Is.True);
        await using var db = Open();
        Assert.That(await db.SessionResultQualifyings.CountAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task FastLapImport_FailedDriverLapsRollBackDriversAndRetryCommitsTogether()
    {
        const string drivers = "[{\"session_key\":9056,\"meeting_key\":1234,\"driver_number\":1,\"last_name\":\"One\"},{\"session_key\":9056,\"meeting_key\":1234,\"driver_number\":2,\"last_name\":\"Two\"}]";
        const string lap = "[{\"session_key\":9056,\"meeting_key\":1234,\"driver_number\":1,\"lap_number\":1,\"date_start\":\"2024-01-01T00:00:00Z\",\"lap_duration\":70,\"duration_sector_1\":20,\"duration_sector_2\":25,\"duration_sector_3\":25}]";
        async Task<Shared.Responses.SingleResponse<Entities.Dtos.LapFastLapDto>> Get()
        {
            using var scope = host.Services.CreateScope();
            return await scope.ServiceProvider.GetRequiredService<ILapService>().GetFastLapOfRaceBySessionKey(9056);
        }
        transport.ExpectSequence(("/v1/drivers?session_key=9056", HttpStatusCode.OK, drivers), ("/v1/laps?session_key=9056&driver_number=1", HttpStatusCode.OK, lap), ("/v1/laps?session_key=9056&driver_number=2", HttpStatusCode.OK, "{invalid"));
        Assert.That((await Get()).HasSuccess, Is.False);
        transport.Verify();
        await using (var db = Open())
        {
            Assert.That(await db.Drivers.CountAsync(), Is.Zero);
            Assert.That(await db.Laps.CountAsync(), Is.Zero);
            Assert.That(await db.LapFastSectors.CountAsync(), Is.Zero);
            Assert.That(await db.ImportWindows.CountAsync(), Is.Zero);
        }
        transport.Reset();
        transport.ExpectSequence(("/v1/drivers?session_key=9056", HttpStatusCode.OK, drivers), ("/v1/laps?session_key=9056&driver_number=1", HttpStatusCode.OK, lap), ("/v1/laps?session_key=9056&driver_number=2", HttpStatusCode.OK, "[]"));
        var recovered = await Get();
        Assert.That(recovered.HasSuccess, Is.True, recovered.Message);
        Assert.That(recovered.Item.LapDuration, Is.EqualTo(70));
        transport.Verify();
        await using var check = Open();
        Assert.That(await check.Drivers.CountAsync(), Is.EqualTo(2));
        Assert.That(await check.Laps.CountAsync(), Is.EqualTo(1));
        Assert.That(await check.LapFastSectors.CountAsync(), Is.EqualTo(3));
        Assert.That(await check.ImportWindows.CountAsync(), Is.EqualTo(3));
    }

    [Test]
    public async Task FastLapImport_CancellationReachesUpstreamAndRollsBackDrivers()
    {
        const string drivers = "[{\"session_key\":9056,\"meeting_key\":1234,\"driver_number\":1,\"last_name\":\"One\"}]";
        transport.ExpectSequence(("/v1/drivers?session_key=9056", HttpStatusCode.OK, drivers), ("/v1/laps?session_key=9056&driver_number=1", HttpStatusCode.OK, "[]"));
        transport.BlockRequest(2);
        using var scope = host.Services.CreateScope();
        using var cancel = new CancellationTokenSource();
        var task = scope.ServiceProvider.GetRequiredService<ILapService>().GetFastLapOfRaceBySessionKey(9056, cancel.Token);
        await transport.WaitUntilBlocked().WaitAsync(TimeSpan.FromSeconds(60));
        cancel.Cancel();
        Assert.CatchAsync<OperationCanceledException>(async () => await task);
        await using var check = Open();
        Assert.That(await check.Drivers.CountAsync(), Is.Zero);
        Assert.That(await check.Laps.CountAsync(), Is.Zero);
        Assert.That(await check.ImportWindows.CountAsync(), Is.Zero);
    }

    [Test]
    public async Task FastLapImport_FastLapMarkerWithoutSectorsIsNotComplete()
    {
        await using (var db = Open())
        {
            db.ImportWindows.Add(new global::Dao.Cache.ImportWindow { Dataset = "fast_lap", WindowKey = "9056", IsComplete = true, CompletedAtUtc = DateTimeOffset.UtcNow.AddMinutes(-1), ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(1), Payload = "[{\"DriverNumber\":99,\"LapDuration\":1}]" });
            await db.SaveChangesAsync();
        }
        const string drivers = "[{\"session_key\":9056,\"meeting_key\":1234,\"driver_number\":1,\"last_name\":\"One\"}]";
        const string lap = "[{\"session_key\":9056,\"meeting_key\":1234,\"driver_number\":1,\"lap_number\":1,\"date_start\":\"2024-01-01T00:00:00Z\",\"lap_duration\":70,\"duration_sector_1\":20,\"duration_sector_2\":25,\"duration_sector_3\":25}]";
        transport.ExpectSequence(("/v1/drivers?session_key=9056", HttpStatusCode.OK, drivers), ("/v1/laps?session_key=9056&driver_number=1", HttpStatusCode.OK, lap));
        using var scope = host.Services.CreateScope();
        var response = await scope.ServiceProvider.GetRequiredService<ILapService>().GetFastLapOfRaceBySessionKey(9056);
        Assert.That(response.HasSuccess, Is.True, response.Message);
        Assert.That(response.Item.DriverNumber, Is.EqualTo(1));
        await using var check = Open();
        Assert.That(await check.ImportWindows.CountAsync(), Is.EqualTo(3));
    }

    [Test]
    public async Task Migrations_LeaveAllMappedTablesQueryableAndAreRepeatable()
    {
        await using var db = Open();
        var migrations = db.Database.GetMigrations().ToArray();
        Assert.That(migrations, Is.Not.Empty);
        Assert.That(await db.Database.GetAppliedMigrationsAsync(), Is.EqualTo(migrations));
        Assert.That(await db.Database.GetPendingMigrationsAsync(), Is.Empty);
        await db.Database.MigrateAsync();
        await db.Meetings.Take(1).ToListAsync(); await db.Sessions.Take(1).ToListAsync();
        await db.Drivers.Take(1).ToListAsync(); await db.CarDatas.Take(1).ToListAsync();
        await db.SessionResults.Take(1).ToListAsync(); await db.SessionResultQualifyings.Take(1).ToListAsync();
        await db.Overtakes.Take(1).ToListAsync(); await db.Pits.Take(1).ToListAsync();
        await db.RaceControls.Take(1).ToListAsync(); await db.Stints.Take(1).ToListAsync();
        await db.Laps.Take(1).ToListAsync(); await db.LapSegments.Take(1).ToListAsync();
        await db.LapFastSectors.Take(1).ToListAsync();
    }

    private ApiF1DB Open() => new(new DbContextOptionsBuilder<ApiF1DB>().UseSqlServer(connection).Options);

    private static async Task AssertEnvelope(HttpResponseMessage response, HttpStatusCode status)
    {
        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(status), body);
        using var json = JsonDocument.Parse(body);
        Assert.That(json.RootElement.GetProperty("status_code").GetInt32(), Is.EqualTo((int)status));
        Assert.That(json.RootElement.GetProperty("message").ValueKind, Is.EqualTo(JsonValueKind.String));
    }

    private sealed class FixtureTransport : HttpMessageHandler
    {
        private string? expected;
        private HttpStatusCode status;
        private string body = "";
        private int calls;
        private bool unexpected;
        private TaskCompletionSource? barrier;
        private (string Path, HttpStatusCode Status, string Body)[]? sequence;
        private int blockAt;
        private TaskCompletionSource? blocked;
        public void Reset() { expected = null; calls = 0; unexpected = false; barrier = null; sequence = null; blockAt = 0; blocked = null; }
        public void Expect(string path, HttpStatusCode responseStatus, string responseBody) { expected = path; status = responseStatus; body = responseBody; }
        public void ExpectConcurrent(string path, string responseBody) { Expect(path, HttpStatusCode.OK, responseBody); barrier = new(TaskCreationOptions.RunContinuationsAsynchronously); }
        public void ExpectSequence(params (string Path, HttpStatusCode Status, string Body)[] requests) { sequence = requests; }
        public void BlockRequest(int number) { blockAt = number; blocked = new(TaskCreationOptions.RunContinuationsAsynchronously); }
        public Task WaitUntilBlocked() => blocked!.Task;
        public void Verify() { Assert.That(unexpected, Is.False, "Unexpected upstream request"); Assert.That(calls, Is.EqualTo(sequence != null ? sequence.Length : expected == null ? 0 : barrier != null ? 2 : status == HttpStatusCode.ServiceUnavailable ? 3 : 1), "Unexpected number of upstream requests"); }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var call = Interlocked.Increment(ref calls);
            if (sequence != null)
            {
                if (call > sequence.Length || request.Method != HttpMethod.Get || request.RequestUri?.Host != "fixtures.invalid" || request.RequestUri.PathAndQuery != sequence[call - 1].Path)
                { unexpected = true; throw new InvalidOperationException($"Unexpected upstream request: {request.RequestUri}"); }
                if (call == blockAt)
                {
                    blocked!.TrySetResult();
                    await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                }
                return new HttpResponseMessage(sequence[call - 1].Status) { Content = new StringContent(sequence[call - 1].Body) };
            }
            if (request.Method != HttpMethod.Get || request.RequestUri?.Host != "fixtures.invalid" || request.RequestUri.PathAndQuery != expected || call > (barrier != null ? 2 : status == HttpStatusCode.ServiceUnavailable ? 3 : 1))
            { unexpected = true; throw new InvalidOperationException($"Unexpected upstream request: {request.RequestUri}"); }
            if (barrier != null)
            {
                if (call == 2) barrier.TrySetResult();
                await barrier.Task.WaitAsync(cancellationToken);
            }
            return new HttpResponseMessage(status) { Content = new StringContent(body) };
        }
    }
}
