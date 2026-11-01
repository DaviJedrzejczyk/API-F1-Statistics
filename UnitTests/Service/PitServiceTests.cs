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
        public async Task GetPitsBySessionKeyApi_ClientFailure_DoesNotCommit()
        {
            _unityMock.Setup(x => x.GetImportWindow(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((global::Dao.Cache.ImportWindow?)null);
            _pitClientMock.Setup(x => x.GetAllPitsSession(3, It.IsAny<CancellationToken>())).ReturnsAsync(new DataResponse<Pit> { HasSuccess = false, Message = "api fail" });
            var result = await new PitService(_unityMock.Object, _pitClientMock.Object).GetPitsBySessionKeyApi(3);
            Assert.That(result.HasSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("api fail"));
            _unityMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task GetPitsBySessionKeyApi_LegacyPartialRows_ImportsMissingAndReturnsFullWindow()
        {
            var oldPit = new Pit { SessionKey = 8, DriverNumber = 1, Date = new DateTime(2024, 1, 1) };
            var newPit = new Pit { SessionKey = 8, DriverNumber = 2, Date = new DateTime(2024, 1, 1) };
            _unityMock.Setup(x => x.GetImportWindow(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((global::Dao.Cache.ImportWindow?)null);
            _unityMock.Setup(x => x.StageImportWindow(It.IsAny<global::Dao.Cache.ImportWindow>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var transaction = new Mock<IImportTransaction>();
            transaction.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _unityMock.Setup(x => x.BeginImport("pits", "8", It.IsAny<CancellationToken>())).ReturnsAsync(transaction.Object);
            _pitDaoMock.Setup(x => x.GetAllPitsBySessionKey(8, It.IsAny<CancellationToken>())).ReturnsAsync(new DataResponse<Pit> { HasSuccess = true, Itens = new() { oldPit } });
            _pitClientMock.Setup(x => x.GetAllPitsSession(8, It.IsAny<CancellationToken>())).ReturnsAsync(new DataResponse<Pit> { HasSuccess = true, Itens = new() { oldPit, newPit } });
            _pitDaoMock.Setup(x => x.SavePits(It.IsAny<List<Pit>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response { HasSuccess = true });
            _unityMock.Setup(x => x.Commit(It.IsAny<CancellationToken>())).ReturnsAsync(new Response { HasSuccess = true });
            var result = await new PitService(_unityMock.Object, _pitClientMock.Object).GetPitsBySessionKeyApi(8);
            Assert.That(result.HasSuccess, Is.True);
            Assert.That(result.Itens.Count, Is.EqualTo(2));
            _pitDaoMock.Verify(x => x.SavePits(It.Is<List<Pit>>(p => p.Count == 1 && p[0] == newPit), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}