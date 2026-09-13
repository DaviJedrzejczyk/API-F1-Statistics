using AutoMapper;
using Entities.Class;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Services.Interfaces;
using Shared.Responses;
using WebApi.Controllers.Meetings;
using WebApi.ViewModels;

namespace UnitTests.WebApi
{
    [TestFixture]
    public class MeetingControllerTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task MeetingController_ValidDependencies_InstanceCreatedAndInsertAllTracksCalled()
        {
            // Arrange
            var meetingServiceMock = new Mock<IMeetingService>();
            var mapperMock = new Mock<IMapper>();

            var model = new MeetingYearViewModel { Year = 2023 };

            meetingServiceMock
                .Setup(s => s.InsertTracksOfCurrentYear(It.IsAny<int>()))
                .ReturnsAsync(new Response { HasSuccess = true, Message = "inserted" });

            var controller = new MeetingController(meetingServiceMock.Object, mapperMock.Object);

            // Act
            var result = await controller.InsertAllTracksOfCurrentSeason(model);

            // Assert
            meetingServiceMock.Verify(s => s.InsertTracksOfCurrentYear(2023), Times.Once);
            Assert.IsInstanceOf<OkObjectResult>(result);
            var ok = (OkObjectResult)result;
            Assert.IsInstanceOf<SuccessViewModel>(ok.Value);
            Assert.IsNotNull(ok.Value);
            var vm = (SuccessViewModel)ok.Value!;
            Assert.That(vm.StatusCode, Is.EqualTo(200));
            Assert.That(vm.Message, Is.EqualTo("inserted"));
        }

        [Test]
        public async Task InsertAllTracksOfCurrentSeason_ServiceReturnsFailure_ReturnsBadRequestWithMessage()
        {
            // Arrange
            var meetingServiceMock = new Mock<IMeetingService>();
            var mapperMock = new Mock<IMapper>();

            var model = new MeetingYearViewModel { Year = 2021 };

            meetingServiceMock
                .Setup(s => s.InsertTracksOfCurrentYear(It.IsAny<int>()))
                .ReturnsAsync(new Response { HasSuccess = false, Message = "failed" });

            var controller = new MeetingController(meetingServiceMock.Object, mapperMock.Object);

            // Act
            var result = await controller.InsertAllTracksOfCurrentSeason(model);

            // Assert
            meetingServiceMock.Verify(s => s.InsertTracksOfCurrentYear(2021), Times.Once);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var bad = (BadRequestObjectResult)result;
            Assert.That(bad.Value, Is.EqualTo("failed"));
        }

        [Test]
        public async Task InsertAllTracksOfCurrentSeason_ServiceThrowsException_ReturnsBadRequestWithExceptionMessage()
        {
            // Arrange
            var meetingServiceMock = new Mock<IMeetingService>();
            var mapperMock = new Mock<IMapper>();

            var model = new MeetingYearViewModel { Year = 2000 };

            meetingServiceMock
                .Setup(s => s.InsertTracksOfCurrentYear(It.IsAny<int>()))
                .ThrowsAsync(new Exception("boom"));

            var controller = new MeetingController(meetingServiceMock.Object, mapperMock.Object);

            // Act
            var result = await controller.InsertAllTracksOfCurrentSeason(model);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var bad = (BadRequestObjectResult)result;
            Assert.That(bad.Value, Is.EqualTo("boom"));
        }

        [Test]
        public async Task GetMeetingByKey_ItemNotFound_ReturnsNotFoundWithMessage()
        {
            // Arrange
            var meetingServiceMock = new Mock<IMeetingService>();
            var mapperMock = new Mock<IMapper>();

            meetingServiceMock
                .Setup(s => s.GetMeetingByKey(It.IsAny<int>()))
                .ReturnsAsync(new SingleResponse<Meeting> { Item = null!, Message = "nope" });

            var controller = new MeetingController(meetingServiceMock.Object, mapperMock.Object);

            // Act
            var result = await controller.GetMeetingByKey(5);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFound = (NotFoundObjectResult)result;
            Assert.That(notFound.Value, Is.EqualTo("nope"));
        }

