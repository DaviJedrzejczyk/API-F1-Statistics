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
        [Test]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            // Arrange
            var unityMock = new Mock<IUnityOfWork>();
            var lapClientMock = new Mock<ILapClient>();
            var driverServiceMock = new Mock<IDriverService>();
            var mapperMock = new Mock<IMapper>();

            // Act
            var service = new LapService(unityMock.Object, lapClientMock.Object, driverServiceMock.Object, mapperMock.Object);

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

            var lapDaoMock = new Mock<ILapDao>();
            lapDaoMock.Setup(d => d.GetFastLapSessionBySessionKey(sessionKey))
                .ReturnsAsync(new SingleResponse<Lap> { HasSuccess = true, Item = lap });

            var unityMock = new Mock<IUnityOfWork>();
            unityMock.SetupGet(u => u.LapDao).Returns(lapDaoMock.Object);

            var lapClientMock = new Mock<ILapClient>();
            var driverServiceMock = new Mock<IDriverService>();
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<LapFastLapDto>(It.IsAny<Lap>())).Returns(lapFast);

            var service = new LapService(unityMock.Object, lapClientMock.Object, driverServiceMock.Object, mapperMock.Object);

            // Act
            var result = await service.GetFastLapOfRaceBySessionKey(sessionKey);

            // Assert
            Assert.IsTrue(result.HasSuccess);
            Assert.IsNotNull(result.Item);
            Assert.AreEqual(lapFast.DriverNumber, result.Item.DriverNumber);
        }

        [Test]
        public async Task GetFastLapOfRaceBySessionKey_NoDriversFound_ReturnsFailure()
        {
            // Arrange
            const int sessionKey = 2;

            var lapDaoMock = new Mock<ILapDao>();
            lapDaoMock.Setup(d => d.GetFastLapSessionBySessionKey(sessionKey))
                .ReturnsAsync(new SingleResponse<Lap> { HasSuccess = true, Item = null });

            var unityMock = new Mock<IUnityOfWork>();
            unityMock.SetupGet(u => u.LapDao).Returns(lapDaoMock.Object);

            var lapClientMock = new Mock<ILapClient>();
            var driverServiceMock = new Mock<IDriverService>();
            driverServiceMock.Setup(d => d.SearchDriversDatabase(It.IsAny<Entities.Dtos.DriverInsertDTO>()))
                .ReturnsAsync(new DataResponse<Entities.Class.Driver> { HasSuccess = true, Itens = new List<Entities.Class.Driver>() });
            driverServiceMock.Setup(d => d.SearchDriversExternalApi(It.IsAny<Entities.Dtos.DriverInsertDTO>()))
                .ReturnsAsync(new DataResponse<Entities.Class.Driver> { HasSuccess = true, Itens = new List<Entities.Class.Driver>() });

            var mapperMock = new Mock<IMapper>();

            var service = new LapService(unityMock.Object, lapClientMock.Object, driverServiceMock.Object, mapperMock.Object);

            // Act
            var result = await service.GetFastLapOfRaceBySessionKey(sessionKey);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.AreEqual("No drivers found for the given session key.", result.Message);
        }

        [Test]
        public async Task GetFastLapOfRaceBySessionKey_DriverDatabaseFailure_ReturnsFailure()
        {
            // Arrange
            const int sessionKey = 3;
            var ex = new Exception("db error");

            var lapDaoMock = new Mock<ILapDao>();
            lapDaoMock.Setup(d => d.GetFastLapSessionBySessionKey(sessionKey))
                .ReturnsAsync(new SingleResponse<Lap> { HasSuccess = true, Item = null });

            var unityMock = new Mock<IUnityOfWork>();
            unityMock.SetupGet(u => u.LapDao).Returns(lapDaoMock.Object);

            var lapClientMock = new Mock<ILapClient>();
            var driverServiceMock = new Mock<IDriverService>();
            driverServiceMock.Setup(d => d.SearchDriversDatabase(It.IsAny<Entities.Dtos.DriverInsertDTO>()))
                .ReturnsAsync(new DataResponse<Entities.Class.Driver> { HasSuccess = false, Exception = ex, Message = "fail" });

            var mapperMock = new Mock<IMapper>();

            var service = new LapService(unityMock.Object, lapClientMock.Object, driverServiceMock.Object, mapperMock.Object);

            // Act
            var result = await service.GetFastLapOfRaceBySessionKey(sessionKey);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.AreEqual("fail", result.Message);
            Assert.AreEqual(ex, result.Exception);
        }

        [Test]
        public async Task GetFastLapOfRaceBySessionKey_LapClientFailure_ReturnsFailure()
        {
            // Arrange
            const int sessionKey = 4;
            var driver = new Entities.Class.Driver { DriverNumber = 7 };

            var lapDaoMock = new Mock<ILapDao>();
            lapDaoMock.Setup(d => d.GetFastLapSessionBySessionKey(sessionKey))
                .ReturnsAsync(new SingleResponse<Lap> { HasSuccess = true, Item = null });

            var unityMock = new Mock<IUnityOfWork>();
            unityMock.SetupGet(u => u.LapDao).Returns(lapDaoMock.Object);

            var lapClientMock = new Mock<ILapClient>();
            var driverServiceMock = new Mock<IDriverService>();
            driverServiceMock.Setup(d => d.SearchDriversDatabase(It.IsAny<Entities.Dtos.DriverInsertDTO>()))
                .ReturnsAsync(new DataResponse<Entities.Class.Driver> { HasSuccess = true, Itens = new List<Entities.Class.Driver> { driver } });

            var lapEx = new Exception("lap error");
            lapClientMock.Setup(c => c.GetAllLapsSessionByDriver(sessionKey, driver.DriverNumber))
                .ReturnsAsync(new DataResponse<LapListDto> { HasSuccess = false, Exception = lapEx, Message = "lapfail" });

            var mapperMock = new Mock<IMapper>();

            var service = new LapService(unityMock.Object, lapClientMock.Object, driverServiceMock.Object, mapperMock.Object);

            // Act
            var result = await service.GetFastLapOfRaceBySessionKey(sessionKey);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.AreEqual("lapfail", result.Message);
            Assert.AreEqual(lapEx, result.Exception);
        }

        [Test]
        public async Task GetFastLapOfRaceBySessionKey_NoLapsFound_ReturnsFailure()
        {
            // Arrange
            const int sessionKey = 5;
            var driver = new Entities.Class.Driver { DriverNumber = 8 };

            var lapDaoMock = new Mock<ILapDao>();
            lapDaoMock.Setup(d => d.GetFastLapSessionBySessionKey(sessionKey))
                .ReturnsAsync(new SingleResponse<Lap> { HasSuccess = true, Item = null });

            var unityMock = new Mock<IUnityOfWork>();
            unityMock.SetupGet(u => u.LapDao).Returns(lapDaoMock.Object);

            var lapClientMock = new Mock<ILapClient>();
            lapClientMock.Setup(c => c.GetAllLapsSessionByDriver(sessionKey, driver.DriverNumber))
                .ReturnsAsync(new DataResponse<LapListDto> { HasSuccess = true, Itens = new List<LapListDto>() });

            var driverServiceMock = new Mock<IDriverService>();
            driverServiceMock.Setup(d => d.SearchDriversDatabase(It.IsAny<Entities.Dtos.DriverInsertDTO>()))
                .ReturnsAsync(new DataResponse<Entities.Class.Driver> { HasSuccess = true, Itens = new List<Entities.Class.Driver> { driver } });

            var mapperMock = new Mock<IMapper>();

            var service = new LapService(unityMock.Object, lapClientMock.Object, driverServiceMock.Object, mapperMock.Object);

            // Act
            var result = await service.GetFastLapOfRaceBySessionKey(sessionKey);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.AreEqual("No laps found for the given session key.", result.Message);
        }

        [Test]
        public async Task GetFastLapOfRaceBySessionKey_FindsFastLapAndSaves_ReturnsSuccess()
        {
            // Arrange
            const int sessionKey = 6;
            var driver = new Entities.Class.Driver { DriverNumber = 9 };
            var lapList = new LapListDto { DriverNumber = 9, LapDuration = 11.1 };
            var lap = new Lap();
            var fastLapDto = new LapFastLapDto { DriverNumber = 9, LapDuration = 11.1 };

            var lapDaoMock = new Mock<ILapDao>();
            lapDaoMock.Setup(d => d.GetFastLapSessionBySessionKey(sessionKey))
                .ReturnsAsync(new SingleResponse<Lap> { HasSuccess = true, Item = null });
            lapDaoMock.Setup(d => d.SaveLap(It.IsAny<Lap>()))
                .ReturnsAsync(new Response { HasSuccess = true });

            var unityMock = new Mock<IUnityOfWork>();
            unityMock.SetupGet(u => u.LapDao).Returns(lapDaoMock.Object);
            unityMock.Setup(u => u.Commit()).ReturnsAsync(new Response { HasSuccess = true });

            var lapClientMock = new Mock<ILapClient>();
            lapClientMock.Setup(c => c.GetAllLapsSessionByDriver(sessionKey, driver.DriverNumber))
                .ReturnsAsync(new DataResponse<LapListDto> { HasSuccess = true, Itens = new List<LapListDto> { lapList } });

            var driverServiceMock = new Mock<IDriverService>();
            driverServiceMock.Setup(d => d.SearchDriversDatabase(It.IsAny<Entities.Dtos.DriverInsertDTO>()))
                .ReturnsAsync(new DataResponse<Entities.Class.Driver> { HasSuccess = true, Itens = new List<Entities.Class.Driver> { driver } });

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<Lap>(It.IsAny<LapListDto>())).Returns(lap);
            mapperMock.Setup(m => m.Map<LapFastLapDto>(It.IsAny<Lap>())).Returns(fastLapDto);

            var service = new LapService(unityMock.Object, lapClientMock.Object, driverServiceMock.Object, mapperMock.Object);

            // Act
            var result = await service.GetFastLapOfRaceBySessionKey(sessionKey);

            // Assert
            Assert.IsTrue(result.HasSuccess);
            Assert.IsNotNull(result.Item);
            Assert.AreEqual(9, result.Item.DriverNumber);
            Assert.AreEqual(11.1, result.Item.LapDuration);
        }

        [Test]
        public async Task GetFastLapOfRaceBySessionKeyDb_DaoFailure_ReturnsFailure()
        {
            // Arrange
            const int sessionKey = 7;
            var ex = new Exception("dao fail");
            var lapDaoMock = new Mock<ILapDao>();
            lapDaoMock.Setup(d => d.GetFastLapSessionBySessionKey(sessionKey))
                .ReturnsAsync(new SingleResponse<Lap> { HasSuccess = false, Exception = ex, Message = "err" });

            var unityMock = new Mock<IUnityOfWork>();
            unityMock.SetupGet(u => u.LapDao).Returns(lapDaoMock.Object);

            var mapperMock = new Mock<IMapper>();
            var lapClientMock = new Mock<ILapClient>();
            var driverServiceMock = new Mock<IDriverService>();

            var service = new LapService(unityMock.Object, lapClientMock.Object, driverServiceMock.Object, mapperMock.Object);

            // Act
            var result = await service.GetFastLapOfRaceBySessionKeyDb(sessionKey);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.AreEqual("err", result.Message);
            Assert.AreEqual(ex, result.Exception);
        }

        [Test]
        public async Task GetFastLapOfRaceBySessionKeyDb_NoItem_ReturnsSuccessWithMessage()
        {
            // Arrange
            const int sessionKey = 8;
            var lapDaoMock = new Mock<ILapDao>();
            lapDaoMock.Setup(d => d.GetFastLapSessionBySessionKey(sessionKey))
                .ReturnsAsync(new SingleResponse<Lap> { HasSuccess = true, Item = null });

            var unityMock = new Mock<IUnityOfWork>();
            unityMock.SetupGet(u => u.LapDao).Returns(lapDaoMock.Object);

            var mapperMock = new Mock<IMapper>();
            var lapClientMock = new Mock<ILapClient>();
            var driverServiceMock = new Mock<IDriverService>();

            var service = new LapService(unityMock.Object, lapClientMock.Object, driverServiceMock.Object, mapperMock.Object);

            // Act
            var result = await service.GetFastLapOfRaceBySessionKeyDb(sessionKey);

            // Assert
            Assert.IsTrue(result.HasSuccess);
            Assert.AreEqual("No fast lap found for the given session key in the database.", result.Message);
            Assert.IsNull(result.Item);
        }

        [Test]
        public async Task GetFastLapOfRaceBySessionKeyDb_ItemExists_ReturnsMapped()
        {
            // Arrange
            const int sessionKey = 9;
            var lap = new Lap { MeetingKey = 99 };
            var mapped = new LapFastLapDto { DriverNumber = 1 };

            var lapDaoMock = new Mock<ILapDao>();
            lapDaoMock.Setup(d => d.GetFastLapSessionBySessionKey(sessionKey))
                .ReturnsAsync(new SingleResponse<Lap> { HasSuccess = true, Item = lap });

            var unityMock = new Mock<IUnityOfWork>();
            unityMock.SetupGet(u => u.LapDao).Returns(lapDaoMock.Object);

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<LapFastLapDto>(lap)).Returns(mapped);

            var lapClientMock = new Mock<ILapClient>();
            var driverServiceMock = new Mock<IDriverService>();

            var service = new LapService(unityMock.Object, lapClientMock.Object, driverServiceMock.Object, mapperMock.Object);

            // Act
            var result = await service.GetFastLapOfRaceBySessionKeyDb(sessionKey);

            // Assert
            Assert.IsTrue(result.HasSuccess);
            Assert.IsNotNull(result.Item);
            Assert.AreEqual(1, result.Item.DriverNumber);
        }

        [Test]
        public async Task SaveLap_DaoSaveFails_ReturnsFailure()
        {
            // Arrange
            var lap = new Lap();
            var ex = new Exception("save fail");

            var lapDaoMock = new Mock<ILapDao>();
            lapDaoMock.Setup(d => d.SaveLap(lap))
                .ReturnsAsync(new Response { HasSuccess = false, Exception = ex, Message = "saveerror" });

            var unityMock = new Mock<IUnityOfWork>();
            unityMock.SetupGet(u => u.LapDao).Returns(lapDaoMock.Object);

            var mapperMock = new Mock<IMapper>();
            var lapClientMock = new Mock<ILapClient>();
            var driverServiceMock = new Mock<IDriverService>();

            var service = new LapService(unityMock.Object, lapClientMock.Object, driverServiceMock.Object, mapperMock.Object);

            // Act
            var result = await service.SaveLap(lap);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.AreEqual("saveerror", result.Message);
            Assert.AreEqual(ex, result.Exception);
        }

        [Test]
        public async Task SaveLap_DaoSaveSucceeds_ReturnsCommitResult()
        {
            // Arrange
            var lap = new Lap();

            var lapDaoMock = new Mock<ILapDao>();
            lapDaoMock.Setup(d => d.SaveLap(lap))
                .ReturnsAsync(new Response { HasSuccess = true });

            var expected = new Response { HasSuccess = true };
            var unityMock = new Mock<IUnityOfWork>();
            unityMock.SetupGet(u => u.LapDao).Returns(lapDaoMock.Object);
            unityMock.Setup(u => u.Commit()).ReturnsAsync(expected);

            var mapperMock = new Mock<IMapper>();
            var lapClientMock = new Mock<ILapClient>();
            var driverServiceMock = new Mock<IDriverService>();

            var service = new LapService(unityMock.Object, lapClientMock.Object, driverServiceMock.Object, mapperMock.Object);

            // Act
            var result = await service.SaveLap(lap);

            // Assert
            Assert.IsTrue(result.HasSuccess);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public async Task SaveLap_DaoThrowsException_ReturnsFailure()
        {
            // Arrange
            var lap = new Lap();
            var lapDaoMock = new Mock<ILapDao>();
            lapDaoMock.Setup(d => d.SaveLap(lap)).ThrowsAsync(new InvalidOperationException("boom"));

            var unityMock = new Mock<IUnityOfWork>();
            unityMock.SetupGet(u => u.LapDao).Returns(lapDaoMock.Object);

            var mapperMock = new Mock<IMapper>();
            var lapClientMock = new Mock<ILapClient>();
            var driverServiceMock = new Mock<IDriverService>();

            var service = new LapService(unityMock.Object, lapClientMock.Object, driverServiceMock.Object, mapperMock.Object);

            // Act
            var result = await service.SaveLap(lap);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.IsNotNull(result.Exception);
            Assert.AreEqual("boom", result.Message);
        }
    }
}
