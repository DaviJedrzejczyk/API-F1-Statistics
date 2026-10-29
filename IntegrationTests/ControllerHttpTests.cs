using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Entities.Class;
using Entities.Dtos;
using IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Services.Interfaces;
using Shared.Responses;

namespace IntegrationTests;

[TestFixture]
public sealed class ControllerHttpTests
{
    private ApiFactory root = null!;
    private WebApplicationFactory<Program> factory = null!;
    private HttpClient http = null!;
    private FixtureHttpHandler transport = null!;

    [SetUp]
    public void StartHost()
    {
        var responses = ResponseFactory.CreateInstance();
        var drivers = new Mock<IDriverService>(MockBehavior.Strict);
        drivers.Setup(s => s.GetAllDriversSession(9056)).ReturnsAsync(responses.CreateSuccessDataResponse(new List<Driver>
            { new() { SessionKey = 9056, DriverNumber = 1, FullName = "Fixture Driver" } }));
        var results = new Mock<ISessionResultService>(MockBehavior.Strict);
        results.Setup(s => s.GetSessionResultBySessionKeyApi(9056)).ReturnsAsync(responses.CreateSuccessDataResponse(new List<SessionResult>
            { new() { SessionKey = 9056, DriverNumber = 1 } }));
        var laps = new Mock<ILapService>(MockBehavior.Strict);
        laps.Setup(s => s.GetFastLapOfRaceBySessionKey(9056)).ReturnsAsync(responses.CreateSuccessSingleResponse(new LapFastLapDto
            { SessionKey = 9056, DriverNumber = 1, LapDuration = 90.123 }));
        laps.Setup(s => s.GetFastLapOfRaceBySessionKey(9999)).ReturnsAsync(responses.CreateFailureSingleResponse<LapFastLapDto>("Fixture: no lap."));
        var sectors = new Mock<ILapFastSectorService>(MockBehavior.Strict);
        sectors.Setup(s => s.GetFastSectorsOfSession(9056)).ReturnsAsync(responses.CreateSuccessDataResponse(new List<LapFastSector>
            { new() { SessionKey = 9056, DriverNumber = 1, Sector = 1, Duration = 30.1 } }));
        var cars = new Mock<ICarDataService>(MockBehavior.Strict);
        cars.Setup(s => s.GetSortedHighSpeedsSession(9056, 300)).ReturnsAsync(responses.CreateSuccessDataResponse(new List<SessionDriverSpeedDTO>
            { new() { SessionKey = 9056, DriverNumber = 1, Speed = 320 } }));
        var overtakes = new Mock<IOvertakeService>(MockBehavior.Strict);
        overtakes.Setup(s => s.GetOvertakesSessionApi(9056)).ReturnsAsync(responses.CreateSuccessDataResponse(new List<Overtake>
            { new() { SessionKey = 9056, MeetingKey = 1234 } }));
        var meetings = new Mock<IMeetingService>(MockBehavior.Strict);
        meetings.Setup(s => s.InsertTracksOfCurrentYear(2024)).ReturnsAsync(responses.CreateSuccessResponse("Fixture imported."));

        transport = new FixtureHttpHandler();
        // These controller boundary tests must never need SQL; all invoked services are strict mocks.
        root = new ApiFactory("Server=unused.invalid;Database=R02_Unused;Connect Timeout=1", transport);
        factory = root.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IDriverService>(); services.AddSingleton(drivers.Object);
            services.RemoveAll<ISessionResultService>(); services.AddSingleton(results.Object);
            services.RemoveAll<ILapService>(); services.AddSingleton(laps.Object);
            services.RemoveAll<ILapFastSectorService>(); services.AddSingleton(sectors.Object);
            services.RemoveAll<ICarDataService>(); services.AddSingleton(cars.Object);
            services.RemoveAll<IOvertakeService>(); services.AddSingleton(overtakes.Object);
            services.RemoveAll<IMeetingService>(); services.AddSingleton(meetings.Object);
        }));
        http = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
        http.Timeout = TimeSpan.FromSeconds(10);
    }

    [TearDown]
    public async Task StopHost()
    {
        http?.Dispose();
        if (factory != null) await factory.DisposeAsync();
        if (root != null) await root.DisposeAsync();
        Assert.That(transport.Requests, Is.Empty);
    }

    [TestCase("/api/Driver/drivers-session?sessionKey=9056", "driver_number", "1")]
    [TestCase("/api/SessionResult/result?sessionKey=9056", "session_key", "9056")]
    [TestCase("/api/Laps/fast_lap_session?sessionKey=9056", "duration", "90.123")]
    [TestCase("/api/Laps/fast_sectors_session?sessionKey=9056", "sector", "1")]
    [TestCase("/api/CarData/high-speed?sessionKey=9056&minimumSpeed=300", "speed", "320")]
    [TestCase("/api/Overtake/get-overtakes-session/9056", "numbers_of_overtakes", "1")]
    public async Task SuccessRoutes_ExerciseBindingMappingAndPublicJson(string url, string property, string expected)
    {
        using var response = await http.GetAsync(url);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var item = json.ValueKind == JsonValueKind.Array ? json[0] : json;
        Assert.That(item.GetProperty(property).GetRawText(), Is.EqualTo(expected));
    }

    [Test]
    public async Task MeetingImport_BindsYearFromJson()
    {
        using var response = await http.PostAsJsonAsync("/api/Meeting/tracks-season", new { year = 2024 });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That((await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("status_code").GetInt32(), Is.EqualTo(200));
    }

    [Test]
    public async Task MissingLap_ReturnsErrorViewModel()
    {
        using var response = await http.GetAsync("/api/Laps/fast_lap_session?sessionKey=9999");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.That(json.GetProperty("status_code").GetInt32(), Is.EqualTo(404));
        Assert.That(json.GetProperty("message").GetString(), Is.EqualTo("Fixture: no lap."));
    }
}
