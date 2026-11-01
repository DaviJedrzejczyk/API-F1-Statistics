#pragma warning disable CS8625
#pragma warning disable NUnit2005
using Moq;
using Dao.Interface;
using ExternalApi.Interfaces;
using Entities.Class;
using Services.Impl;
using Shared.Responses;

namespace UnitTests.Service
{
    [TestFixture]
    public class RaceControlServiceTests
    {
        [Test]
        public void Constructor_WithValidDependencies_DoesNotThrow()
        {
            // Arrange
            var mockRace = new Mock<IRaceControlClient>();
            var mockUow = new Mock<IUnityOfWork>();

            // Act & Assert
            Assert.DoesNotThrow(() => new RaceControlService(mockRace.Object, mockUow.Object));
        }

        [Test]
        public async Task GetRaceControlsBySessionFlags_UsesCompleteWindowAndOrdersEvents()
        {
            var race = new Mock<IRaceControlClient>();
            var uow = new Mock<IUnityOfWork>();
            var now = DateTime.UtcNow;
            var events = new List<RaceControl> {
                new() { SessionKey = 1, Sector = 1, Flag = "CLEAR", Date = now.AddSeconds(10) },
                new() { SessionKey = 1, Sector = 1, Flag = "YELLOW", Date = now }
            };
            uow.Setup(x => x.GetImportWindow("race_control", It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new global::Dao.Cache.ImportWindow { IsComplete = true, CompletedAtUtc = DateTimeOffset.UtcNow.AddMinutes(-1), ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(1), Payload = System.Text.Json.JsonSerializer.Serialize(events) });
            var result = await new RaceControlService(race.Object, uow.Object).GetRaceControlsBySessionFlags(1, new[] { "YELLOW", "CLEAR" });
            Assert.That(result.HasSuccess, Is.True);
            Assert.That(result.Itens.Single().DateEnd, Is.EqualTo(now.AddSeconds(10)));
            race.Verify(x => x.GetRaceControlsBySessionFlags(It.IsAny<int>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task GetRaceControlsBySessionFlags_UpstreamFailureDoesNotCommit()
        {
            var race = new Mock<IRaceControlClient>();
            var uow = new Mock<IUnityOfWork>();
            uow.Setup(x => x.GetImportWindow(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((global::Dao.Cache.ImportWindow?)null);
            race.Setup(x => x.GetRaceControlsBySessionFlags(1, It.IsAny<string[]>(), It.IsAny<CancellationToken>())).ReturnsAsync(new DataResponse<RaceControl> { HasSuccess = false, Message = "upstream failed" });
            var result = await new RaceControlService(race.Object, uow.Object).GetRaceControlsBySessionFlags(1, new[] { "YELLOW" });
            Assert.That(result.HasSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("upstream failed"));
            uow.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Never);
        }
        [Test]
        public async Task GetRaceControlsBySessionFlagsDb_DaoThrows_ReturnsFailureDataResponse()
        {
            // Arrange
            var mockRace = new Mock<IRaceControlClient>();
            var mockDao = new Mock<IRaceControlDao>();
            var mockUow = new Mock<IUnityOfWork>();

            mockDao.Setup(d => d.GetRaceControlsBySessionFlags(It.IsAny<int>(), It.IsAny<string[]>()))
                   .ThrowsAsync(new InvalidOperationException("db error"));
            mockUow.Setup(u => u.RaceControlDao).Returns(mockDao.Object);

            var service = new RaceControlService(mockRace.Object, mockUow.Object);

            // Act
            var result = await service.GetRaceControlsBySessionFlagsDb(1, Array.Empty<string>());

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasSuccess);
            Assert.IsNotNull(result.Exception);
            Assert.IsTrue(result.Message.Length > 0);
        }

        [Test]
        public async Task GetRaceControlsBySessionFlagsDb_DaoReturnsSuccess_ReturnsSameResponse()
        {
            // Arrange
            var mockRace = new Mock<IRaceControlClient>();
            var mockDao = new Mock<IRaceControlDao>();
            var mockUow = new Mock<IUnityOfWork>();

            var list = new List<RaceControl> { new RaceControl() };
            var dbSuccess = new DataResponse<RaceControl>("ok", true, null, list);
            mockDao.Setup(d => d.GetRaceControlsBySessionFlags(It.IsAny<int>(), It.IsAny<string[]>()))
                   .ReturnsAsync(dbSuccess);
            mockUow.Setup(u => u.RaceControlDao).Returns(mockDao.Object);

            var service = new RaceControlService(mockRace.Object, mockUow.Object);

            // Act
            var result = await service.GetRaceControlsBySessionFlagsDb(2, Array.Empty<string>());

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasSuccess);
            Assert.AreEqual(dbSuccess, result);
        }

        [Test]
        public async Task SaveRaceControls_DaoReturnsFailure_ReturnsSameResponse()
        {
            // Arrange
            var mockRace = new Mock<IRaceControlClient>();
            var mockDao = new Mock<IRaceControlDao>();
            var mockUow = new Mock<IUnityOfWork>();

            var failure = new Response("fail", false, null);
            mockDao.Setup(d => d.SaveRaceControls(It.IsAny<List<RaceControl>>()))
                   .ReturnsAsync(failure);
            mockUow.Setup(u => u.RaceControlDao).Returns(mockDao.Object);

            var service = new RaceControlService(mockRace.Object, mockUow.Object);

            // Act
            var result = await service.SaveRaceControls(new List<RaceControl>());

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasSuccess);
            Assert.AreEqual(failure, result);
        }

        [Test]
        public async Task SaveRaceControls_DaoReturnsSuccess_CommitsAndReturnsCommitResponse()
        {
            // Arrange
            var mockRace = new Mock<IRaceControlClient>();
            var mockDao = new Mock<IRaceControlDao>();
            var mockUow = new Mock<IUnityOfWork>();

            var success = new Response("ok", true, null);
            var commit = new Response("committed", true, null);
            mockDao.Setup(d => d.SaveRaceControls(It.IsAny<List<RaceControl>>()))
                   .ReturnsAsync(success);
            mockUow.Setup(u => u.RaceControlDao).Returns(mockDao.Object);
            mockUow.Setup(u => u.Commit()).ReturnsAsync(commit);

            var service = new RaceControlService(mockRace.Object, mockUow.Object);

            // Act
            var result = await service.SaveRaceControls(new List<RaceControl>());

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasSuccess);
            Assert.AreEqual(commit, result);
        }
    }
}
