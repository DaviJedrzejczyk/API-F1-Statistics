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
        public async Task GetRaceControlsBySessionFlags_DbReturnsFailure_ReturnsFailureDataResponse()
        {
            // Arrange
            var mockRace = new Mock<IRaceControlClient>();
            var mockDao = new Mock<IRaceControlDao>();
            var mockUow = new Mock<IUnityOfWork>();

            var dbFailure = new DataResponse<RaceControl>("db fail", false, null!, null);
            mockDao.Setup(d => d.GetRaceControlsBySessionFlags(It.IsAny<int>(), It.IsAny<string[]>()))
                   .ReturnsAsync(dbFailure);
            mockUow.Setup(u => u.RaceControlDao).Returns(mockDao.Object);

            var service = new RaceControlService(mockRace.Object, mockUow.Object);

            // Act
            var result = await service.GetRaceControlsBySessionFlags(1, new[] { "YELLOW" });

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasSuccess);
            Assert.AreEqual("db fail", result.Message);
        }

        [Test]
        public async Task GetRaceControlsBySessionFlags_DbReturnsItems_ReturnsMappedDtos()
        {
            // Arrange
            var mockRace = new Mock<IRaceControlClient>();
            var mockDao = new Mock<IRaceControlDao>();
            var mockUow = new Mock<IUnityOfWork>();

            var now = DateTime.UtcNow;
            var rc1 = new RaceControl { Sector = 1, Flag = "YELLOW", Date = now };
            var rc2 = new RaceControl { Sector = 1, Flag = "CLEAR", Date = now.AddSeconds(10) };
            var list = new List<RaceControl> { rc1, rc2 };

            var dbSuccess = new DataResponse<RaceControl>("ok", true, null!, list);
            mockDao.Setup(d => d.GetRaceControlsBySessionFlags(It.IsAny<int>(), It.IsAny<string[]>()))
                   .ReturnsAsync(dbSuccess);
            mockUow.Setup(u => u.RaceControlDao).Returns(mockDao.Object);

            var service = new RaceControlService(mockRace.Object, mockUow.Object);

            // Act
            var result = await service.GetRaceControlsBySessionFlags(1, new[] { "YELLOW" });

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasSuccess);
            Assert.IsNotNull(result.Itens);
            Assert.AreEqual(1, result.Itens.Count);
            var dto = result.Itens[0];
            Assert.AreEqual(rc1.Sector, dto.Sector);
            Assert.AreEqual(rc1.Flag, dto.Flag);
            Assert.AreEqual(rc1.Date, dto.DateStart);
            Assert.AreEqual(rc2.Date, dto.DateEnd);
        }

        [Test]
        public async Task GetRaceControlsBySessionFlags_DbEmpty_ApiReturnsItems_ReturnsMappedDtos()
        {
            // Arrange
            var mockRace = new Mock<IRaceControlClient>();
            var mockDao = new Mock<IRaceControlDao>();
            var mockUow = new Mock<IUnityOfWork>();

            var now = DateTime.UtcNow;
            var rc1 = new RaceControl { Sector = 2, Flag = "DOUBLE YELLOW", Date = now };
            var rc2 = new RaceControl { Sector = 2, Flag = "CLEAR", Date = now.AddSeconds(5) };
            var apiList = new List<RaceControl> { rc1, rc2 };

            var dbSuccessEmpty = new DataResponse<RaceControl>("ok", true, null, new List<RaceControl>());
            mockDao.Setup(d => d.GetRaceControlsBySessionFlags(It.IsAny<int>(), It.IsAny<string[]>()))
                   .ReturnsAsync(dbSuccessEmpty);
            mockUow.Setup(u => u.RaceControlDao).Returns(mockDao.Object);

            var apiSuccess = new DataResponse<RaceControl>("ok", true, null, apiList);
            mockRace.Setup(r => r.GetRaceControlsBySessionFlags(It.IsAny<int>(), It.IsAny<string[]>()))
                    .ReturnsAsync(apiSuccess);

            var service = new RaceControlService(mockRace.Object, mockUow.Object);

            // Act
            var result = await service.GetRaceControlsBySessionFlags(5, new[] { "DOUBLE YELLOW" });

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasSuccess);
            Assert.IsNotNull(result.Itens);
            Assert.AreEqual(1, result.Itens.Count);
            var dto = result.Itens[0];
            Assert.AreEqual(rc1.Sector, dto.Sector);
            Assert.AreEqual(rc1.Flag, dto.Flag);
            Assert.AreEqual(rc1.Date, dto.DateStart);
            Assert.AreEqual(rc2.Date, dto.DateEnd);
        }

        [Test]
        public async Task GetRaceControlsBySessionFlags_DbEmpty_ApiReturnsNoItems_ReturnsFailure()
        {
            // Arrange
            var mockRace = new Mock<IRaceControlClient>();
            var mockDao = new Mock<IRaceControlDao>();
            var mockUow = new Mock<IUnityOfWork>();

            var dbSuccessEmpty = new DataResponse<RaceControl>("ok", true, null, new List<RaceControl>());
            mockDao.Setup(d => d.GetRaceControlsBySessionFlags(It.IsAny<int>(), It.IsAny<string[]>()))
                   .ReturnsAsync(dbSuccessEmpty);
            mockUow.Setup(u => u.RaceControlDao).Returns(mockDao.Object);

            var apiEmpty = new DataResponse<RaceControl>("ok", true, null, new List<RaceControl>());
            mockRace.Setup(r => r.GetRaceControlsBySessionFlags(It.IsAny<int>(), It.IsAny<string[]>()))
                    .ReturnsAsync(apiEmpty);

            var service = new RaceControlService(mockRace.Object, mockUow.Object);

            // Act
            var result = await service.GetRaceControlsBySessionFlags(7, new[] { "YELLOW" });

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasSuccess);
            Assert.AreEqual("No race controls found for the specified session.", result.Message);
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
