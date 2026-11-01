using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Dao.Interface;
using Entities.Class;
using Entities.Dtos;
using ExternalApi.Interfaces;
using Moq;
using NUnit.Framework;
using Services.Impl;
using Services.Interfaces;
using Shared.Responses;

namespace UnitTests.Dao
{
    [TestFixture]
    public class LapServiceTests
    {
        private LapService service;
        private Mock<IUnityOfWork> unityMock;
        private Mock<ILapClient> lapClientMock;
        private Mock<IDriverService> driverServiceMock;
        private Mock<IMapper> mapperMock;
        private Mock<ILapFastSectorService> lapFastSectorServiceMock;
        private Mock<ILapDao> lapDaoMock;

        [SetUp]
        public void Setup()
        {
            unityMock = new Mock<IUnityOfWork>();
            lapClientMock = new Mock<ILapClient>();
            driverServiceMock = new Mock<IDriverService>();
            mapperMock = new Mock<IMapper>();
            lapFastSectorServiceMock = new Mock<ILapFastSectorService>();
            lapDaoMock = new Mock<ILapDao>();
            service = new LapService(unityMock.Object, lapClientMock.Object, driverServiceMock.Object, mapperMock.Object, lapFastSectorServiceMock.Object);
        }

        [Test]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            // Arrange
            unityMock = new Mock<IUnityOfWork>();
            lapClientMock = new Mock<ILapClient>();
            driverServiceMock = new Mock<IDriverService>();
            mapperMock = new Mock<IMapper>();
            lapFastSectorServiceMock = new Mock<ILapFastSectorService>();

            // Act
            service = new LapService(unityMock.Object, lapClientMock.Object, driverServiceMock.Object, mapperMock.Object, lapFastSectorServiceMock.Object);

            // Assert
            Assert.IsNotNull(service);
        }

