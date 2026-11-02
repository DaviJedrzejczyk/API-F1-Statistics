using AutoMapper;
using Dao.Interface;
using Entities.Class;
using Entities.Dtos;
using ExternalApi.Interfaces;
using Moq;
using Services.Impl;
using Services.Interfaces;
using Shared.Responses;

namespace UnitTests.Service
{
    [TestFixture]
    public class SessionResultServiceTests
    {
        private Mock<IUnityOfWork> _unityMock = null!;
        private Mock<ISessionResultClient> _sessionResultClientMock = null!;
        private Mock<ISessionResultDao> _sessionResultDaoMock = null!;
        private Mock<ISessionResultQualifyingsService> _sessionResultQualifyingsMock = null!;
        private Mock<IStintService> _stintServiceMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private SessionResultService service = null!;
        [SetUp]
        public void Setup()
        {
            _unityMock = new Mock<IUnityOfWork>();
            _sessionResultDaoMock = new Mock<ISessionResultDao>();
            _sessionResultClientMock = new Mock<ISessionResultClient>();
            _sessionResultQualifyingsMock = new Mock<ISessionResultQualifyingsService>();
            _stintServiceMock = new Mock<IStintService>();
            _mapperMock = new Mock<IMapper>();
            _unityMock.Setup(x => x.SessionResultDao).Returns(_sessionResultDaoMock.Object);
            service = new SessionResultService(_sessionResultClientMock.Object, _unityMock.Object, _sessionResultQualifyingsMock.Object, _stintServiceMock.Object, Mock.Of<ILapService>(), _mapperMock.Object);
        }

