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
        public async Task GetFastLapOfRaceBySessionKey_DbHasFastLap_ReturnsDbItem()
        {
            // Arrange
            const int sessionKey = 1;
            var lap = new Lap { MeetingKey = 10 };
            var lapFast = new LapFastLapDto { DriverNumber = 5, LapDuration = 12.3 };

            lapDaoMock.Setup(d => d.GetFastLapSessionBySessionKey(sessionKey))
                .ReturnsAsync(new SingleResponse<Lap> { HasSuccess = true, Item = lap });

            unityMock.SetupGet(u => u.LapDao).Returns(lapDaoMock.Object);

            mapperMock.Setup(m => m.Map<LapFastLapDto>(It.IsAny<Lap>())).Returns(lapFast);

            // Act
            var result = await service.GetFastLapOfRaceBySessionKey(sessionKey);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.HasSuccess, Is.True);
                Assert.That(result.Item, Is.Not.Null);
            });
            Assert.That(result.Item.DriverNumber, Is.EqualTo(lapFast.DriverNumber));
        }

        [Test]
        public async Task GetFastLapOfRaceBySessionKey_NoDriversFound_ReturnsFailure()
        {
            // Arrange
            const int sessionKey = 2;

            lapDaoMock.Setup(d => d.GetFastLapSessionBySessionKey(sessionKey))
                .ReturnsAsync(new SingleResponse<Lap> { HasSuccess = true, Item = null });

            unityMock.SetupGet(u => u.LapDao).Returns(lapDaoMock.Object);

            driverServiceMock.Setup(d => d.SearchDriversDatabase(It.IsAny<Entities.Dtos.DriverInsertDTO>()))
                .ReturnsAsync(new DataResponse<Entities.Class.Driver> { HasSuccess = true, Itens = new List<Entities.Class.Driver>() });
            driverServiceMock.Setup(d => d.SearchDriversExternalApi(It.IsAny<Entities.Dtos.DriverInsertDTO>()))
                .ReturnsAsync(new DataResponse<Entities.Class.Driver> { HasSuccess = true, Itens = new List<Entities.Class.Driver>() });

            // Act
            var result = await service.GetFastLapOfRaceBySessionKey(sessionKey);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.HasSuccess, Is.False);
                Assert.That(result.Message, Is.EqualTo("No drivers found for the given session key."));
            });
        }

        [Test]
        public async Task GetFastLapOfRaceBySessionKey_DriverDatabaseFailure_ReturnsFailure()
        {
            // Arrange
            const int sessionKey = 3;
            var ex = new Exception("db error");

            lapDaoMock.Setup(d => d.GetFastLapSessionBySessionKey(sessionKey))
                .ReturnsAsync(new SingleResponse<Lap> { HasSuccess = true, Item = null });

            unityMock.SetupGet(u => u.LapDao).Returns(lapDaoMock.Object);

            driverServiceMock.Setup(d => d.SearchDriversDatabase(It.IsAny<Entities.Dtos.DriverInsertDTO>()))
                .ReturnsAsync(new DataResponse<Entities.Class.Driver> { HasSuccess = false, Exception = ex, Message = "fail" });
            // Act
            var result = await service.GetFastLapOfRaceBySessionKey(sessionKey);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.HasSuccess, Is.False);
                Assert.That(result.Message, Is.EqualTo("fail"));
                Assert.That(result.Exception, Is.EqualTo(ex));
            });
        }

        [Test]
        public async Task GetFastLapOfRaceBySessionKey_LapClientFailure_ReturnsFailure()
        {
            // Arrange
            const int sessionKey = 4;
            var driver = new Driver { DriverNumber = 7 };
            
            lapDaoMock.Setup(d => d.GetFastLapSessionBySessionKey(sessionKey))
                .ReturnsAsync(new SingleResponse<Lap> { HasSuccess = true, Item = null });

            unityMock.SetupGet(u => u.LapDao).Returns(lapDaoMock.Object);

            driverServiceMock.Setup(d => d.SearchDriversDatabase(It.IsAny<DriverInsertDTO>()))
                .ReturnsAsync(new DataResponse<Driver> { HasSuccess = true, Itens = new List<Driver> { driver } });

            lapFastSectorServiceMock.Setup(s => s.GetFastSectorsOfSession(sessionKey))
                .ReturnsAsync(new DataResponse<LapFastSector> { HasSuccess = true, Itens = new List<LapFastSector>() });

            var lapEx = new Exception("lap error");
            lapClientMock.Setup(c => c.GetAllLapsSessionByDriver(sessionKey, driver.DriverNumber))
                .ReturnsAsync(new DataResponse<LapListDto> { HasSuccess = false, Exception = lapEx, Message = "lapfail" });

            // Act
            var result = await service.GetFastLapOfRaceBySessionKey(sessionKey);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.HasSuccess, Is.False);
                Assert.That(result.Message, Is.EqualTo("lapfail"));
                Assert.That(result.Exception, Is.EqualTo(lapEx));
            });
        }

        [Test]
        public async Task GetFastLapOfRaceBySessionKey_NoLapsFound_ReturnsFailure()
        {
            // Arrange
            const int sessionKey = 5;
            var driver = new Driver { DriverNumber = 8 };

            lapDaoMock.Setup(d => d.GetFastLapSessionBySessionKey(sessionKey))
                .ReturnsAsync(new SingleResponse<Lap> { HasSuccess = true, Item = null! });

            unityMock.SetupGet(u => u.LapDao).Returns(lapDaoMock.Object);

            lapFastSectorServiceMock.Setup(s => s.GetFastSectorsOfSession(sessionKey))
                .ReturnsAsync(new DataResponse<LapFastSector> { HasSuccess = true, Itens = new System.Collections.Generic.List<LapFastSector>() });

            lapClientMock.Setup(c => c.GetAllLapsSessionByDriver(sessionKey, driver.DriverNumber))
                .ReturnsAsync(new DataResponse<LapListDto> { HasSuccess = true, Itens = new System.Collections.Generic.List<LapListDto>() });

            driverServiceMock.Setup(d => d.SearchDriversDatabase(It.IsAny<DriverInsertDTO>()))
                .ReturnsAsync(new DataResponse<Driver> { HasSuccess = true, Itens = new System.Collections.Generic.List<Driver> { driver } });

            // Act
            var result = await service.GetFastLapOfRaceBySessionKey(sessionKey);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.HasSuccess, Is.False);
                Assert.That(result.Message, Is.EqualTo("No laps found for the given session key."));
            });
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
