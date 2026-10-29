using AutoMapper;
using Entities.Class;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Services.Interfaces;
using Shared.Responses;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using WebApi.Config;
using WebApi.ViewModels;

namespace UnitTests.WebApi
{
    [TestFixture]
    public class ApiErrorResponseRegressionTests
    {
        private Mock<ISessionService> _sessionService = null!;
        private Mock<IMeetingService> _meetingService = null!;
        private ApiErrorTestHost _host = null!;

        [OneTimeSetUp]
        public async Task StartHost()
        {
            _sessionService = new Mock<ISessionService>();
            _meetingService = new Mock<IMeetingService>();
            var mapper = new Mock<IMapper>();

            _host = await ApiErrorTestHost.StartAsync(services =>
            {
                services.AddSingleton<ISessionService>(_sessionService.Object);
                services.AddSingleton<IMeetingService>(_meetingService.Object);
                services.AddSingleton<IMapper>(mapper.Object);
            });
        }

        [OneTimeTearDown]
        public async Task StopHost() => await _host.DisposeAsync();

        [SetUp]
        public void ResetMocks()
        {
            _sessionService.Reset();
            _meetingService.Reset();
        }

        [Test]
        public async Task InsertSessions_ValidBody_ReturnsSuccessViewModel()
        {
            _sessionService.Setup(s => s.InsertSessions(7)).ReturnsAsync(new Response { HasSuccess = true, Message = "inserted" });

            using var response = await PostAsync("/api/Session/insert-sessions", "{\"meeting_key\":7}");

            Assert.That((int)response.StatusCode, Is.EqualTo(200));
            var root = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
            Assert.That(root.GetProperty("status_code").GetInt32(), Is.EqualTo(200));
            Assert.That(root.GetProperty("message").GetString(), Is.EqualTo("inserted"));
        }

        [Test]
        public async Task InsertSessions_ServiceFailure_Returns400WithServiceMessage()
        {
            _sessionService.Setup(s => s.InsertSessions(7)).ReturnsAsync(new Response { HasSuccess = false, Message = "Meeting not found" });

            using var response = await PostAsync("/api/Session/insert-sessions", "{\"meeting_key\":7}");

            await AssertErrorBodyAsync(response, 400, "Meeting not found");
        }

        [Test]
        public async Task InsertSessions_ServiceThrows_Returns400WithoutExceptionText()
        {
            _sessionService.Setup(s => s.InsertSessions(7)).ThrowsAsync(new Exception("connection string secret"));

            using var response = await PostAsync("/api/Session/insert-sessions", "{\"meeting_key\":7}");

            await AssertErrorBodyAsync(response, 400, ApiErrorMessages.RequestFailed, "connection string secret");
        }

        [Test]
        public async Task InsertSessions_InvalidBinding_Returns400ErrorViewModel()
        {
            using var response = await PostAsync("/api/Session/insert-sessions", "{\"meeting_key\":\"abc\"}");

            await AssertErrorBodyAsync(response, 400, ApiErrorMessages.InvalidRequest);
        }

        [Test]
        public async Task InsertSessions_MalformedJson_Returns400ErrorViewModel()
        {
            using var response = await PostAsync("/api/Session/insert-sessions", "{\"meeting_key\":");

            await AssertErrorBodyAsync(response, 400, ApiErrorMessages.InvalidRequest);
        }

        [Test]
        public async Task InsertSessions_MissingBody_Returns400ErrorViewModel()
        {
            using var response = await PostAsync("/api/Session/insert-sessions", string.Empty);

            await AssertErrorBodyAsync(response, 400, ApiErrorMessages.InvalidRequest);
        }

        [Test]
        public async Task InsertSessions_UnsupportedMediaType_Returns415ErrorViewModel()
        {
            using var content = new StringContent("meeting_key=7", Encoding.UTF8, "text/plain");

            using var response = await _host.Client.PostAsync("/api/Session/insert-sessions", content);

            await AssertErrorBodyAsync(response, 415, ApiErrorMessages.UnsupportedMediaType);
        }

        [Test]
        public async Task UnknownApiRoute_Returns404ErrorViewModel()
        {
            using var response = await _host.Client.GetAsync("/api/DoesNotExist");

            await AssertErrorBodyAsync(response, 404, ApiErrorMessages.ResourceNotFound);
        }

        [Test]
        public async Task WrongHttpMethod_Returns405ErrorViewModel()
        {
            using var content = new StringContent("{\"meeting_key\":7}", Encoding.UTF8, "application/json");

            using var response = await _host.Client.PutAsync("/api/Session/insert-sessions", content);

            await AssertErrorBodyAsync(response, 405, ApiErrorMessages.MethodNotAllowed);
        }

        [Test]
        public async Task GetMeeting_NullItem_Returns404WithServiceMessage()
        {
            _meetingService.Setup(s => s.GetMeetingByKey(5)).ReturnsAsync(new SingleResponse<Meeting> { Item = null!, Message = "Meeting 5 not found" });

            using var response = await _host.Client.GetAsync("/api/Meeting/5");

            await AssertErrorBodyAsync(response, 404, "Meeting 5 not found");
        }

        [Test]
        public async Task GetMeeting_ServiceThrows_Returns400WithoutExceptionText()
        {
            _meetingService.Setup(s => s.GetMeetingByKey(5)).ThrowsAsync(new Exception("stack trace detail"));

            using var response = await _host.Client.GetAsync("/api/Meeting/5");

            await AssertErrorBodyAsync(response, 400, ApiErrorMessages.RequestFailed, "stack trace detail");
        }

        [Test]
        public async Task GetSession_UnhandledException_Returns500WithoutExceptionText()
        {
            _sessionService.Setup(s => s.GetSessionByMeetingKeySessionKey(1, 3)).ThrowsAsync(new InvalidOperationException("internal state detail"));

            using var response = await _host.Client.GetAsync("/api/Session/3?meetingkey=1");

            await AssertErrorBodyAsync(response, 500, ApiErrorMessages.UnexpectedError, "internal state detail");
        }

        private Task<HttpResponseMessage> PostAsync(string path, string json)
        {
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return _host.Client.PostAsync(path, content);
        }

        private static async Task AssertErrorBodyAsync(HttpResponseMessage response, int expectedStatus, string expectedMessage, params string[] hiddenDetails)
        {
            Assert.That((int)response.StatusCode, Is.EqualTo(expectedStatus));
            Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));

            var raw = await response.Content.ReadAsStringAsync();
            var error = JsonSerializer.Deserialize<ErrorViewModel>(raw);

            Assert.That(error, Is.Not.Null);
            Assert.That(error!.StatusCode, Is.EqualTo(expectedStatus));
            Assert.That(error.Message, Is.EqualTo(expectedMessage));
            foreach (var detail in hiddenDetails)
                Assert.That(raw, Does.Not.Contain(detail));
        }
    }
}
