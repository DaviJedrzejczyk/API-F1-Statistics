using Dao.Interface;
using Entities;
using Entities.Class;
using Entities.Dtos.SessionResultDTOs;
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
        private SessionResultService service = null!;

        [SetUp]
        public void SetUp()
        {
            _unityMock = new Mock<IUnityOfWork>();
            _sessionResultClientMock = new Mock<ISessionResultClient>();
            _sessionResultDaoMock = new Mock<ISessionResultDao>();
            _sessionResultQualifyingsMock = new Mock<ISessionResultQualifyingsService>();
            _stintServiceMock = new Mock<IStintService>();

            _unityMock.Setup(u => u.SessionResultDao).Returns(_sessionResultDaoMock.Object);

            service = new SessionResultService(_sessionResultClientMock.Object, _unityMock.Object, _sessionResultQualifyingsMock.Object, _stintServiceMock.Object);
        }

        [Test]
        public void Constructor_WithValidDependencies_DoesNotThrow()
        {
            // Arrange / Act / Assert
            Assert.DoesNotThrow(() => new SessionResultService(_sessionResultClientMock.Object, _unityMock.Object, _sessionResultQualifyingsMock.Object, _stintServiceMock.Object));
        }

        [Test]
        public async Task GetSessionResultBySessionKeyApi_DatabaseHasItems_ReturnsDatabaseResponse()
        {
            // Arrange
            int sessionKey = 1;
            var dbList = new List<SessionResult> { new() { SessionKey = sessionKey } };
            var dbResponse = new DataResponse<SessionResult> { HasSuccess = true, Itens = dbList };
            _sessionResultDaoMock.Setup(d => d.GetSessionResultsBySesssionKey(sessionKey)).ReturnsAsync(dbResponse);

            // Act
            var result = await service.GetSessionResultBySessionKeyApi(sessionKey);

            // Assert
            Assert.IsTrue(result.HasSuccess);
            Assert.That(result.Itens, Is.EqualTo(dbList));
            _sessionResultClientMock.Verify(c => c.GetSessionResultApi(It.IsAny<int>()), Times.Never);
        }

        [Test]
        public async Task GetSessionResultBySessionKeyApi_DatabaseFailureWithException_CallsClientAndReturnsClientFailure()
        {
            // Arrange
            int sessionKey = 2;
            var ex = new InvalidOperationException("db error");
            var dbResponse = new DataResponse<SessionResult> { HasSuccess = false, Exception = ex };
            _sessionResultDaoMock.Setup(d => d.GetSessionResultsBySesssionKey(sessionKey)).ReturnsAsync(dbResponse);

            var clientResponse = new DataResponse<SessionResultDto> { HasSuccess = false, Message = "api fail" };
            _sessionResultClientMock.Setup(c => c.GetSessionResultApi(sessionKey)).ReturnsAsync(clientResponse);

            // Act
            var result = await service.GetSessionResultBySessionKeyApi(sessionKey);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("api fail"));
            _sessionResultClientMock.Verify(c => c.GetSessionResultApi(sessionKey), Times.Once);
        }

        [Test]
        public async Task GetSessionResultBySessionKeyApi_ClientReturnsFailure_ReturnsFailureDataResponse()
        {
            // Arrange
            int sessionKey = 3;
            var dbResponse = new DataResponse<SessionResult> { HasSuccess = true, Itens = new List<SessionResult>() };
            _sessionResultDaoMock.Setup(d => d.GetSessionResultsBySesssionKey(sessionKey)).ReturnsAsync(dbResponse);

            var clientResponse = new DataResponse<SessionResultDto> { HasSuccess = false, Message = "api fail" };
            _sessionResultClientMock.Setup(c => c.GetSessionResultApi(sessionKey)).ReturnsAsync(clientResponse);

            // Act
            var result = await service.GetSessionResultBySessionKeyApi(sessionKey);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("api fail"));
            _sessionResultDaoMock.Verify(d => d.GetSessionResultsBySesssionKey(sessionKey), Times.Once);
        }

        [Test]
        public async Task GetSessionResultBySessionKeyApi_ClientReturnsItems_SaveFails_ReturnsFailureDataResponse()
        {
            // Arrange
            int sessionKey = 4;
            var dbResponse = new DataResponse<SessionResult> { HasSuccess = true, Itens = new List<SessionResult>() };
            _sessionResultDaoMock.Setup(d => d.GetSessionResultsBySesssionKey(sessionKey)).ReturnsAsync(dbResponse);

            var clientItems = new List<SessionResultDto> { new() { SessionKey = sessionKey } };
            var clientResponse = new DataResponse<SessionResultDto> { HasSuccess = true, Itens = clientItems, Message = "client ok" };
            _sessionResultClientMock.Setup(c => c.GetSessionResultApi(sessionKey)).ReturnsAsync(clientResponse);

            var saveResponse = new Response { HasSuccess = false, Message = "insert failed" };
            _sessionResultDaoMock.Setup(d => d.SaveSessionResults(It.IsAny<List<SessionResult>>())).ReturnsAsync(saveResponse);


            // Act
            var result = await service.GetSessionResultBySessionKeyApi(sessionKey);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("insert failed"));
            _sessionResultDaoMock.Verify(d => d.SaveSessionResults(It.Is<List<SessionResult>>(l => l.Count == clientItems.Count && l[0].SessionKey == sessionKey)), Times.Once);
        }

        [Test]
        public async Task GetSessionResultBySessionKeyApi_ClientReturnsItems_SaveSucceeds_ReturnsClientResponse()
        {
            // Arrange
            int sessionKey = 5;
            var dbResponse = new DataResponse<SessionResult> { HasSuccess = true, Itens = new List<SessionResult>() };
            _sessionResultDaoMock.Setup(d => d.GetSessionResultsBySesssionKey(sessionKey)).ReturnsAsync(dbResponse);

            var clientItems = new List<SessionResultDto> { new() { SessionKey = sessionKey } };
            var clientResponse = new DataResponse<SessionResultDto> { HasSuccess = true, Itens = clientItems };
            _sessionResultClientMock.Setup(c => c.GetSessionResultApi(sessionKey)).ReturnsAsync(clientResponse);

            var saveResponse = new Response { HasSuccess = true };
            _sessionResultDaoMock.Setup(d => d.SaveSessionResults(It.IsAny<List<SessionResult>>())).ReturnsAsync(saveResponse);
            var commitResponse = new Response { HasSuccess = true, Message = "committed" };
            _unityMock.Setup(u => u.Commit()).ReturnsAsync(commitResponse);


            // Act
            var result = await service.GetSessionResultBySessionKeyApi(sessionKey);

            // Assert
            Assert.IsTrue(result.HasSuccess);
            Assert.That(result.Itens.Count, Is.EqualTo(clientItems.Count));
            Assert.That(result.Itens[0].SessionKey, Is.EqualTo(sessionKey));
            _sessionResultDaoMock.Verify(d => d.SaveSessionResults(It.Is<List<SessionResult>>(l => l.Count == clientItems.Count && l[0].SessionKey == sessionKey)), Times.Once);
            _unityMock.Verify(u => u.Commit(), Times.Once);
        }

        [Test]
        public async Task GetSessionResultBySessionKeyApi_ClientThrows_ReturnsFailureWithException()
        {
            // Arrange
            int sessionKey = 6;
            var dbResponse = new DataResponse<SessionResult> { HasSuccess = true, Itens = new List<SessionResult>() };
            _sessionResultDaoMock.Setup(d => d.GetSessionResultsBySesssionKey(sessionKey)).ReturnsAsync(dbResponse);
            _sessionResultClientMock.Setup(c => c.GetSessionResultApi(It.IsAny<int>())).ThrowsAsync(new InvalidOperationException("boom"));

            // Act
            var result = await service.GetSessionResultBySessionKeyApi(sessionKey);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.IsNotNull(result.Exception);
            Assert.IsInstanceOf<InvalidOperationException>(result.Exception);
        }

        [Test]
        public async Task GetSessionResultBySessionKeyDatabase_DaoFailure_ReturnsFailureResponse()
        {
            // Arrange
            int sessionKey = 7;
            var dbFail = new DataResponse<SessionResult> { HasSuccess = false, Message = "dbfail" };
            _sessionResultDaoMock.Setup(d => d.GetSessionResultsBySesssionKey(sessionKey)).ReturnsAsync(dbFail);

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
        public async Task GetSessionResultBySessionKeyApi_ClientReturnsQualifyingItems_QualisExist_SaveSucceeds_ReturnsQualifyingResults()
        {
            // Arrange
            int sessionKey = 10;
            var dbResponse = new DataResponse<SessionResult> { HasSuccess = true, Itens = new List<SessionResult>() };
            _sessionResultDaoMock.Setup(d => d.GetSessionResultsBySesssionKey(sessionKey)).ReturnsAsync(dbResponse);

            var clientItem = new SessionResultDto
            {
                SessionKey = sessionKey,
                Duration = new List<double> { 60.0, 61.0 },
                DriverNumber = 7,
                Dnf = false,
                Dns = false,
                Dsq = false,
                GapToLeader = "0",
                NumberOfLaps = 10,
                MeetingKey = 100,
                Position = 1
            };

            var clientResponse = new DataResponse<SessionResultDto> { HasSuccess = true, Itens = new List<SessionResultDto> { clientItem } };
            _sessionResultClientMock.Setup(c => c.GetSessionResultApi(sessionKey)).ReturnsAsync(clientResponse);

            var qualis = new List<SessionResultQualify>
            {
                new SessionResultQualify { SessionKey = sessionKey, DriverNumber = 7, Duration = 60.0, MeetingKey = 100, QualifyingPhase = "Q1" }
            };

            var qualisResponse = new DataResponse<SessionResultQualify> { HasSuccess = true, Itens = qualis };
            _sessionResultQualifyingsMock.Setup(q => q.GetQualyBySessionKey(sessionKey)).ReturnsAsync(qualisResponse);

            // ensure stint service is mocked to avoid awaiting a null Task
            var stintsResponse = new DataResponse<Stint> { HasSuccess = true, Itens = new List<Stint>() };
            _stintServiceMock.Setup(s => s.GetStintsBySessionKey(sessionKey)).ReturnsAsync(stintsResponse);

            var saveResponse = new Response { HasSuccess = true };
            var commitResponse = new Response { HasSuccess = true };
            _sessionResultDaoMock.Setup(d => d.SaveSessionResults(It.IsAny<List<SessionResult>>())).ReturnsAsync(saveResponse);
            _unityMock.Setup(u => u.Commit()).ReturnsAsync(commitResponse);

            // Act
            var result = await service.GetSessionResultBySessionKeyApi(sessionKey);

            // Assert
            Assert.IsTrue(result.HasSuccess);
            Assert.IsNotNull(result.Itens);
            Assert.That(result.Itens.Count, Is.EqualTo(qualis.Count));
            _sessionResultDaoMock.Verify(d => d.SaveSessionResults(It.IsAny<List<SessionResult>>()), Times.Once);
            _unityMock.Verify(u => u.Commit(), Times.Once);
        }

        [Test]
        public async Task GetSessionResultBySessionKeyApi_ClientReturnsQualifyingItems_CreateQualisFails_ReturnsFailure()
        {
            // Arrange
            int sessionKey = 11;
            var dbResponse = new DataResponse<SessionResult> { HasSuccess = true, Itens = new List<SessionResult>() };
            _sessionResultDaoMock.Setup(d => d.GetSessionResultsBySesssionKey(sessionKey)).ReturnsAsync(dbResponse);

            var clientItem = new SessionResultDto
            {
                SessionKey = sessionKey,
                Duration = new List<double> { 60.0, 61.0 },
                DriverNumber = 8
            };

            var clientResponse = new DataResponse<SessionResultDto> { HasSuccess = true, Itens = new List<SessionResultDto> { clientItem } };
            _sessionResultClientMock.Setup(c => c.GetSessionResultApi(sessionKey)).ReturnsAsync(clientResponse);

            // return empty qualis to force CreateListResultQualyfing
            var emptyQualis = new DataResponse<SessionResultQualify> { HasSuccess = true, Itens = new List<SessionResultQualify>() };
            _sessionResultQualifyingsMock.Setup(q => q.GetQualyBySessionKey(sessionKey)).ReturnsAsync(emptyQualis);

            var createFail = new DataResponse<SessionResultQualify> { HasSuccess = false, Message = "qualify create failed" };
            _sessionResultQualifyingsMock.Setup(q => q.CreateListResultQualyfing(It.IsAny<List<SessionResultDto>>())).ReturnsAsync(createFail);

            // Act
            var result = await service.GetSessionResultBySessionKeyApi(sessionKey);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("qualify create failed"));
            _sessionResultDaoMock.Verify(d => d.SaveSessionResults(It.IsAny<List<SessionResult>>()), Times.Never);
        }

        [Test]
        public async Task GetSessionResultBySessionKeyApi_ClientReturnsQualifyingItems_SaveFails_ReturnsFailureDataResponse()
        {
            // Arrange
            int sessionKey = 12;
            var dbResponse = new DataResponse<SessionResult> { HasSuccess = true, Itens = new List<SessionResult>() };
            _sessionResultDaoMock.Setup(d => d.GetSessionResultsBySesssionKey(sessionKey)).ReturnsAsync(dbResponse);

            var clientItem = new SessionResultDto
            {
                SessionKey = sessionKey,
                Duration = new List<double> { 60.0, 61.0 },
                DriverNumber = 9
            };

            var clientResponse = new DataResponse<SessionResultDto> { HasSuccess = true, Itens = new List<SessionResultDto> { clientItem } };
            _sessionResultClientMock.Setup(c => c.GetSessionResultApi(sessionKey)).ReturnsAsync(clientResponse);

            var qualis = new List<SessionResultQualify>
            {
                new SessionResultQualify { SessionKey = sessionKey, DriverNumber = 9, Duration = 60.0 }
            };

            var qualisResponse = new DataResponse<SessionResultQualify> { HasSuccess = true, Itens = qualis };
            _sessionResultQualifyingsMock.Setup(q => q.GetQualyBySessionKey(sessionKey)).ReturnsAsync(qualisResponse);

            var stintsResponse = new DataResponse<Stint> { HasSuccess = true, Itens = new List<Stint>() };
            _stintServiceMock.Setup(s => s.GetStintsBySessionKey(sessionKey)).ReturnsAsync(stintsResponse);

            var saveResponse = new Response { HasSuccess = false, Message = "qualify save failed" };
            _sessionResultDaoMock.Setup(d => d.SaveSessionResults(It.IsAny<List<SessionResult>>())).ReturnsAsync(saveResponse);

            // Act
            var result = await service.GetSessionResultBySessionKeyApi(sessionKey);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("qualify save failed"));
            _sessionResultDaoMock.Verify(d => d.SaveSessionResults(It.IsAny<List<SessionResult>>()), Times.Once);
        }

        [Test]
        public async Task SaveSessionResults_SaveSucceeds_CommitsAndReturnsCommitResponse()
        {
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
