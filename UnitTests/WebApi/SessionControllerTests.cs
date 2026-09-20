using Microsoft.AspNetCore.Mvc;
using WebApi.ViewModels;
using WebApi.Controllers.Sessions;
using Moq;
using Services.Interfaces;
using Shared.Responses;
using Entities.Class;

namespace UnitTests.WebApi
{
    [TestFixture]
    public class SessionControllerTests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public async Task SessionController_Constructor_WithService_CallsServiceInsertSessions()
        {
            // Arrange
            var serviceMock = new Mock<ISessionService>();
            serviceMock.Setup(s => s.InsertSessions(It.IsAny<int>()))
                .Returns(Task.FromResult(new Response("ok", true, null)));

            var controller = new SessionController(serviceMock.Object);

            // Act
            var result = await controller.InsertSessionsInDataBase(new SessionMeetingKeyViewModel { MeetingKey = 1 });

            // Assert
            Assert.IsInstanceOf(typeof(OkObjectResult), result);
            var ok = (OkObjectResult)result;
            var success = (SuccessViewModel)ok.Value!;
            Assert.AreEqual(200, success.StatusCode);
            Assert.AreEqual("ok", success.Message);
            serviceMock.Verify(s => s.InsertSessions(1), Times.Once());
        }

        [Test]
        public async Task SessionController_InsertSessionsInDataBase_ServiceReturnsFailure_ReturnsBadRequest()
        {
            // Arrange
            var serviceMock = new Mock<ISessionService>();
            serviceMock.Setup(s => s.InsertSessions(It.IsAny<int>()))
                .Returns(Task.FromResult(new Response("fail", false, null)));

            var controller = new SessionController(serviceMock.Object);

            // Act
            var result = await controller.InsertSessionsInDataBase(new SessionMeetingKeyViewModel { MeetingKey = 5 });

            // Assert
            Assert.IsInstanceOf(typeof(BadRequestObjectResult), result);
            var bad = (BadRequestObjectResult)result;
            var error = (ErrorViewModel)bad.Value!;
            Assert.AreEqual(400, error.StatusCode);
            Assert.AreEqual("fail", error.Message);
        }

        [Test]
        public async Task SessionController_InsertSessionsInDataBase_ServiceThrowsException_ReturnsBadRequestWithMessage()
        {
            // Arrange
            var serviceMock = new Mock<ISessionService>();
            serviceMock.Setup(s => s.InsertSessions(It.IsAny<int>()))
                .Returns(Task.FromException<Response>(new Exception("boom")));

            var controller = new SessionController(serviceMock.Object);

            // Act
            var result = await controller.InsertSessionsInDataBase(new SessionMeetingKeyViewModel { MeetingKey = 7 });

            // Assert
            Assert.IsInstanceOf(typeof(BadRequestObjectResult), result);
            var bad = (BadRequestObjectResult)result;
            var message = (string)bad.Value!;
            Assert.AreEqual("boom", message);
        }

        [Test]
        public async Task SessionController_Constructor_NullService_ReturnsBadRequestOnInsert()
        {
            // Arrange
            SessionController controller = new SessionController(null!);

            // Act
            var result = await controller.InsertSessionsInDataBase(new SessionMeetingKeyViewModel { MeetingKey = 1 });

            // Assert
            Assert.IsInstanceOf(typeof(BadRequestObjectResult), result);
            var bad = (BadRequestObjectResult)result;
            Assert.IsInstanceOf(typeof(string), bad.Value);
            var msg = (string)bad.Value!;
            Assert.IsNotEmpty(msg);
        }

        [Test]
        public async Task GetSessionById_ReturnsNotFound_WhenNotFound()
        {
            // Arrange
            var serviceMock = new Mock<ISessionService>();
            var resp = new SingleResponse<Entities.Class.Session>() { Message = "no item", HasSuccess = false, Exception = null, Item = default! };
            serviceMock.Setup(s => s.GetSessionByMeetingKeySessionKey(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(resp));

            var controller = new SessionController(serviceMock.Object);

            // Act
            var result = await controller.GetSessionBySessionKey(1, 2);

            // Assert
            Assert.IsInstanceOf(typeof(NotFoundObjectResult), result);
            var notFound = (NotFoundObjectResult)result;
            var error = (ErrorViewModel)notFound.Value!;
            Assert.AreEqual(404, error.StatusCode);
            Assert.AreEqual("no item", error.Message);
        }

        [Test]
        public async Task GetSessionById_ReturnsBadRequest_WhenExceptionPresent()
        {
            // Arrange
            var serviceMock = new Mock<ISessionService>();
            var resp = new SingleResponse<Session>() { Message = "bad", HasSuccess = false, Exception = new Exception("x"), Item = new Session() };
            serviceMock.Setup(s => s.GetSessionByMeetingKeySessionKey(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(resp));

            var controller = new SessionController(serviceMock.Object);

            // Act
            var result = await controller.GetSessionBySessionKey(10, 20);

            // Assert
            Assert.IsInstanceOf(typeof(BadRequestObjectResult), result);
            var bad = (BadRequestObjectResult)result;
            var error = (ErrorViewModel)bad.Value!;
            Assert.AreEqual(400, error.StatusCode);
            Assert.AreEqual("bad", error.Message);
        }

        [Test]
        public async Task GetSessionById_ReturnsOk_WhenSuccess()
        {
            // Arrange
            var serviceMock = new Mock<ISessionService>();
            var resp = new SingleResponse<Entities.Class.Session>("ok session", true, null, new Entities.Class.Session());
            serviceMock.Setup(s => s.GetSessionByMeetingKeySessionKey(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(resp));

            var controller = new SessionController(serviceMock.Object);

            // Act
            var result = await controller.GetSessionBySessionKey(3, 4);

            // Assert
            Assert.IsInstanceOf(typeof(OkObjectResult), result);
            var ok = (OkObjectResult)result;
            var success = (SuccessViewModel)ok.Value!;
            Assert.AreEqual(200, success.StatusCode);
            Assert.AreEqual("ok session", success.Message);
        }

    }
}
