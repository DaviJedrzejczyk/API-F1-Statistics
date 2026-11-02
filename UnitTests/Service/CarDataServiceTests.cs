using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dao.Interface;
using Entities.Class;
using Entities.Dtos;
using ExternalApi.Interfaces;
using Moq;
using NUnit.Framework;
using Services.Impl;
using Services.Interfaces;
using Shared.Responses;

namespace UnitTests.Service
{
    [TestFixture]
    public class CarDataServiceTests
    {
        private Mock<IUnityOfWork> _unityMock = null!;
        private Mock<ICarDataClient> _carClientMock = null!;
        private Mock<IDriverService> _driverServiceMock = null!;
        private Mock<ICarDataDao> _carDataDaoMock = null!;

        [SetUp]
        public void SetUp()
        {
            _unityMock = new Mock<IUnityOfWork>();
            _carClientMock = new Mock<ICarDataClient>();
            _driverServiceMock = new Mock<IDriverService>();
            _carDataDaoMock = new Mock<ICarDataDao>();

            _unityMock.Setup(u => u.CarDataDao).Returns(_carDataDaoMock.Object);
            _unityMock.Setup(u => u.GetImportWindow(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((global::Dao.Cache.ImportWindow?)null);
            _unityMock.Setup(u => u.StageImportWindow(It.IsAny<global::Dao.Cache.ImportWindow>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var transaction = new Mock<IImportTransaction>();
            transaction.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _unityMock.Setup(u => u.BeginImport(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(transaction.Object);
        }

        [Test]
        public void Constructor_WithValidDependencies_DoesNotThrow()
        {
            // Arrange / Act
            Assert.DoesNotThrow(() => new CarDataService(_unityMock.Object, _carClientMock.Object, _driverServiceMock.Object));
        }

        [Test]
        public async Task SaveCarDatas_SaveFails_ReturnsSaveResponse()
        {
            // Arrange
            var service = new CarDataService(_unityMock.Object, _carClientMock.Object, _driverServiceMock.Object);
            var data = new List<CarData> { new() };
            var saveResponse = new Response { HasSuccess = false, Message = "save failed" };

            _carDataDaoMock.Setup(d => d.SaveCarDatas(It.IsAny<List<CarData>>())).ReturnsAsync(saveResponse);

            // Act
            var result = await service.SaveCarDatas(data);

            // Assert
            Assert.That(result, Is.SameAs(saveResponse));
            _carDataDaoMock.Verify(d => d.SaveCarDatas(It.Is<List<CarData>>(l => l == data)), Times.Once);
        }

        [Test]
        public async Task SaveCarDatas_SaveSucceeds_CommitsAndReturnsCommitResponse()
        {
            // Arrange
            var service = new CarDataService(_unityMock.Object, _carClientMock.Object, _driverServiceMock.Object);
            var data = new List<CarData> { new() };
            var saveResponse = new Response { HasSuccess = true };
            var commitResponse = new Response { HasSuccess = true, Message = "committed" };

            _carDataDaoMock.Setup(d => d.SaveCarDatas(It.IsAny<List<CarData>>())).ReturnsAsync(saveResponse);
            _unityMock.Setup(u => u.Commit()).ReturnsAsync(commitResponse);

            // Act
            var result = await service.SaveCarDatas(data);

            // Assert
            Assert.That(result, Is.SameAs(commitResponse));
            _carDataDaoMock.Verify(d => d.SaveCarDatas(It.Is<List<CarData>>(l => l == data)), Times.Once);
            _unityMock.Verify(u => u.Commit(), Times.Once);
        }

        [Test]
        public async Task GetHighSpeedsSessionApi_DatabaseFailure_ReturnsFailureResponse()
        {
            // Arrange
            var databaseResponse = new DataResponse<CarData> { HasSuccess = false, Message = "db fail" };
            _carDataDaoMock.Setup(d => d.GetHighSpeedSessionDatabase(1, 10, It.IsAny<CancellationToken>())).ReturnsAsync(databaseResponse);
            _carClientMock.Setup(c => c.GetHighSpeedsSession(1, 10, It.IsAny<CancellationToken>())).ReturnsAsync(new DataResponse<CarData> { HasSuccess = true, Itens = new() });

            var service = new CarDataService(_unityMock.Object, _carClientMock.Object, _driverServiceMock.Object);

            // Act
            var result = await service.GetHighSpeedsSessionApi(1, 10);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("db fail"));
        }

        [Test]
        public async Task GetHighSpeedsSessionApi_DatabaseHas15Items_ReturnsDatabaseResponse()
        {
            // Arrange
            var list = Enumerable.Range(1, 15).Select(i => new CarData { DriverNumber = i }).ToList();
            _unityMock.Setup(u => u.GetImportWindow("car_data", "2:5", It.IsAny<CancellationToken>())).ReturnsAsync(new global::Dao.Cache.ImportWindow { IsComplete = true, CompletedAtUtc = DateTimeOffset.UtcNow.AddMinutes(-1), ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(1), Payload = System.Text.Json.JsonSerializer.Serialize(list) });

            var service = new CarDataService(_unityMock.Object, _carClientMock.Object, _driverServiceMock.Object);

            // Act
            var result = await service.GetHighSpeedsSessionApi(2, 5);

            // Assert
            Assert.IsTrue(result.HasSuccess);
            Assert.That(result.Itens.Count, Is.EqualTo(15));
        }

        [Test]
        public async Task GetHighSpeedsSessionApi_ClientFailure_ReturnsFailureResponse()
        {
            // Arrange
            var databaseResponse = new DataResponse<CarData> { HasSuccess = true, Itens = new List<CarData>() };
            _carDataDaoMock.Setup(d => d.GetHighSpeedSessionDatabase(3, 20, It.IsAny<CancellationToken>())).ReturnsAsync(databaseResponse);

            var clientResponse = new DataResponse<CarData> { HasSuccess = false, Message = "api fail" };
            _carClientMock.Setup(c => c.GetHighSpeedsSession(3, 20, It.IsAny<CancellationToken>())).ReturnsAsync(clientResponse);

            var service = new CarDataService(_unityMock.Object, _carClientMock.Object, _driverServiceMock.Object);

            // Act
            var result = await service.GetHighSpeedsSessionApi(3, 20);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("api fail"));
        }

        [Test]
        public async Task GetHighSpeedsSessionApi_Success_SavesOnlyNewCarDatasAndReturnsMaxPerDriver()
        {
            // Arrange
            var dbList = new List<CarData> { new() { DriverNumber = 1, SessionKey = 4 } };
            var databaseResponse = new DataResponse<CarData> { HasSuccess = true, Itens = dbList };
            _carDataDaoMock.Setup(d => d.GetHighSpeedSessionDatabase(4, 50, It.IsAny<CancellationToken>())).ReturnsAsync(databaseResponse);

            // Client returns two entries for driver 1 (speeds 100 and 110) and one for driver 2 (speed 105)
            var clientList = new List<CarData>
            {
                new() { DriverNumber = 1, SessionKey = 4, Speed = 100 },
                new() { DriverNumber = 1, SessionKey = 4, Speed = 110 },
                new() { DriverNumber = 2, SessionKey = 4, Speed = 105 }
            };
            var clientResponse = new DataResponse<CarData> { HasSuccess = true, Itens = clientList };
            _carClientMock.Setup(c => c.GetHighSpeedsSession(4, 50, It.IsAny<CancellationToken>())).ReturnsAsync(clientResponse);

            // SaveCarDatas will be called with only driver 2 (since driver 1 exists in db)
            var saveResponse = new Response { HasSuccess = true };
            _carDataDaoMock.Setup(d => d.SaveCarDatas(It.IsAny<List<CarData>>(), It.IsAny<CancellationToken>())).ReturnsAsync(saveResponse);
            var commitResponse = new Response { HasSuccess = true };
            _unityMock.Setup(u => u.Commit(It.IsAny<CancellationToken>())).ReturnsAsync(commitResponse);

            var service = new CarDataService(_unityMock.Object, _carClientMock.Object, _driverServiceMock.Object);

            // Act
            var result = await service.GetHighSpeedsSessionApi(4, 50);

            // Assert
            Assert.IsTrue(result.HasSuccess);
            // Should return two carDatas (max per driver): driver1 speed110 and driver2 speed105
            Assert.That(result.Itens.Count, Is.EqualTo(2));
            var d1 = result.Itens.First(i => i.DriverNumber == 1);
            var d2 = result.Itens.First(i => i.DriverNumber == 2);
            Assert.That(d1.Speed, Is.EqualTo(110));
            Assert.That(d2.Speed, Is.EqualTo(105));

            // Verify SaveCarDatas called with only driver2
            _carDataDaoMock.Verify(d => d.SaveCarDatas(It.Is<List<CarData>>(l => l.Count == 1 && l[0].DriverNumber == 2), It.IsAny<CancellationToken>()), Times.Once);
            _unityMock.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetHighSpeedsSessionApi_InsertFails_ReturnsFailureResponse()
        {
            // Arrange
            var dbList = new List<CarData>();
            var databaseResponse = new DataResponse<CarData> { HasSuccess = true, Itens = dbList };
            _carDataDaoMock.Setup(d => d.GetHighSpeedSessionDatabase(5, 30, It.IsAny<CancellationToken>())).ReturnsAsync(databaseResponse);

            var clientList = new List<CarData> { new() { DriverNumber = 7, SessionKey = 5, Speed = 88 } };
            var clientResponse = new DataResponse<CarData> { HasSuccess = true, Itens = clientList };
            _carClientMock.Setup(c => c.GetHighSpeedsSession(5, 30, It.IsAny<CancellationToken>())).ReturnsAsync(clientResponse);

            var saveResponse = new Response { HasSuccess = false, Message = "insert failed" };
            _carDataDaoMock.Setup(d => d.SaveCarDatas(It.IsAny<List<CarData>>(), It.IsAny<CancellationToken>())).ReturnsAsync(saveResponse);

            var service = new CarDataService(_unityMock.Object, _carClientMock.Object, _driverServiceMock.Object);

            // Act
            var result = await service.GetHighSpeedsSessionApi(5, 30);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("insert failed"));
        }

        [Test]
        public async Task GetHighSpeedsSessionApi_WhenExceptionThrown_ReturnsFailureWithException()
        {
            // Arrange
            _carDataDaoMock.Setup(d => d.GetHighSpeedSessionDatabase(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(new DataResponse<CarData> { HasSuccess = true, Itens = new List<CarData>() });
            _carClientMock.Setup(c => c.GetHighSpeedsSession(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("boom"));

            var service = new CarDataService(_unityMock.Object, _carClientMock.Object, _driverServiceMock.Object);

            // Act
            var result = await service.GetHighSpeedsSessionApi(6, 1);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.IsNotNull(result.Exception);
            Assert.IsInstanceOf<InvalidOperationException>(result.Exception);
            Assert.That(result.Exception.Message, Is.EqualTo("boom"));
        }

        [Test]
        public async Task GetSortedHighSpeedsSession_NoDrivers_ReturnsFailure()
        {
            // Arrange
            var emptyDrivers = new DataResponse<Driver> { HasSuccess = true, Itens = new List<Driver>() };
            _driverServiceMock.Setup(d => d.GetAllDriversSession(10, It.IsAny<CancellationToken>())).ReturnsAsync(emptyDrivers);

            var service = new CarDataService(_unityMock.Object, _carClientMock.Object, _driverServiceMock.Object);

            // Act
            var result = await service.GetSortedHighSpeedsSession(10, 0);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("Drivers must be inserted."));
        }

        [Test]
        public async Task GetSortedHighSpeedsSession_NoCarDataItems_ReturnsFailure()
        {
            // Arrange
            var drivers = new List<Driver>
            {
                new() { DriverNumber = 11, SessionKey = 20, LastName = "X", HeadshotUrl = "u", TeamColour = "c", TeamName = "t" }
            };
            _driverServiceMock.Setup(d => d.GetAllDriversSession(20, It.IsAny<CancellationToken>())).ReturnsAsync(new DataResponse<Driver> { HasSuccess = true, Itens = drivers });

            // Arrange car client to return a failure/empty response so GetHighSpeedsSessionApi will produce a DataResponse<CarData> with Itens == null
            _carClientMock.Setup(c => c.GetHighSpeedsSession(20, 0, It.IsAny<CancellationToken>())).ReturnsAsync(new DataResponse<CarData> { HasSuccess = false, Itens = null! });

            // Use real service instance (do not attempt to Setup non-overridable members)
            var service = new CarDataService(_unityMock.Object, _carClientMock.Object, _driverServiceMock.Object);

            // Act
            var result = await service.GetSortedHighSpeedsSession(20, 0);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("Not found the maximum speed of the drivers"));
        }

        [Test]
        public async Task GetSortedHighSpeedsSession_Success_ReturnsSortedDTOs()
        {
            // Arrange
            int sessionKey = 30;
            var drivers = new List<Driver>
            {
                new() { DriverNumber = 1, SessionKey = sessionKey, LastName = "A", HeadshotUrl = "u1", TeamColour = "c1", TeamName = "t1" },
                new() { DriverNumber = 2, SessionKey = sessionKey, LastName = "B", HeadshotUrl = "u2", TeamColour = "c2", TeamName = "t2" }
            };
            _driverServiceMock.Setup(d => d.GetAllDriversSession(sessionKey, It.IsAny<CancellationToken>())).ReturnsAsync(new DataResponse<Driver> { HasSuccess = true, Itens = drivers });

            var carDataList = new List<CarData>
            {
                new() { DriverNumber = 1, SessionKey = sessionKey, Speed = 150 },
                new() { DriverNumber = 2, SessionKey = sessionKey, Speed = 160 }
            };
            var carDataResponse = new DataResponse<CarData> { HasSuccess = true, Itens = carDataList };

            // Use real service instance (do not attempt to Setup non-overridable members)
            _carDataDaoMock.Setup(d => d.GetHighSpeedSessionDatabase(sessionKey, 0, It.IsAny<CancellationToken>())).ReturnsAsync(new DataResponse<CarData> { HasSuccess = true, Itens = new List<CarData>() });
            _carClientMock.Setup(c => c.GetHighSpeedsSession(sessionKey, 0, It.IsAny<CancellationToken>())).ReturnsAsync(carDataResponse);
            _carDataDaoMock.Setup(d => d.SaveCarDatas(It.IsAny<List<CarData>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Response { HasSuccess = true });
            _unityMock.Setup(u => u.Commit(It.IsAny<CancellationToken>())).ReturnsAsync(new Response { HasSuccess = true });

            var service = new CarDataService(_unityMock.Object, _carClientMock.Object, _driverServiceMock.Object);

            // Act
            var result = await service.GetSortedHighSpeedsSession(sessionKey, 0);

            // Assert
            Assert.IsTrue(result.HasSuccess);
            Assert.That(result.Itens.Count, Is.EqualTo(2));
            // Should be sorted descending by speed: driver 2 then driver 1
            Assert.That(result.Itens[0].Speed, Is.EqualTo(160));
            Assert.That(result.Itens[1].Speed, Is.EqualTo(150));
        }

        [Test]
        public async Task GetHighSpeedSessionDatabase_DaoFailure_ReturnsFailure()
        {
            // Arrange
            var dbFail = new DataResponse<CarData> { HasSuccess = false, Message = "dbfail" };
            _carDataDaoMock.Setup(d => d.GetHighSpeedSessionDatabase(7, 3)).ReturnsAsync(dbFail);

            var service = new CarDataService(_unityMock.Object, _carClientMock.Object, _driverServiceMock.Object);

            // Act
            var result = await service.GetHighSpeedSessionDatabase(7, 3);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("dbfail"));
        }

        [Test]
        public async Task GetHighSpeedSessionDatabase_DaoThrows_ReturnsFailureWithException()
        {
            // Arrange
            _carDataDaoMock.Setup(d => d.GetHighSpeedSessionDatabase(It.IsAny<int>(), It.IsAny<int>())).ThrowsAsync(new InvalidOperationException("dberr"));
            var service = new CarDataService(_unityMock.Object, _carClientMock.Object, _driverServiceMock.Object);

            // Act
            var result = await service.GetHighSpeedSessionDatabase(8, 4);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.IsNotNull(result.Exception);
            Assert.That(result.Exception.Message, Is.EqualTo("dberr"));
        }

        [Test]
        public async Task SaveCarDatas_SaveThrows_ReturnsFailureResponseWithException()
        {
            // Arrange
            var service = new CarDataService(_unityMock.Object, _carClientMock.Object, _driverServiceMock.Object);
            var data = new List<CarData> { new() };

            _carDataDaoMock.Setup(d => d.SaveCarDatas(It.IsAny<List<CarData>>())).ThrowsAsync(new InvalidOperationException("boom"));

            // Act
            var result = await service.SaveCarDatas(data);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.IsNotNull(result.Exception);
            Assert.IsInstanceOf<InvalidOperationException>(result.Exception);
            Assert.That(result.Exception.Message, Is.EqualTo("boom"));
            _carDataDaoMock.Verify(d => d.SaveCarDatas(It.Is<List<CarData>>(l => l == data)), Times.Once);
            _unityMock.Verify(u => u.Commit(), Times.Never);
        }

    }
}
