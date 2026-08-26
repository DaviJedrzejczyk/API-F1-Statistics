using Dao.Interface;
using Entities;
using Entities.Dtos;
using ExternalApi.Interfaces;
using Moq;
using Services.Impl;
using Shared.Responses;

namespace UnitTests.Service
{
    [TestFixture]
    public class DriverServiceTests
    {
        private Mock<IUnityOfWork> _unityOfWorkMock = null!;
        private Mock<IDriverDao> _driverDaoMock = null!;
        private Mock<IDriverClient> _driverClientMock = null!;
        private DriverInsertDTO _driverInsertDTO;

        [SetUp]
        public void SetUp()
        {
            _unityOfWorkMock = new Mock<IUnityOfWork>();
            _driverDaoMock = new Mock<IDriverDao>();
            _driverClientMock = new Mock<IDriverClient>();

            _unityOfWorkMock.Setup(u => u.DriverDao).Returns(_driverDaoMock.Object);

            _driverInsertDTO = new DriverInsertDTO(12);
        }

        [Test]
        public void Constructor_WithMocks_CreatesInstance()
        {
            // Arrange & Act
            var svc = new DriverService(_unityOfWorkMock.Object, _driverClientMock.Object);

            // Assert
            Assert.IsNotNull(svc);
            Assert.IsInstanceOf<DriverService>(svc);
        }

        [Test]
        public async Task DeleteDriver_NullDriver_ReturnsFailure()
        {
            // Arrange
            var svc = new DriverService(_unityOfWorkMock.Object, _driverClientMock.Object);

            // Act
            Response result = await svc.DeleteDriver(null!);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("Driver must be informed."));
        }

