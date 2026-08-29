using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dao.Interface;
using Entities;
using ExternalApi.Interfaces;
using Moq;
using NUnit.Framework;
using Services.Impl;
using Shared.Responses;

namespace UnitTests.Service
{
    [TestFixture]
    public class SessionResultServiceTests
    {
        private Mock<IUnityOfWork> _unityMock = null!;
        private Mock<ISessionResultClient> _sessionResultClientMock = null!;
        private Mock<ISessionResultDao> _sessionResultDaoMock = null!;

        [SetUp]
        public void SetUp()
        {
            _unityMock = new Mock<IUnityOfWork>();
            _sessionResultClientMock = new Mock<ISessionResultClient>();
            _sessionResultDaoMock = new Mock<ISessionResultDao>();

            _unityMock.Setup(u => u.SessionResultDao).Returns(_sessionResultDaoMock.Object);
        }

        [Test]
        public void Constructor_WithValidDependencies_DoesNotThrow()
        {
            // Arrange / Act / Assert
            Assert.DoesNotThrow(() => new SessionResultService(_sessionResultClientMock.Object, _unityMock.Object));
        }

        [Test]
        public async Task GetSessionResultBySessionKeyApi_DatabaseHasItems_ReturnsDatabaseResponse()
        {
            // Arrange
            int sessionKey = 1;
            var dbList = new List<SessionResult> { new() { SessionKey = sessionKey } };
            var dbResponse = new DataResponse<SessionResult> { HasSuccess = true, Itens = dbList };
            _sessionResultDaoMock.Setup(d => d.GetSessionResultsBySesssionKey(sessionKey)).ReturnsAsync(dbResponse);

            var service = new SessionResultService(_sessionResultClientMock.Object, _unityMock.Object);

            // Act
            var result = await service.GetSessionResultBySessionKeyApi(sessionKey);

            // Assert
            Assert.IsTrue(result.HasSuccess);
            Assert.That(result.Itens, Is.SameAs(dbList));
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

            var clientResponse = new DataResponse<SessionResult> { HasSuccess = false, Message = "api fail" };
            _sessionResultClientMock.Setup(c => c.GetSessionResultApi(sessionKey)).ReturnsAsync(clientResponse);

            var service = new SessionResultService(_sessionResultClientMock.Object, _unityMock.Object);

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

            var clientResponse = new DataResponse<SessionResult> { HasSuccess = false, Message = "api fail" };
            _sessionResultClientMock.Setup(c => c.GetSessionResultApi(sessionKey)).ReturnsAsync(clientResponse);

            var service = new SessionResultService(_sessionResultClientMock.Object, _unityMock.Object);

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

            var clientItems = new List<SessionResult> { new() { SessionKey = sessionKey } };
            var clientResponse = new DataResponse<SessionResult> { HasSuccess = true, Itens = clientItems, Message = "client ok" };
            _sessionResultClientMock.Setup(c => c.GetSessionResultApi(sessionKey)).ReturnsAsync(clientResponse);

            var saveResponse = new Response { HasSuccess = false, Message = "insert failed" };
            _sessionResultDaoMock.Setup(d => d.SaveSessionResults(It.IsAny<List<SessionResult>>())).ReturnsAsync(saveResponse);

            var service = new SessionResultService(_sessionResultClientMock.Object, _unityMock.Object);

            // Act
            var result = await service.GetSessionResultBySessionKeyApi(sessionKey);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("client ok"));
            _sessionResultDaoMock.Verify(d => d.SaveSessionResults(It.Is<List<SessionResult>>(l => l == clientItems)), Times.Once);
        }

        [Test]
        public async Task GetSessionResultBySessionKeyApi_ClientReturnsItems_SaveSucceeds_ReturnsClientResponse()
        {
            // Arrange
            int sessionKey = 5;
            var dbResponse = new DataResponse<SessionResult> { HasSuccess = true, Itens = new List<SessionResult>() };
            _sessionResultDaoMock.Setup(d => d.GetSessionResultsBySesssionKey(sessionKey)).ReturnsAsync(dbResponse);

            var clientItems = new List<SessionResult> { new() { SessionKey = sessionKey } };
            var clientResponse = new DataResponse<SessionResult> { HasSuccess = true, Itens = clientItems };
            _sessionResultClientMock.Setup(c => c.GetSessionResultApi(sessionKey)).ReturnsAsync(clientResponse);

            var saveResponse = new Response { HasSuccess = true };
            _sessionResultDaoMock.Setup(d => d.SaveSessionResults(It.IsAny<List<SessionResult>>())).ReturnsAsync(saveResponse);
            var commitResponse = new Response { HasSuccess = true, Message = "committed" };
            _unityMock.Setup(u => u.Commit()).ReturnsAsync(commitResponse);

            var service = new SessionResultService(_sessionResultClientMock.Object, _unityMock.Object);

            // Act
            var result = await service.GetSessionResultBySessionKeyApi(sessionKey);

            // Assert
            Assert.IsTrue(result.HasSuccess);
            Assert.That(result.Itens, Is.SameAs(clientItems));
            _sessionResultDaoMock.Verify(d => d.SaveSessionResults(It.Is<List<SessionResult>>(l => l == clientItems)), Times.Once);
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

            var service = new SessionResultService(_sessionResultClientMock.Object, _unityMock.Object);

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

            var service = new SessionResultService(_sessionResultClientMock.Object, _unityMock.Object);

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

            var service = new SessionResultService(_sessionResultClientMock.Object, _unityMock.Object);

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

            var service = new SessionResultService(_sessionResultClientMock.Object, _unityMock.Object);

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

            var service = new SessionResultService(_sessionResultClientMock.Object, _unityMock.Object);

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
            // Arrange
            var data = new List<SessionResult> { new() };
            var saveResponse = new Response { HasSuccess = true };
            var commitResponse = new Response { HasSuccess = true, Message = "committed" };
            _sessionResultDaoMock.Setup(d => d.SaveSessionResults(It.IsAny<List<SessionResult>>())).ReturnsAsync(saveResponse);
            _unityMock.Setup(u => u.Commit()).ReturnsAsync(commitResponse);

            var service = new SessionResultService(_sessionResultClientMock.Object, _unityMock.Object);

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

            var service = new SessionResultService(_sessionResultClientMock.Object, _unityMock.Object);

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
