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
            services.AddTransient<IF1ApiClient>(_ => new F1ApiClient(new HttpClient(transport, false) { BaseAddress = new Uri("https://fixtures.invalid/v1/") }));
        });
    }

    [SetUp]
    public async Task Reset()
    {
        await using var db = Open();
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
        public void Reset() { expected = null; calls = 0; unexpected = false; }
        public void Expect(string path, HttpStatusCode responseStatus, string responseBody) { expected = path; status = responseStatus; body = responseBody; }
        public void Verify() { Assert.That(unexpected, Is.False, "Unexpected upstream request"); Assert.That(calls, Is.EqualTo(expected == null ? 0 : 1), "Unexpected number of upstream requests"); }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            calls++;
            if (request.Method != HttpMethod.Get || request.RequestUri?.Host != "fixtures.invalid" || request.RequestUri.PathAndQuery != expected || calls != 1)
            { unexpected = true; throw new InvalidOperationException($"Unexpected upstream request: {request.RequestUri}"); }
            return Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(body) });
        }
    }
}