        [Test]
        public async Task GetFastLapOfRaceBySessionKey_CompleteWindowsSkipImport()
        {
            var now = DateTimeOffset.UtcNow;
            unityMock.Setup(u => u.GetImportWindow("fast_lap", "1", It.IsAny<CancellationToken>())).ReturnsAsync(new global::Dao.Cache.ImportWindow { IsComplete = true, CompletedAtUtc = now.AddMinutes(-1), ExpiresAtUtc = now.AddMinutes(1), Payload = System.Text.Json.JsonSerializer.Serialize(new[] { new LapFastLapDto { DriverNumber = 5, LapDuration = 12.3 } }) });
            unityMock.Setup(u => u.GetImportWindow("fast_sectors", "1", It.IsAny<CancellationToken>())).ReturnsAsync(new global::Dao.Cache.ImportWindow { IsComplete = true, CompletedAtUtc = now.AddMinutes(-1), ExpiresAtUtc = now.AddMinutes(1), Payload = "[]" });
            var result = await service.GetFastLapOfRaceBySessionKey(1);
            Assert.That(result.HasSuccess, Is.True);
            Assert.That(result.Item.DriverNumber, Is.EqualTo(5));
            lapClientMock.Verify(x => x.GetAllLapsSessionByDriver(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task GetFastLapOfRaceBySessionKey_FailedDriverFetchDoesNotSavePartialResults()
        {
            unityMock.Setup(u => u.GetImportWindow(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((global::Dao.Cache.ImportWindow?)null);
            var transaction = new Mock<IImportTransaction>();
            unityMock.Setup(u => u.BeginImport("fast_lap", "42", It.IsAny<CancellationToken>())).ReturnsAsync(transaction.Object);
            driverServiceMock.Setup(x => x.GetAllDriversSession(42, It.IsAny<CancellationToken>())).ReturnsAsync(new DataResponse<Driver> { HasSuccess = true, Itens = new() { new() { DriverNumber = 1 }, new() { DriverNumber = 2 } } });
            lapClientMock.Setup(x => x.GetAllLapsSessionByDriver(42, 1, It.IsAny<CancellationToken>())).ReturnsAsync(new DataResponse<LapListDto> { HasSuccess = true, Itens = new() { new() { SessionKey = 42, DriverNumber = 1, LapDuration = 70 } } });
            lapClientMock.Setup(x => x.GetAllLapsSessionByDriver(42, 2, It.IsAny<CancellationToken>())).ReturnsAsync(new DataResponse<LapListDto> { HasSuccess = false, Message = "upstream failed" });
            var result = await service.GetFastLapOfRaceBySessionKey(42);
            Assert.That(result.HasSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("upstream failed"));
            unityMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Never);
            lapDaoMock.Verify(x => x.UpsertLap(It.IsAny<Lap>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        [Test]
        public async Task GetFastLapOfRaceBySessionKeyDb_DaoFailure_ReturnsFailure()
        {
            // Arrange
            const int sessionKey = 7;
            var ex = new Exception("dao fail");

            lapDaoMock.Setup(d => d.GetFastLapSessionBySessionKey(sessionKey))
                .ReturnsAsync(new SingleResponse<Lap> { HasSuccess = false, Exception = ex, Message = "err" });

            unityMock.SetupGet(u => u.LapDao).Returns(lapDaoMock.Object);

            // Act
            var result = await service.GetFastLapOfRaceBySessionKeyDb(sessionKey);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.HasSuccess, Is.False);
                Assert.That(result.Message, Is.EqualTo("err"));
                Assert.That(result.Exception, Is.EqualTo(ex));
            });
        }

        [Test]
        public async Task GetFastLapOfRaceBySessionKeyDb_NoItem_ReturnsSuccessWithMessage()
        {
            // Arrange
            const int sessionKey = 8;

            lapDaoMock.Setup(d => d.GetFastLapSessionBySessionKey(sessionKey))
                .ReturnsAsync(new SingleResponse<Lap> { HasSuccess = true, Item = null });

            unityMock.SetupGet(u => u.LapDao).Returns(lapDaoMock.Object);

            // Act
            var result = await service.GetFastLapOfRaceBySessionKeyDb(sessionKey);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.HasSuccess, Is.True);
                Assert.That(result.Message, Is.EqualTo("No fast lap found for the given session key in the database."));
                Assert.That(result.Item, Is.Null);
            });
        }

        [Test]
        public async Task GetFastLapOfRaceBySessionKeyDb_ItemExists_ReturnsMapped()
        {
            // Arrange
            const int sessionKey = 9;
            var lap = new Lap { MeetingKey = 99 };
            var mapped = new LapFastLapDto { DriverNumber = 1 };

            lapDaoMock.Setup(d => d.GetFastLapSessionBySessionKey(sessionKey))
                .ReturnsAsync(new SingleResponse<Lap> { HasSuccess = true, Item = lap });

            unityMock.SetupGet(u => u.LapDao).Returns(lapDaoMock.Object);

            mapperMock.Setup(m => m.Map<LapFastLapDto>(lap)).Returns(mapped);

            // Act
            var result = await service.GetFastLapOfRaceBySessionKeyDb(sessionKey);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.HasSuccess, Is.True);
                Assert.That(result.Item, Is.Not.Null);
                Assert.That(result.Item.DriverNumber, Is.EqualTo(1));
            });
        }

        [Test]
        public async Task SaveLap_DaoSaveFails_ReturnsFailure()
        {
            // Arrange
            var lap = new Lap();
            var ex = new Exception("save fail");

            lapDaoMock.Setup(d => d.SaveLap(lap))
                .ReturnsAsync(new Response { HasSuccess = false, Exception = ex, Message = "saveerror" });

            unityMock.SetupGet(u => u.LapDao).Returns(lapDaoMock.Object);

            // Act
            var result = await service.SaveLap(lap);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.HasSuccess, Is.False);
                Assert.That(result.Message, Is.EqualTo("saveerror"));
                Assert.That(result.Exception, Is.EqualTo(ex));
            });
        }

        [Test]
        public async Task SaveLap_DaoSaveSucceeds_ReturnsCommitResult()
        {
            // Arrange
            var lap = new Lap();

            lapDaoMock.Setup(d => d.SaveLap(lap))
                .ReturnsAsync(new Response { HasSuccess = true });

            var expected = new Response { HasSuccess = true };
            unityMock.SetupGet(u => u.LapDao).Returns(lapDaoMock.Object);
            unityMock.SetupGet(u => u.LapDao).Returns(lapDaoMock.Object);
            unityMock.Setup(u => u.Commit()).ReturnsAsync(expected);

            // Act
            var result = await service.SaveLap(lap);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.HasSuccess, Is.True);
                Assert.That(result, Is.EqualTo(expected));
            });
        }

        [Test]
        public async Task SaveLap_DaoThrowsException_ReturnsFailure()
        {
            // Arrange
            var lap = new Lap();
            var lapDaoMock = new Mock<ILapDao>();
            lapDaoMock.Setup(d => d.SaveLap(lap)).ThrowsAsync(new InvalidOperationException("boom"));

            unityMock.SetupGet(u => u.LapDao).Returns(lapDaoMock.Object);

            // Act
            var result = await service.SaveLap(lap);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.HasSuccess, Is.False);
                Assert.That(result.Exception, Is.Not.Null);
                Assert.That(result.Message, Is.EqualTo("boom"));
            });
        }
    }
}
