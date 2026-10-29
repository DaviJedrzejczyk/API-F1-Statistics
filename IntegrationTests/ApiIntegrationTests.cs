using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Dao;
using Dao.Impl;
using Entities.Class;
using ExternalApi.Interfaces;
using IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebApi.BackgroundServices;

namespace IntegrationTests;

[TestFixture, NonParallelizable]
public sealed class ApiIntegrationTests
{
    private TestDatabase database = null!;
    private ApiFactory factory = null!;
    private FixtureHttpHandler transport = null!;
    private HttpClient http = null!;

    [OneTimeSetUp]
    public async Task CreateDatabase()
    {
        database = new TestDatabase();
        try { await database.CreateAsync(); }
        catch { await database.DisposeAsync(); throw; }
    }

    [SetUp]
    public async Task StartHost()
    {
        await using var db = database.Open();
        await db.Sessions.ExecuteDeleteAsync();
        await db.Meetings.ExecuteDeleteAsync();
        await db.Drivers.ExecuteDeleteAsync();
        transport = new FixtureHttpHandler();
        factory = new ApiFactory(database.ConnectionString, transport);
        http = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false
        });
        http.Timeout = TimeSpan.FromSeconds(15);
    }

    [TearDown]
    public async Task StopHost()
    {
        http?.Dispose();
        if (factory != null) await factory.DisposeAsync();
        transport?.VerifyComplete();
    }

    [OneTimeTearDown]
    public async Task DropDatabase()
    {
        if (database != null) await database.DisposeAsync();
    }

    [Test]
    public async Task Migrations_AreAppliedAndRepeatable_AndEveryMappedTableIsQueryable()
    {
        await using var db = database.Open();
        var expected = db.Database.GetMigrations().ToArray();
        Assert.That(expected, Is.Not.Empty);
        Assert.That(await db.Database.GetAppliedMigrationsAsync(), Is.EqualTo(expected));
        Assert.That(await db.Database.GetPendingMigrationsAsync(), Is.Empty);
        await db.Database.MigrateAsync();
        // Real SELECTs validate the migrated schema against every current entity mapping.
        await db.Meetings.Take(1).ToListAsync();
        await db.Sessions.Take(1).ToListAsync();
        await db.Drivers.Take(1).ToListAsync();
        await db.CarDatas.Take(1).ToListAsync();
        await db.SessionResults.Take(1).ToListAsync();
        await db.SessionResultQualifyings.Take(1).ToListAsync();
        await db.Overtakes.Take(1).ToListAsync();
        await db.Pits.Take(1).ToListAsync();
        await db.RaceControls.Take(1).ToListAsync();
        await db.Stints.Take(1).ToListAsync();
        await db.Laps.Take(1).ToListAsync();
        await db.LapSegments.Take(1).ToListAsync();
        await db.LapFastSectors.Take(1).ToListAsync();
    }

    [Test]
    public void Host_RemovesUpdateWorker_AndUsesOnlyTheGeneratedDatabase()
    {
        Assert.That(factory.Services.GetServices<IHostedService>(), Has.None.InstanceOf<SessionUpdateWorker>());
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApiF1DB>();
        Assert.That(db.Database.GetConnectionString(), Is.EqualTo(database.ConnectionString));
        Assert.That(db.Database.ProviderName, Is.EqualTo("Microsoft.EntityFrameworkCore.SqlServer"));
        Assert.That(transport.Requests, Is.Empty);
    }

    [Test]
    public async Task SessionImport_ExercisesHttpBindingExternalMappingCommitAndRead()
    {
        transport.Enqueue("/v1/sessions?meeting_key=1234", "sessions.json");
        using var result = await http.PostAsJsonAsync("/api/Session/insert-sessions", new { meeting_key = 1234 });
        await AssertEnvelope(result, HttpStatusCode.OK);
        transport.VerifyComplete();
        await using var db = database.Open();
        var saved = await db.Sessions.SingleAsync();
        Assert.Multiple(() =>
        {
            Assert.That(saved.SessionKey, Is.EqualTo(9056));
            Assert.That(saved.MeetingKey, Is.EqualTo(1234));
            Assert.That(saved.CountryCode, Is.EqualTo("BEL"));
            Assert.That(saved.SessionName, Is.EqualTo("Race"));
            Assert.That(saved.DateStart, Is.EqualTo(DateTimeOffset.Parse("2024-07-28T13:00:00Z")));
        });
        using var found = await http.GetAsync("/api/Session/9056?meetingkey=1234");
        await AssertEnvelope(found, HttpStatusCode.OK);
        using var wrongMeeting = await http.GetAsync("/api/Session/9056?meetingkey=9999");
        await AssertEnvelope(wrongMeeting, HttpStatusCode.NotFound);
        Assert.That(transport.Requests, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task SessionAbsent_ReturnsErrorViewModelWithoutExternalCall()
    {
        using var result = await http.GetAsync("/api/Session/9999?meetingkey=1234");
        await AssertEnvelope(result, HttpStatusCode.NotFound);
        Assert.That(transport.Requests, Is.Empty);
    }

    [TestCase("empty.json", HttpStatusCode.OK, HttpStatusCode.OK)]
    [TestCase(null, HttpStatusCode.ServiceUnavailable, HttpStatusCode.BadRequest)]
    [TestCase("malformed.json", HttpStatusCode.OK, HttpStatusCode.BadRequest)]
    public async Task SessionImport_EmptyAndExternalFailures_DoNotPersist(string? fixture, HttpStatusCode external, HttpStatusCode expected)
    {
        transport.Enqueue("/v1/sessions?meeting_key=1234", fixture, external);
        using var result = await http.PostAsJsonAsync("/api/Session/insert-sessions", new { meeting_key = 1234 });
        await AssertEnvelope(result, expected);
        transport.VerifyComplete();
        await using var db = database.Open();
        Assert.That(await db.Sessions.CountAsync(), Is.Zero);
    }

    [Test]
    public async Task SessionImport_ExternalTimeout_ReturnsErrorViewModelWithoutWrites()
    {
        transport.EnqueueTimeout("/v1/sessions?meeting_key=1234");
        using var result = await http.PostAsJsonAsync("/api/Session/insert-sessions", new { meeting_key = 1234 });
        await AssertEnvelope(result, HttpStatusCode.BadRequest);
        await using var db = database.Open();
        Assert.That(await db.Sessions.CountAsync(), Is.Zero);
    }

    [Test]
    public async Task MeetingQuery_TranslatesFilterAndOrderingOnSqlServer_AndSerializesViewModel()
    {
        await using (var db = database.Open())
        {
            db.Meetings.AddRange(
                new Meeting { MeetingKey = 3, Year = 2024, MeetingName = "Later Grand Prix", DateStart = DateTimeOffset.Parse("2024-07-01T00:00:00Z") },
                new Meeting { MeetingKey = 1, Year = 2024, MeetingName = "First Grand Prix", DateStart = DateTimeOffset.Parse("2024-03-01T00:00:00Z") },
                new Meeting { MeetingKey = 2, Year = 2024, MeetingName = "Testing" },
                new Meeting { MeetingKey = 4, Year = 2023, MeetingName = "Old Grand Prix" });
            await db.SaveChangesAsync();
        }
        using var result = await http.GetAsync("/api/Meeting/all-tracks/2024");
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var json = await result.Content.ReadFromJsonAsync<JsonElement>();
        Assert.That(json.EnumerateArray().Select(m => m.GetProperty("meeting_key").GetInt32()), Is.EqualTo(new[] { 1, 3 }));
        using var single = await http.GetAsync("/api/Meeting/1");
        Assert.That(single.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That((await single.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("meeting_name").GetString(), Is.EqualTo("First Grand Prix"));
        Assert.That(transport.Requests, Is.Empty);
    }

    [Test]
    public async Task EmptyMeetings_ReturnsEmptyJsonArray()
    {
        using var result = await http.GetAsync("/api/Meeting/all-tracks/2024");
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That((await result.Content.ReadFromJsonAsync<JsonElement>()).GetArrayLength(), Is.Zero);
    }

    [Test]
    public async Task SqlServer_EnforcesPrimaryKey_AndFailedCommitPreservesExistingData()
    {
        await using (var db = database.Open())
        {
            db.Sessions.Add(new Session { SessionKey = 9056, MeetingKey = 1234 });
            await db.SaveChangesAsync();
        }
        await using (var db = database.Open())
        {
            var dao = new SessionDao(db);
            Assert.That((await dao.InsertSessions(new List<Session> { new() { SessionKey = 9056 }, new() { SessionKey = 9999 } })).HasSuccess, Is.True);
            Assert.ThrowsAsync<DbUpdateException>(async () => await db.SaveChangesAsync());
        }
        await using var verify = database.Open();
        Assert.That(await verify.Sessions.Select(s => s.SessionKey).ToArrayAsync(), Is.EqualTo(new[] { 9056 }));
    }

    [Test]
    public async Task DriverImport_ExercisesRealClientMappingAndIdentityPersistence()
    {
        transport.Enqueue("/v1/drivers?session_key=9056", "drivers.json");
        using var result = await http.GetAsync("/api/Driver/drivers-session?sessionKey=9056");
        Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK), await result.Content.ReadAsStringAsync());
        var json = await result.Content.ReadFromJsonAsync<JsonElement>();
        Assert.That(json[0].GetProperty("full_name").GetString(), Is.EqualTo("Fixture Driver"));
        Assert.That(json[0].GetProperty("driver_number").GetInt32(), Is.EqualTo(1));
        await using var db = database.Open();
        var driver = await db.Drivers.SingleAsync();
        Assert.That(driver.DriverKey, Is.GreaterThan(0));
        Assert.That(driver.SessionKey, Is.EqualTo(9056));
        Assert.That(driver.FullName, Is.EqualTo("Fixture Driver"));
        transport.VerifyComplete();
    }

    [Test]
    public async Task ExternalResultsFixture_UsesRealClientAndNullableArrayConverters()
    {
        transport.Enqueue("/v1/session_result?session_key=9056", "results.json");
        using var scope = factory.Services.CreateScope();
        var result = await scope.ServiceProvider.GetRequiredService<ISessionResultClient>().GetSessionResultApi(9056);
        Assert.That(result.HasSuccess, Is.True, result.Message);
        Assert.That(result.Itens, Has.Count.EqualTo(1));
        Assert.That(result.Itens[0].Duration, Is.EqualTo(new[] { 90.1, 0, 89.2 }));
        Assert.That(result.Itens[0].Points, Is.Zero);
        transport.VerifyComplete();
    }

    private static async Task AssertEnvelope(HttpResponseMessage response, HttpStatusCode expected)
    {
        var text = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(expected), text);
        Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
        using var document = JsonDocument.Parse(text);
        Assert.That(document.RootElement.ValueKind, Is.EqualTo(JsonValueKind.Object));
        Assert.That(document.RootElement.GetProperty("status_code").GetInt32(), Is.EqualTo((int)expected));
        Assert.That(document.RootElement.GetProperty("message").ValueKind, Is.EqualTo(JsonValueKind.String));
    }
}
