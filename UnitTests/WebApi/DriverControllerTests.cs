using AutoMapper;
using Entities.Class;
using Entities.Dtos;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Services.Interfaces;
using Shared.Responses;
using WebApi.Controllers.Drivers;
using WebApi.ViewModels;

namespace UnitTests.WebApi
{
    [TestFixture]
    public class DriverControllerTests
    {
        [Test]
        public void Constructor_WithDependencies_DoesNotThrow()
        {
            // Arrange
            var serviceMock = new Mock<IDriverService>();
            var mapperMock = new Mock<IMapper>();

            // Act
            var controller = new DriverController(serviceMock.Object, mapperMock.Object);

            // Assert
            Assert.IsNotNull(controller);
            Assert.IsInstanceOf<DriverController>(controller);
        }

        [Test]
        public async Task InsertDrivers_ServiceReturnsFailure_ReturnsBadRequestWithErrorViewModel()
        {
            // Arrange
            var serviceMock = new Mock<IDriverService>();
            var mapperMock = new Mock<IMapper>();

            var response = new Response("failed", false, new System.Exception("fail"));
            serviceMock.Setup(s => s.InsertDrivers(It.IsAny<DriverInsertDTO>())).ReturnsAsync(response);

            var controller = new DriverController(serviceMock.Object, mapperMock.Object);

            // Act
            var result = await controller.InsertDrivers(new DriverInsertDTO());

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var bad = (BadRequestObjectResult)result;
            Assert.IsInstanceOf<ErrorViewModel>(bad.Value);
            Assert.IsNotNull(bad.Value);
            var vm = (ErrorViewModel)bad.Value!;
            Assert.That(vm.StatusCode, Is.EqualTo(400));
            Assert.That(vm.Message, Is.EqualTo("failed"));
        }

        [Test]
        public async Task InsertDrivers_ServiceReturnsSuccess_ReturnsOkWithSuccessViewModel()
        {
            // Arrange
            var serviceMock = new Mock<IDriverService>();
            var mapperMock = new Mock<IMapper>();

            var response = new Response("ok", true, new System.Exception("ok"));
            serviceMock.Setup(s => s.InsertDrivers(It.IsAny<DriverInsertDTO>())).ReturnsAsync(response);

            var controller = new DriverController(serviceMock.Object, mapperMock.Object);

            // Act
            var result = await controller.InsertDrivers(new DriverInsertDTO());

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var ok = (OkObjectResult)result;
            Assert.IsInstanceOf<SuccessViewModel>(ok.Value);
            Assert.IsNotNull(ok.Value);
            var vm = (SuccessViewModel)ok.Value!;
            Assert.That(vm.StatusCode, Is.EqualTo(200));
            Assert.That(vm.Message, Is.EqualTo("ok"));
        }

        [Test]
        public async Task GetAllDriversSession_ServiceThrowsExceptionInResponse_ReturnsBadRequest()
        {
            // Arrange
            var serviceMock = new Mock<IDriverService>();
            var mapperMock = new Mock<IMapper>();

            var dataResponse = new DataResponse<Driver> { Message = "err", HasSuccess = false, Exception = new System.Exception("x"), Itens = null! };
            serviceMock.Setup(s => s.GetAllDriversSession(It.IsAny<int>())).ReturnsAsync(dataResponse);

            var controller = new DriverController(serviceMock.Object, mapperMock.Object);

            // Act
            var result = await controller.GetAllDriversSession(1);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var bad = (BadRequestObjectResult)result;
            Assert.IsInstanceOf<ErrorViewModel>(bad.Value);
            Assert.IsNotNull(bad.Value);
            var vm = (ErrorViewModel)bad.Value!;
            Assert.That(vm.StatusCode, Is.EqualTo(400));
            Assert.That(vm.Message, Is.EqualTo("err"));
        }

        [Test]
        public async Task GetAllDriversSession_ItemsNull_ReturnsBadRequest()
        {
            // Arrange
            var serviceMock = new Mock<IDriverService>();
            var mapperMock = new Mock<IMapper>();

            var dataResponse = new DataResponse<Driver> { Message = "noitems", HasSuccess = true, Exception = null!, Itens = null! };
            serviceMock.Setup(s => s.GetAllDriversSession(It.IsAny<int>())).ReturnsAsync(dataResponse);

            var controller = new DriverController(serviceMock.Object, mapperMock.Object);

            // Act
            var result = await controller.GetAllDriversSession(2);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var bad = (BadRequestObjectResult)result;
            Assert.IsInstanceOf<ErrorViewModel>(bad.Value);
            Assert.IsNotNull(bad.Value);
            var vm = (ErrorViewModel)bad.Value!;
            Assert.That(vm.StatusCode, Is.EqualTo(400));
            Assert.That(vm.Message, Is.EqualTo("noitems"));
        }

        [Test]
        public async Task GetAllDriversSession_ItemsEmpty_ReturnsNotFound()
        {
            // Arrange
            var serviceMock = new Mock<IDriverService>();
            var mapperMock = new Mock<IMapper>();

            var dataResponse = new DataResponse<Driver> { Message = "none", HasSuccess = true, Exception = null!, Itens = new List<Driver>() };
            serviceMock.Setup(s => s.GetAllDriversSession(It.IsAny<int>())).ReturnsAsync(dataResponse);

            var controller = new DriverController(serviceMock.Object, mapperMock.Object);

            // Act
            var result = await controller.GetAllDriversSession(3);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFound = (NotFoundObjectResult)result;
            Assert.IsInstanceOf<ErrorViewModel>(notFound.Value);
            Assert.IsNotNull(notFound.Value);
            var vm = (ErrorViewModel)notFound.Value!;
            Assert.That(vm.StatusCode, Is.EqualTo(404));
            Assert.That(vm.Message, Is.EqualTo("none"));
        }

        [Test]
        public async Task GetAllDriversSession_ItemsFound_ReturnsOkWithMappedViewModels()
        {
            // Arrange
            var serviceMock = new Mock<IDriverService>();
            var mapperMock = new Mock<IMapper>();

            var driver = new Driver { DriverKey = 1, BroadcastName = "B", DriverNumber = 44, FirstName = "F", FullName = "Full", HeadshotUrl = "url", LastName = "L", MeetingKey = 10, NameAcronym = "NA", SessionKey = 20, TeamColour = "red", TeamName = "Team" };
            var drivers = new List<Driver> { driver };
            var dataResponse = new DataResponse<Driver> { Message = "ok", HasSuccess = true, Exception = null!, Itens = drivers };
            serviceMock.Setup(s => s.GetAllDriversSession(It.IsAny<int>())).ReturnsAsync(dataResponse);

            var mapped = new List<DriverListViewModel> { new DriverListViewModel { BroadcastName = "B", DriverNumber = 44, FirstName = "F", FullName = "Full", HeadshotUrl = "url", LastName = "L", MeetingKey = 10, NameAcronym = "NA", SessionKey = 20, TeamColour = "red", TeamName = "Team" } };
            mapperMock.Setup(m => m.Map<List<DriverListViewModel>>(It.IsAny<object>())).Returns(mapped);

            var controller = new DriverController(serviceMock.Object, mapperMock.Object);

            // Act
            var result = await controller.GetAllDriversSession(4);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var ok = (OkObjectResult)result;
            Assert.IsInstanceOf<List<DriverListViewModel>>(ok.Value);
            Assert.IsNotNull(ok.Value);
            var list = (List<DriverListViewModel>)ok.Value!;
            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0].BroadcastName, Is.EqualTo("B"));
            Assert.That(list[0].DriverNumber, Is.EqualTo(44));
        }
    }
}