        [Test]
        public async Task GetMeetingByKey_ItemFound_ReturnsOkWithMappedViewModel()
        {
            // Arrange
            var meetingServiceMock = new Mock<IMeetingService>();
            var mapperMock = new Mock<IMapper>();

            var meeting = new Meeting { /* properties if needed */ };
            var mapped = new MeetingViewModel { MeetingKey = 123 };

            meetingServiceMock
                .Setup(s => s.GetMeetingByKey(It.IsAny<int>()))
                .ReturnsAsync(new SingleResponse<Meeting> { Item = meeting });

            mapperMock
                .Setup(m => m.Map<MeetingViewModel>(It.IsAny<Meeting>()))
                .Returns(mapped);

            var controller = new MeetingController(meetingServiceMock.Object, mapperMock.Object);

            // Act
            var result = await controller.GetMeetingByKey(7);

            // Assert
            meetingServiceMock.Verify(s => s.GetMeetingByKey(7), Times.Once);
            mapperMock.Verify(m => m.Map<MeetingViewModel>(meeting), Times.Once);
            Assert.IsInstanceOf<OkObjectResult>(result);
            var ok = (OkObjectResult)result;
            Assert.That(ok.Value, Is.SameAs(mapped));
        }

        [Test]
        public async Task GetMeetingByKey_ServiceThrowsException_ReturnsBadRequestWithExceptionMessage()
        {
            // Arrange
            var meetingServiceMock = new Mock<IMeetingService>();
            var mapperMock = new Mock<IMapper>();

            meetingServiceMock
                .Setup(s => s.GetMeetingByKey(It.IsAny<int>()))
                .ThrowsAsync(new Exception("err"));

            var controller = new MeetingController(meetingServiceMock.Object, mapperMock.Object);

            // Act
            var result = await controller.GetMeetingByKey(8);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var bad = (BadRequestObjectResult)result;
            Assert.That(bad.Value, Is.EqualTo("err"));
        }

        [Test]
        public async Task GetAllTracksOfCurrentYear_ServiceReturnsFailure_ReturnsBadRequestWithResponse()
        {
            // Arrange
            var meetingServiceMock = new Mock<IMeetingService>();
            var mapperMock = new Mock<IMapper>();

            var response = new DataResponse<Meeting> { HasSuccess = false, Message = "bad" };

            meetingServiceMock
                .Setup(s => s.GetAllTracksOfCurrentYear(It.IsAny<int>()))
                .ReturnsAsync(response);

            var controller = new MeetingController(meetingServiceMock.Object, mapperMock.Object);

            // Act
            var result = await controller.GetAllTracksOfCurrentYear(2022);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var bad = (BadRequestObjectResult)result;
            Assert.That(bad.Value, Is.SameAs(response));
        }

        [Test]
        public async Task GetAllTracksOfCurrentYear_ServiceReturnsSuccess_ReturnsOkWithMappedList()
        {
            // Arrange
            var meetingServiceMock = new Mock<IMeetingService>();
            var mapperMock = new Mock<IMapper>();

            var meetings = new List<Meeting> { new Meeting(), new Meeting() };
            var response = new DataResponse<Meeting> { HasSuccess = true, Itens = meetings };

            var mappedList = new List<MeetingViewModel> { new MeetingViewModel { MeetingKey = 1 }, new MeetingViewModel { MeetingKey = 2 } };

            meetingServiceMock
                .Setup(s => s.GetAllTracksOfCurrentYear(It.IsAny<int>()))
                .ReturnsAsync(response);

            mapperMock
                .Setup(m => m.Map<List<MeetingViewModel>>(It.IsAny<List<Meeting>>()))
                .Returns(mappedList);

            var controller = new MeetingController(meetingServiceMock.Object, mapperMock.Object);

            // Act
            var result = await controller.GetAllTracksOfCurrentYear(2019);

            // Assert
            meetingServiceMock.Verify(s => s.GetAllTracksOfCurrentYear(2019), Times.Once);
            mapperMock.Verify(m => m.Map<List<MeetingViewModel>>(meetings), Times.Once);
            Assert.IsInstanceOf<OkObjectResult>(result);
            var ok = (OkObjectResult)result;
            Assert.That(ok.Value, Is.SameAs(mappedList));
        }

        [Test]
        public async Task GetAllTracksOfCurrentYear_ServiceThrowsException_ReturnsBadRequestWithExceptionMessage()
        {
            // Arrange
            var meetingServiceMock = new Mock<IMeetingService>();
            var mapperMock = new Mock<IMapper>();

            meetingServiceMock
                .Setup(s => s.GetAllTracksOfCurrentYear(It.IsAny<int>()))
                .ThrowsAsync(new Exception("explode"));

            var controller = new MeetingController(meetingServiceMock.Object, mapperMock.Object);

            // Act
            var result = await controller.GetAllTracksOfCurrentYear(0);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var bad = (BadRequestObjectResult)result;
            Assert.That(bad.Value, Is.EqualTo("explode"));
        }
    }
}
