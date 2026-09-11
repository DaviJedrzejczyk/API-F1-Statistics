using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dao.Interface;
using Entities.Class;
using ExternalApi.Interfaces;
using Moq;
using NUnit.Framework;
using Services.Impl;
using Shared.Responses;

namespace UnitTests.Service
{
    [TestFixture]
    public class PitServiceTests
    {
        private Mock<IUnityOfWork> _unityMock = null!;
        private Mock<IPitClient> _pitClientMock = null!;
        private Mock<IPitDao> _pitDaoMock = null!;

        [SetUp]
        public void SetUp()
        {
            _unityMock = new Mock<IUnityOfWork>();
            _pitClientMock = new Mock<IPitClient>();
            _pitDaoMock = new Mock<IPitDao>();

            _unityMock.Setup(u => u.PitDao).Returns(_pitDaoMock.Object);
        }

        [Test]
        public void Constructor_WithValidDependencies_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => new PitService(_unityMock.Object, _pitClientMock.Object));
        }

        [Test]
        public async Task GetPitsBySessionKeyDb_NoItems_SetsNotFoundMessageAndReturns()
        {
            // Arrange
            var dbResponse = new DataResponse<Pit> { HasSuccess = true, Itens = new List<Pit>() };
            _pitDaoMock.Setup(d => d.GetAllPitsBySessionKey(1)).ReturnsAsync(dbResponse);
            var service = new PitService(_unityMock.Object, _pitClientMock.Object);

            // Act
            var result = await service.GetPitsBySessionKeyDb(1);

            // Assert
            // HasSuccess remains as provided by DAO (true), message should be set to the not-found text
            Assert.IsTrue(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("Pits not found for this session in database."));
            // The method sets the message on the same response instance
            Assert.That(result, Is.SameAs(dbResponse));
        }

        [Test]
        public async Task GetPitsBySessionKeyDb_DaoThrows_ReturnsFailureWithException()
        {
            // Arrange
            _pitDaoMock.Setup(d => d.GetAllPitsBySessionKey(It.IsAny<int>())).ThrowsAsync(new InvalidOperationException("dberr"));
            var service = new PitService(_unityMock.Object, _pitClientMock.Object);

            // Act
            var result = await service.GetPitsBySessionKeyDb(5);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.IsNotNull(result.Exception);
            Assert.That(result.Exception.Message, Is.EqualTo("dberr"));
        }

        [Test]
        public async Task SavePits_SaveFails_ReturnsSaveResponse()
        {
            // Arrange
            var service = new PitService(_unityMock.Object, _pitClientMock.Object);
            var pits = new List<Pit> { new() };
            var saveResponse = new Response { HasSuccess = false, Message = "save failed" };

            _pitDaoMock.Setup(d => d.SavePits(It.IsAny<List<Pit>>())).ReturnsAsync(saveResponse);

            // Act
            var result = await service.SavePits(pits);

            // Assert
            Assert.That(result, Is.SameAs(saveResponse));
            _pitDaoMock.Verify(d => d.SavePits(It.Is<List<Pit>>(l => l == pits)), Times.Once);
            _unityMock.Verify(u => u.Commit(), Times.Never);
        }

        [Test]
        public async Task SavePits_SaveSucceeds_CommitsAndReturnsCommitResponse()
        {
            // Arrange
            var service = new PitService(_unityMock.Object, _pitClientMock.Object);
            var pits = new List<Pit> { new() };
            var saveResponse = new Response { HasSuccess = true };
            var commitResponse = new Response { HasSuccess = true, Message = "committed" };

            _pitDaoMock.Setup(d => d.SavePits(It.IsAny<List<Pit>>())).ReturnsAsync(saveResponse);
            _unityMock.Setup(u => u.Commit()).ReturnsAsync(commitResponse);

            // Act
            var result = await service.SavePits(pits);

            // Assert
            Assert.That(result, Is.SameAs(commitResponse));
            _pitDaoMock.Verify(d => d.SavePits(It.Is<List<Pit>>(l => l == pits)), Times.Once);
            _unityMock.Verify(u => u.Commit(), Times.Once);
        }

        [Test]
        public async Task SavePits_SaveThrows_ReturnsFailureResponseWithException()
        {
            // Arrange
            var service = new PitService(_unityMock.Object, _pitClientMock.Object);
            var pits = new List<Pit> { new() };

            _pitDaoMock.Setup(d => d.SavePits(It.IsAny<List<Pit>>())).ThrowsAsync(new InvalidOperationException("boom"));

            // Act
            var result = await service.SavePits(pits);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.IsNotNull(result.Exception);
            Assert.IsInstanceOf<InvalidOperationException>(result.Exception);
            Assert.That(result.Exception.Message, Is.EqualTo("boom"));
            _pitDaoMock.Verify(d => d.SavePits(It.Is<List<Pit>>(l => l == pits)), Times.Once);
            _unityMock.Verify(u => u.Commit(), Times.Never);
        }

        [Test]
        public async Task GetPitsBySessionKeyApi_DatabaseFailure_ReturnsDatabaseResponse()
        {
            // Arrange
            var dbFail = new DataResponse<Pit> { HasSuccess = false, Message = "dbfail" };
            _pitDaoMock.Setup(d => d.GetAllPitsBySessionKey(10)).ReturnsAsync(dbFail);
            var service = new PitService(_unityMock.Object, _pitClientMock.Object);

            // Act
            var result = await service.GetPitsBySessionKeyApi(10);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            // Because DAO returned an empty/null items the DB method sets a not-found message
            Assert.That(result.Message, Is.EqualTo("Pits not found for this session in database."));
            _pitClientMock.Verify(c => c.GetAllPitsSession(It.IsAny<int>()), Times.Never);
        }

        [Test]
        public async Task GetPitsBySessionKeyApi_ClientFailure_ReturnsClientResponse()
        {
            // Arrange
            var dbResponse = new DataResponse<Pit> { HasSuccess = true, Itens = new List<Pit>() };
            _pitDaoMock.Setup(d => d.GetAllPitsBySessionKey(3)).ReturnsAsync(dbResponse);

            var clientResponse = new DataResponse<Pit> { HasSuccess = false, Message = "api fail" };
            _pitClientMock.Setup(c => c.GetAllPitsSession(3)).ReturnsAsync(clientResponse);

            var service = new PitService(_unityMock.Object, _pitClientMock.Object);

            // Act
            var result = await service.GetPitsBySessionKeyApi(3);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("api fail"));
        }

        [Test]
        public async Task GetPitsBySessionKeyApi_CountsEqual_ReturnsClientDataWithoutInsert()
        {
            // Arrange
            var dbList = new List<Pit> { new() { SessionKey = 1 }, new() { SessionKey = 2 } };
            var dbResponse = new DataResponse<Pit> { HasSuccess = true, Itens = dbList };
            _pitDaoMock.Setup(d => d.GetAllPitsBySessionKey(4)).ReturnsAsync(dbResponse);

            var clientList = new List<Pit> { new() { SessionKey = 1 }, new() { SessionKey = 2 } };
            var clientResponse = new DataResponse<Pit> { HasSuccess = true, Itens = clientList };
            _pitClientMock.Setup(c => c.GetAllPitsSession(4)).ReturnsAsync(clientResponse);

            var service = new PitService(_unityMock.Object, _pitClientMock.Object);

            // Act
            var result = await service.GetPitsBySessionKeyApi(4);

            // Assert
            Assert.IsTrue(result.HasSuccess);
            Assert.That(result.Itens.Count, Is.EqualTo(2));
            _pitDaoMock.Verify(d => d.SavePits(It.IsAny<List<Pit>>()), Times.Never);
        }

        [Test]
        public async Task GetPitsBySessionKeyApi_ClientThrows_ReturnsFailureWithException()
        {
            // Arrange
            _pitDaoMock.Setup(d => d.GetAllPitsBySessionKey(It.IsAny<int>())).ReturnsAsync(new DataResponse<Pit> { HasSuccess = true, Itens = new List<Pit>() });
            _pitClientMock.Setup(c => c.GetAllPitsSession(It.IsAny<int>())).ThrowsAsync(new InvalidOperationException("boom"));
            var service = new PitService(_unityMock.Object, _pitClientMock.Object);

            // Act
            var result = await service.GetPitsBySessionKeyApi(6);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.IsNotNull(result.Exception);
            Assert.IsInstanceOf<InvalidOperationException>(result.Exception);
            Assert.That(result.Exception.Message, Is.EqualTo("boom"));
        }

        [Test]
        public async Task GetPitsBySessionKeyApi_DifferentCounts_SaveFails_ReturnsFailureDataResponse()
        {
            // Arrange
            var dbList = new List<Pit> { new() { SessionKey = 1 } };
            var dbResponse = new DataResponse<Pit> { HasSuccess = true, Itens = dbList };
            _pitDaoMock.Setup(d => d.GetAllPitsBySessionKey(7)).ReturnsAsync(dbResponse);

            var clientList = new List<Pit> { new() { SessionKey = 1 }, new() { SessionKey = 2 } };
            var clientResponse = new DataResponse<Pit> { HasSuccess = true, Itens = clientList };
            _pitClientMock.Setup(c => c.GetAllPitsSession(7)).ReturnsAsync(clientResponse);

            var saveResponse = new Response { HasSuccess = false, Message = "insert failed" };
            _pitDaoMock.Setup(d => d.SavePits(It.IsAny<List<Pit>>())).ReturnsAsync(saveResponse);

            var service = new PitService(_unityMock.Object, _pitClientMock.Object);

            // Act
            var result = await service.GetPitsBySessionKeyApi(7);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("insert failed"));
        }

        [Test]
        public async Task GetPitsBySessionKeyApi_DifferentCounts_SaveSucceeds_InsertsOnlyNewAndReturnsData()
        {
            // Arrange
            var dbList = new List<Pit> { new() { SessionKey = 1 } };
            var dbResponse = new DataResponse<Pit> { HasSuccess = true, Itens = dbList };
            _pitDaoMock.Setup(d => d.GetAllPitsBySessionKey(8)).ReturnsAsync(dbResponse);

            var clientList = new List<Pit> { new() { SessionKey = 1 }, new() { SessionKey = 2 } };
            var clientResponse = new DataResponse<Pit> { HasSuccess = true, Itens = clientList };
            _pitClientMock.Setup(c => c.GetAllPitsSession(8)).ReturnsAsync(clientResponse);

            var saveResponse = new Response { HasSuccess = true };
            _pitDaoMock.Setup(d => d.SavePits(It.IsAny<List<Pit>>())).ReturnsAsync(saveResponse);
            var commitResponse = new Response { HasSuccess = true };
            _unityMock.Setup(u => u.Commit()).ReturnsAsync(commitResponse);

            var service = new PitService(_unityMock.Object, _pitClientMock.Object);

            // Act
            var result = await service.GetPitsBySessionKeyApi(8);

            // Assert
            Assert.IsTrue(result.HasSuccess);
            // After filtering the client data should only contain the new SessionKey = 2
            Assert.That(result.Itens.Count, Is.EqualTo(1));
            Assert.That(result.Itens[0].SessionKey, Is.EqualTo(2));
            // Verify SavePits was called with only the new item
            _pitDaoMock.Verify(d => d.SavePits(It.Is<List<Pit>>(l => l.Count == 1 && l[0].SessionKey == 2)), Times.Once);
        }
    }
}
