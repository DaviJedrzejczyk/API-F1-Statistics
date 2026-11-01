using System.Data.Common;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AutoMapper;
using Dao;
using Dao.Impl;
using Dao.Interface;
using Entities.Class;
using ExternalApi.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Services.Impl;
using Services.Interfaces;
using Shared.Responses;
using UnitTests.WebApi;
using WebApi.Config;

namespace UnitTests.Integration
{
    [TestFixture]
    [NonParallelizable]
    public class SqlServerMeetingIntegrationTests
    {
        private const string SqlServerConnectionEnvironmentVariable = "INTEGRATION_SQLSERVER_CONNECTION";

        private string _connectionString = string.Empty;
        private ApiErrorTestHost _host = null!;

        [OneTimeSetUp]
        public async Task SetUp()
        {
            string? serverConnectionString = Environment.GetEnvironmentVariable(SqlServerConnectionEnvironmentVariable);
            if (string.IsNullOrWhiteSpace(serverConnectionString))
            {
                Assert.Ignore($"{SqlServerConnectionEnvironmentVariable} is required to run SQL Server integration tests.");
            }

            var connectionStringBuilder = new DbConnectionStringBuilder
            {
                ConnectionString = serverConnectionString
            };
            connectionStringBuilder["Initial Catalog"] = $"api_f1_integration_{Guid.NewGuid():N}";
            _connectionString = connectionStringBuilder.ConnectionString;

            await using (var database = CreateDbContext())
            {
                await database.Database.MigrateAsync();
                Assert.That(await database.Database.GetAppliedMigrationsAsync(), Is.Not.Empty);
            }

            _host = await ApiErrorTestHost.StartAsync(services =>
            {
                services.AddDbContext<ApiF1DB>(options => options.UseSqlServer(_connectionString));
                services.AddAutoMapper(configuration => configuration.AddProfile<AutoMapperConfig>());
                services.AddScoped<IMeetingService, MeetingService>();
                services.AddScoped<IUnityOfWork>(CreateUnityOfWork);
                services.AddSingleton<IMeetingClient, FixtureMeetingClient>();
            });
        }

        [SetUp]
        public async Task ResetDatabase()
        {
            await using var database = CreateDbContext();
            await database.Database.ExecuteSqlRawAsync("DELETE FROM [F1_MEETINGS]");
        }

        [OneTimeTearDown]
        public async Task TearDown()
        {
            if (_host != null)
            {
                await _host.DisposeAsync();
            }

            if (!string.IsNullOrEmpty(_connectionString))
            {
                await using var database = CreateDbContext();
                await database.Database.EnsureDeletedAsync();
            }
        }

        [Test]
        public async Task InsertTracks_WithFixtureResponse_PersistsMigratedSqlDataAndServesItOverHttp()
        {
            HttpResponseMessage insertResponse = await _host.Client.PostAsJsonAsync(
                "/api/Meeting/tracks-season",
                new { year = 2024 });

            Assert.That(insertResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            await using (var database = CreateDbContext())
            {
                Meeting? persistedMeeting = await database.Meetings.SingleOrDefaultAsync(meeting => meeting.MeetingKey == FixtureMeetingClient.MeetingKey);
                Assert.That(persistedMeeting, Is.Not.Null);
                Assert.That(persistedMeeting!.MeetingName, Is.EqualTo("Integration Test Grand Prix"));
            }

            HttpResponseMessage getResponse = await _host.Client.GetAsync($"/api/Meeting/{FixtureMeetingClient.MeetingKey}");
            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            using JsonDocument body = JsonDocument.Parse(await getResponse.Content.ReadAsStringAsync());
            Assert.That(body.RootElement.GetProperty("meeting_key").GetInt32(), Is.EqualTo(FixtureMeetingClient.MeetingKey));
        }

        [Test]
        public async Task GetMeeting_WhenDataIsMissing_ReturnsNotFoundOverHttp()
        {
            HttpResponseMessage response = await _host.Client.GetAsync("/api/Meeting/999999");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

            using JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.That(body.RootElement.GetProperty("status_code").GetInt32(), Is.EqualTo((int)HttpStatusCode.NotFound));
        }

        [Test]
        public async Task InsertTracks_WhenFixtureSimulatesExternalFailure_ReturnsBadRequestWithoutNetworkCall()
        {
            HttpResponseMessage response = await _host.Client.PostAsJsonAsync(
                "/api/Meeting/tracks-season",
                new { year = 2025 });

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            using JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.That(body.RootElement.GetProperty("message").GetString(), Is.EqualTo(FixtureMeetingClient.ExternalFailureMessage));
        }

        private ApiF1DB CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<ApiF1DB>()
                .UseSqlServer(_connectionString)
                .Options;

            return new ApiF1DB(options);
        }

        private static IUnityOfWork CreateUnityOfWork(IServiceProvider serviceProvider)
        {
            ApiF1DB database = serviceProvider.GetRequiredService<ApiF1DB>();
            var unitOfWork = new Mock<IUnityOfWork>();
            unitOfWork.SetupGet(value => value.MeetingDao).Returns(new MeetingDao(database));
            unitOfWork.Setup(value => value.Commit()).Returns(async () =>
            {
                await database.SaveChangesAsync();
                return ResponseFactory.CreateInstance().CreateSuccessResponse();
            });

            return unitOfWork.Object;
        }

        private sealed class FixtureMeetingClient : IMeetingClient
        {
            public const int MeetingKey = 910001;
            public const string ExternalFailureMessage = "Simulated upstream failure.";

            public Task<DataResponse<Meeting>> GetMeetingsByYear(int year)
            {
                if (year == 2025)
                {
                    return Task.FromResult(ResponseFactory.CreateInstance()
                        .CreateFailureDataResponse<Meeting>(ExternalFailureMessage));
                }

                return Task.FromResult(ResponseFactory.CreateInstance()
                    .CreateSuccessDataResponse(new List<Meeting>
                    {
                        new()
                        {
                            CircuitKey = 910001,
                            CircuitInfoUrl = "https://fixtures.invalid/circuit",
                            CircuitImage = "https://fixtures.invalid/circuit.png",
                            CircuitShortName = "Fixture",
                            CircuitType = "Permanent",
                            CountryCode = "TST",
                            CountryFlag = "https://fixtures.invalid/flag.png",
                            CountryKey = 910001,
                            CountryName = "Test Country",
                            DateStart = new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.Zero),
                            DateEnd = new DateTimeOffset(2024, 1, 3, 10, 0, 0, TimeSpan.Zero),
                            GmtOffset = "00:00:00",
                            IsCancelled = false,
                            Location = "Test Location",
                            MeetingKey = MeetingKey,
                            MeetingName = "Integration Test Grand Prix",
                            MeetingOfficialName = "Integration Test Grand Prix",
                            Year = year
                        }
                    }));
            }
        }
    }
}