        [Test]
        public async Task DeleteDriver_DaoReturnsFailure_ReturnsFailureWithMessageAndException()
        {
            // Arrange
            var expectedEx = new InvalidOperationException("db error");
            var daoResponse = new Response("db fail", false, expectedEx);
            _driverDaoMock.Setup(d => d.DeleteDriver(It.IsAny<Driver>())).ReturnsAsync(daoResponse);

            var svc = new DriverService(_unityOfWorkMock.Object, _driverClientMock.Object);

            // Act
            Response result = await svc.DeleteDriver(new Driver());

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("db fail"));
            Assert.That(result.Exception, Is.EqualTo(expectedEx));
        }

        [Test]
        public async Task DeleteDriver_DaoReturnsSuccess_ReturnsSameResponse()
        {
            // Arrange
            var daoResponse = new Response("ok", true, null);
            _driverDaoMock.Setup(d => d.DeleteDriver(It.IsAny<Driver>())).ReturnsAsync(daoResponse);

            var svc = new DriverService(_unityOfWorkMock.Object, _driverClientMock.Object);

            // Act
            Response result = await svc.DeleteDriver(new Driver());

            // Assert
            Assert.IsTrue(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("ok"));
            Assert.IsNull(result.Exception);
        }

        [Test]
        public async Task DeleteDriver_DaoThrows_ReturnsFailureWithException()
        {
            // Arrange
            var expectedEx = new Exception("boom");
            _driverDaoMock.Setup(d => d.DeleteDriver(It.IsAny<Driver>())).ThrowsAsync(expectedEx);

            var svc = new DriverService(_unityOfWorkMock.Object, _driverClientMock.Object);

            // Act
            Response result = await svc.DeleteDriver(new Driver());

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo(expectedEx.Message));
            Assert.That(result.Exception, Is.EqualTo(expectedEx));
        }

        [Test]
        public async Task GetDriverById_IdLessOrEqualZero_ReturnsFailureSingleResponse()
        {
            // Arrange
            var svc = new DriverService(_unityOfWorkMock.Object, _driverClientMock.Object);

            // Act
            var result = await svc.GetDriverById(0);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("Id must be greater than 0"));
        }

        [Test]
        public async Task GetDriverById_DaoReturnsNullItem_ReturnsNotFoundFailure()
        {
            // Arrange
            var daoResponse = new SingleResponse<Driver> { HasSuccess = true, Message = "m", Item = null };
            _driverDaoMock.Setup(d => d.GetDriverById(It.IsAny<int>())).ReturnsAsync(daoResponse);

            var svc = new DriverService(_unityOfWorkMock.Object, _driverClientMock.Object);

            // Act
            var result = await svc.GetDriverById(1);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("Driver not found!"));
        }

        [Test]
        public async Task GetDriverById_DaoReturnsExceptionInResponse_ReturnsFailureWithMessageAndException()
        {
            // Arrange
            var expectedEx = new Exception("dao ex");
            var daoResponse = new SingleResponse<Driver> { HasSuccess = true, Message = "dao msg", Exception = expectedEx, Item = new Driver() };
            _driverDaoMock.Setup(d => d.GetDriverById(It.IsAny<int>())).ReturnsAsync(daoResponse);

            var svc = new DriverService(_unityOfWorkMock.Object, _driverClientMock.Object);

            // Act
            var result = await svc.GetDriverById(2);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("A error has ocurred while search the driver in database: dao msg"));
            Assert.That(result.Exception, Is.EqualTo(expectedEx));
        }

        [Test]
        public async Task GetDriverById_DaoThrows_ReturnsFailureWithException()
        {
            // Arrange
            var expectedEx = new Exception("bad");
            _driverDaoMock.Setup(d => d.GetDriverById(It.IsAny<int>())).ThrowsAsync(expectedEx);

            var svc = new DriverService(_unityOfWorkMock.Object, _driverClientMock.Object);

            // Act
            var result = await svc.GetDriverById(10);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo(expectedEx.Message));
            Assert.That(result.Exception, Is.EqualTo(expectedEx));
        }

        [Test]
        public async Task InsertDrivers_MeetingKeyFailure_ReturnsFailureResponse()
        {
            // Arrange
            var meetingResp = new DataResponse<Driver>("no", false, null, null);
            _driverClientMock.Setup(c => c.GetAllDriversSessionSelected(It.IsAny<DriverInsertDTO>())).ReturnsAsync(meetingResp);

            // Ensure the database search returns success so the service proceeds to call the external API
            _driverDaoMock.Setup(d => d.GetAllDriversSession(It.IsAny<int>())).ReturnsAsync(new DataResponse<Driver>() { HasSuccess = true, Itens = [] });

            var svc = new DriverService(_unityOfWorkMock.Object, _driverClientMock.Object);

            // Act
            var result = await svc.InsertDrivers(_driverInsertDTO);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("The Drivers in the most recent meeting was not found"));
        }

        [Test]
        public async Task InsertDrivers_NoDriversFound_ReturnsFailure()
        {
            // Arrange
            var meetingResp = new SingleResponse<int>("ok", true, null, 123);
            var dataResp = new DataResponse<Driver>("m", true, null, null);
            _driverClientMock.Setup(c => c.GetAllDriversSessionSelected(It.IsAny<DriverInsertDTO>())).ReturnsAsync(dataResp);

            var dbResp = new DataResponse<Driver>("m", true, null, []);
            _unityOfWorkMock.Setup(u => u.DriverDao.GetAllDriversSession(It.IsAny<int>())).ReturnsAsync(dbResp);

            var svc = new DriverService(_unityOfWorkMock.Object, _driverClientMock.Object);

            // Act
            var result = await svc.InsertDrivers(new DriverInsertDTO(123));

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("The Drivers in the most recent meeting was not found"));
        }

        [Test]
        public async Task InsertDrivers_InsertFails_ReturnsFailureWithMessageAndException()
        {
            // Arrange
            var meetingResp = new SingleResponse<int>("ok", true, null, 123);


            var drivers = new List<Driver> { new Driver() };
            var dataResp = new DataResponse<Driver>("ok", true, null, drivers);
            _driverClientMock.Setup(c => c.GetAllDriversSessionSelected(It.IsAny<DriverInsertDTO>())).ReturnsAsync(dataResp);

            var insertResp = new Response("insert msg", false, new Exception("ins ex"));
            _driverDaoMock.Setup(d => d.InsertDrivers(It.IsAny<List<Driver>>())).ReturnsAsync(insertResp);
            _driverDaoMock.Setup(d => d.GetAllDriversSession(14)).ReturnsAsync(new DataResponse<Driver>() { HasSuccess = true, Itens = [] });

            var svc = new DriverService(_unityOfWorkMock.Object, _driverClientMock.Object);

            // Act
            var result = await svc.InsertDrivers(new DriverInsertDTO(14));

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("Failed to insert the driver(s): insert msg"));
            Assert.IsNotNull(result.Exception);
        }

        [Test]
        public async Task InsertDrivers_Success_ReturnsSuccessMessage()
        {
            // Arrange
            var meetingResp = new SingleResponse<int>("ok", true, null, 123);


            var drivers = new List<Driver> { new Driver() };
            var dataResp = new DataResponse<Driver>("ok", true, null, drivers);
            _driverClientMock.Setup(c => c.GetAllDriversSessionSelected(It.IsAny<DriverInsertDTO>())).ReturnsAsync(dataResp);

            var insertResp = new Response("ok", true, null);
            _driverDaoMock.Setup(d => d.InsertDrivers(It.IsAny<List<Driver>>())).ReturnsAsync(insertResp);

            // Ensure the database search returns success and an empty list so insertion proceeds
            _driverDaoMock.Setup(d => d.GetAllDriversSession(_driverInsertDTO.SessionKey))
                .ReturnsAsync(new DataResponse<Driver>() { HasSuccess = true, Itens = [] });

            // Ensure commit succeeds
            _unityOfWorkMock.Setup(u => u.Commit()).ReturnsAsync(new Response("ok", true, null));

            var svc = new DriverService(_unityOfWorkMock.Object, _driverClientMock.Object);

            // Act
            var result = await svc.InsertDrivers(_driverInsertDTO);

            // Assert
            Assert.IsTrue(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("The new driver(s) have been inserted!"));
        }

        [Test]
        public async Task InsertDrivers_MeetingServiceThrows_ReturnsFailureWithException()
        {
            // Arrange
            var expectedEx = new Exception("meet fail");
            _driverClientMock.Setup(c => c.GetAllDriversSessionSelected(It.IsAny<DriverInsertDTO>())).ThrowsAsync(expectedEx);
            // Ensure the database search returns success so the service proceeds to call the external API
            _driverDaoMock.Setup(d => d.GetAllDriversSession(It.IsAny<int>())).ReturnsAsync(new DataResponse<Driver>() { HasSuccess = true, Itens = [] });
            var svc = new DriverService(_unityOfWorkMock.Object, _driverClientMock.Object);

            // Act
            var result = await svc.InsertDrivers(_driverInsertDTO);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo(expectedEx.Message));
            Assert.That(result.Exception, Is.EqualTo(expectedEx));
        }
    }
}