        [Test]
        public async Task GetSessionResultBySessionKeyApi_ValidWindowSkipsUpstream()
        {
            var data = new List<SessionResult> { new() { SessionKey = 1, DriverNumber = 1 } };
            _unityMock.Setup(x => x.GetImportWindow("session_results", "1", It.IsAny<CancellationToken>())).ReturnsAsync(new global::Dao.Cache.ImportWindow { IsComplete = true, CompletedAtUtc = DateTimeOffset.UtcNow.AddMinutes(-1), ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(1), Payload = System.Text.Json.JsonSerializer.Serialize(data) });
            var result = await service.GetSessionResultBySessionKeyApi(1);
            Assert.That(result.HasSuccess, Is.True);
            Assert.That(result.Itens.Single().DriverNumber, Is.EqualTo(1));
            _sessionResultClientMock.Verify(x => x.GetSessionResultApi(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task GetSessionResultBySessionKeyApi_UpstreamFailureDoesNotWrite()
        {
            _unityMock.Setup(x => x.GetImportWindow(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((global::Dao.Cache.ImportWindow?)null);
            _sessionResultClientMock.Setup(x => x.GetSessionResultApi(2, It.IsAny<CancellationToken>())).ReturnsAsync(new DataResponse<SessionResultDto> { HasSuccess = false, Message = "api fail" });
            var result = await service.GetSessionResultBySessionKeyApi(2);
            Assert.That(result.HasSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("api fail"));
            _unityMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task GetSessionResultBySessionKeyDatabase_DaoFailure_ReturnsFailureResponse()
        {
            // Arrange
            int sessionKey = 7;
            var dbFail = new DataResponse<SessionResult> { HasSuccess = false, Message = "dbfail" };

            var sessionResultDaoMock = new Mock<ISessionResultDao>();
            sessionResultDaoMock.Setup(d => d.GetSessionResultsBySesssionKey(sessionKey)).ReturnsAsync(dbFail);

            var unityMock = new Mock<IUnityOfWork>();
            unityMock.Setup(u => u.SessionResultDao).Returns(sessionResultDaoMock.Object);

            var sessionResultClientMock = new Mock<ISessionResultClient>();
            var sessionResultQualifyingsMock = new Mock<ISessionResultQualifyingsService>();
            var stintServiceMock = new Mock<IStintService>();
            var lapServiceMock = new Mock<ILapService>();
            var mapperMock = new Mock<IMapper>();

            var service = new SessionResultService(sessionResultClientMock.Object, unityMock.Object, sessionResultQualifyingsMock.Object, stintServiceMock.Object, lapServiceMock.Object, mapperMock.Object);

            // Act
            var result = await service.GetSessionResultBySessionKeyDatabase(sessionKey);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("An error has ocurred to found the results of this session."));
        }

        [Test]
        public async Task GetSessionResultBySessionKeyDatabase_NoItems_ReturnsNotFoundResponse()
        {
            // Arrange
            int sessionKey = 8;
            var dbEmpty = new DataResponse<SessionResult> { HasSuccess = true, Itens = new List<SessionResult>() };
            _sessionResultDaoMock.Setup(d => d.GetSessionResultsBySesssionKey(sessionKey)).ReturnsAsync(dbEmpty);

            // Act
            var result = await service.GetSessionResultBySessionKeyDatabase(sessionKey);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("Not found the results of this session."));
        }

        [Test]
        public async Task GetSessionResultBySessionKeyDatabase_DaoThrows_ReturnsFailureWithException()
        {
            // Arrange
            int sessionKey = 9;
            _sessionResultDaoMock.Setup(d => d.GetSessionResultsBySesssionKey(It.IsAny<int>())).ThrowsAsync(new InvalidOperationException("dberr"));

            // Act
            var result = await service.GetSessionResultBySessionKeyDatabase(sessionKey);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.IsNotNull(result.Exception);
            Assert.That(result.Exception.Message, Is.EqualTo("dberr"));
        }

        [Test]
        public async Task SaveSessionResults_SaveFails_ReturnsSaveResponse()
        {
            // Arrange
            var data = new List<SessionResult> { new() };
            var saveResponse = new Response { HasSuccess = false, Message = "save failed" };
            _sessionResultDaoMock.Setup(d => d.SaveSessionResults(It.IsAny<List<SessionResult>>())).ReturnsAsync(saveResponse);

            // Act
            var result = await service.SaveSessionResults(data);

            // Assert
            Assert.That(result, Is.SameAs(saveResponse));
            _sessionResultDaoMock.Verify(d => d.SaveSessionResults(It.Is<List<SessionResult>>(l => l == data)), Times.Once);
            _unityMock.Verify(u => u.Commit(), Times.Never);
        }

        [Test]
        public async Task SaveSessionResults_SaveSucceeds_CommitsAndReturnsCommitResponse()
        {
            // initialize mocks and service (missing class SetUp)
            _sessionResultDaoMock = new Mock<ISessionResultDao>();
            _unityMock = new Mock<IUnityOfWork>();
            _sessionResultClientMock = new Mock<ISessionResultClient>();
            _sessionResultQualifyingsMock = new Mock<ISessionResultQualifyingsService>();
            _stintServiceMock = new Mock<IStintService>();
            var lapServiceMock = new Mock<ILapService>();
            _mapperMock = new Mock<IMapper>();
            _unityMock.Setup(u => u.SessionResultDao).Returns(_sessionResultDaoMock.Object);
            service = new SessionResultService(_sessionResultClientMock.Object, _unityMock.Object, _sessionResultQualifyingsMock.Object, _stintServiceMock.Object, lapServiceMock.Object, _mapperMock.Object);

            // Arrange
            var data = new List<SessionResult> { new() };
            var saveResponse = new Response { HasSuccess = true };
            var commitResponse = new Response { HasSuccess = true, Message = "committed" };
            _sessionResultDaoMock.Setup(d => d.SaveSessionResults(It.IsAny<List<SessionResult>>())).ReturnsAsync(saveResponse);
            _unityMock.Setup(u => u.Commit()).ReturnsAsync(commitResponse);

            // Act
            var result = await service.SaveSessionResults(data);

            // Assert
            Assert.That(result, Is.SameAs(commitResponse));
            _sessionResultDaoMock.Verify(d => d.SaveSessionResults(It.Is<List<SessionResult>>(l => l == data)), Times.Once);
            _unityMock.Verify(u => u.Commit(), Times.Once);
        }

        [Test]
        public async Task SaveSessionResults_DaoThrows_ReturnsFailureResponseWithException()
        {
            // Arrange
            var data = new List<SessionResult> { new() };
            _sessionResultDaoMock.Setup(d => d.SaveSessionResults(It.IsAny<List<SessionResult>>())).ThrowsAsync(new InvalidOperationException("boom"));

            // Act
            var result = await service.SaveSessionResults(data);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.IsNotNull(result.Exception);
            Assert.IsInstanceOf<InvalidOperationException>(result.Exception);
            _unityMock.Verify(u => u.Commit(), Times.Never);
        }
    }
}
