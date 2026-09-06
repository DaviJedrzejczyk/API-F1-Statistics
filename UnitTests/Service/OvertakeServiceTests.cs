using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Dao.Interface;
using ExternalApi.Interfaces;
using Services.Interfaces;
using Services.Impl;
using Entities;
using Shared.Responses;
using Entities.Dtos.RaceControlDTOs;

namespace UnitTests.Service
{
    [TestFixture]
    public class OvertakeServiceTests
    {
        [Test]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            // Arrange
            var clientMock = new Mock<IOvertakeClient>();
            var uowMock = new Mock<IUnityOfWork>();
            var pitServiceMock = new Mock<IPitService>();
            var raceControlMock = new Mock<IRaceControlService>();

            // Act
            var svc = new OvertakeService(clientMock.Object, uowMock.Object, pitServiceMock.Object, raceControlMock.Object);

            // Assert
            Assert.That(svc, Is.Not.Null);
            Assert.That(svc, Is.InstanceOf<OvertakeService>());
        }

        [Test]
        public async Task GetOvertakesSessionApi_DatabaseHasItems_ReturnsDbResponse()
        {
            // Arrange
            var clientMock = new Mock<IOvertakeClient>();
            var uowMock = new Mock<IUnityOfWork>();
            var overtakeDaoMock = new Mock<IOvertakeDao>();
            var pitServiceMock = new Mock<IPitService>();
            var raceControlMock = new Mock<IRaceControlService>();

            var dbResponse = new DataResponse<Overtake> { HasSuccess = true, Itens = new List<Overtake> { new Overtake() } };
            overtakeDaoMock.Setup(d => d.GetOvertakesBySession(It.IsAny<int>())).ReturnsAsync(dbResponse);
            uowMock.Setup(u => u.OvertakeDao).Returns(overtakeDaoMock.Object);

            var svc = new OvertakeService(clientMock.Object, uowMock.Object, pitServiceMock.Object, raceControlMock.Object);

            // Act
            var result = await svc.GetOvertakesSessionApi(1);

            // Assert
            Assert.That(result, Is.SameAs(dbResponse));
            clientMock.Verify(c => c.GetOvertakesSession(It.IsAny<int>()), Times.Never);
        }

        [Test]
        public async Task GetOvertakesSessionApi_ApiReturnsFailure_ReturnsApiResponse()
        {
            // Arrange
            var clientMock = new Mock<IOvertakeClient>();
            var uowMock = new Mock<IUnityOfWork>();
            var overtakeDaoMock = new Mock<IOvertakeDao>();
            var pitServiceMock = new Mock<IPitService>();
            var raceControlMock = new Mock<IRaceControlService>();

            // DB returns empty list
            var dbResponse = new DataResponse<Overtake> { HasSuccess = true, Itens = new List<Overtake>() };
            overtakeDaoMock.Setup(d => d.GetOvertakesBySession(It.IsAny<int>())).ReturnsAsync(dbResponse);
            uowMock.Setup(u => u.OvertakeDao).Returns(overtakeDaoMock.Object);

            var apiResponse = new DataResponse<Overtake> { HasSuccess = false, Message = "api fail" };
            clientMock.Setup(c => c.GetOvertakesSession(It.IsAny<int>())).ReturnsAsync(apiResponse);

            var svc = new OvertakeService(clientMock.Object, uowMock.Object, pitServiceMock.Object, raceControlMock.Object);

            // Act
            var result = await svc.GetOvertakesSessionApi(2);

            // Assert
            Assert.That(result, Is.SameAs(apiResponse));
        }

        [Test]
        public async Task GetOvertakesSessionApi_RaceControlFailure_ReturnsFailureDataResponse()
        {
            // Arrange
            var clientMock = new Mock<IOvertakeClient>();
            var uowMock = new Mock<IUnityOfWork>();
            var overtakeDaoMock = new Mock<IOvertakeDao>();
            var pitServiceMock = new Mock<IPitService>();
            var raceControlMock = new Mock<IRaceControlService>();

            // DB returns empty
            var dbResponse = new DataResponse<Overtake> { HasSuccess = true, Itens = new List<Overtake>() };
            overtakeDaoMock.Setup(d => d.GetOvertakesBySession(It.IsAny<int>())).ReturnsAsync(dbResponse);
            uowMock.Setup(u => u.OvertakeDao).Returns(overtakeDaoMock.Object);

            // API returns some overtakes
            var overtakesApi = new List<Overtake> { new Overtake { Date = DateTime.UtcNow, OvertakingDriverNumber = 1, OvertakedDriverNumber = 2 } };
            var apiResponse = new DataResponse<Overtake> { HasSuccess = true, Itens = overtakesApi };
            clientMock.Setup(c => c.GetOvertakesSession(It.IsAny<int>())).ReturnsAsync(apiResponse);

            // pit service returns empty pits (so nothing is filtered)
            var pitsResponse = new DataResponse<Pit> { HasSuccess = true, Itens = new List<Pit>() };
            pitServiceMock.Setup(p => p.GetPitsBySessionKeyApi(It.IsAny<int>())).ReturnsAsync(pitsResponse);

            // race control returns failure
            var rcResponse = new DataResponse<RaceControlFilterDto> { HasSuccess = false, Message = "rc fail" };
            raceControlMock.Setup(r => r.GetRaceControlsBySessionFlags(It.IsAny<int>(), It.IsAny<string[]>())).ReturnsAsync(rcResponse);

            var svc = new OvertakeService(clientMock.Object, uowMock.Object, pitServiceMock.Object, raceControlMock.Object);

            // Act
            var result = await svc.GetOvertakesSessionApi(3);

            // Assert
            Assert.That(result.HasSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("rc fail"));
            // Save should not be called when race control fails
            overtakeDaoMock.Verify(d => d.SaveOvertakes(It.IsAny<List<Overtake>>()), Times.Never);
        }

        [Test]
        public async Task GetOvertakesSessionDb_NoItems_SetsMessageAndReturns()
        {
            // Arrange
            var clientMock = new Mock<IOvertakeClient>();
            var uowMock = new Mock<IUnityOfWork>();
            var overtakeDaoMock = new Mock<IOvertakeDao>();

            var dbResponse = new DataResponse<Overtake> { Itens = null };
            overtakeDaoMock.Setup(d => d.GetOvertakesBySession(It.IsAny<int>())).ReturnsAsync(dbResponse);
            uowMock.Setup(u => u.OvertakeDao).Returns(overtakeDaoMock.Object);

            var svc = new OvertakeService(clientMock.Object, uowMock.Object, new Mock<IPitService>().Object, new Mock<IRaceControlService>().Object);

            // Act
            var result = await svc.GetOvertakesSessionDb(5);

            // Assert
            Assert.That(result.Message, Is.EqualTo("The overtakes of session not found or dosent have."));
        }

        [Test]
        public async Task GetOvertakesSessionDb_Exception_ReturnsFailureDataResponse()
        {
            // Arrange
            var clientMock = new Mock<IOvertakeClient>();
            var uowMock = new Mock<IUnityOfWork>();
            var overtakeDaoMock = new Mock<IOvertakeDao>();

            overtakeDaoMock.Setup(d => d.GetOvertakesBySession(It.IsAny<int>())).ThrowsAsync(new InvalidOperationException("boom"));
            uowMock.Setup(u => u.OvertakeDao).Returns(overtakeDaoMock.Object);

            var svc = new OvertakeService(clientMock.Object, uowMock.Object, new Mock<IPitService>().Object, new Mock<IRaceControlService>().Object);

            // Act
            var result = await svc.GetOvertakesSessionDb(6);

            // Assert
            Assert.That(result.HasSuccess, Is.False);
            Assert.That(result.Exception, Is.Not.Null);
            Assert.That(result.Exception, Is.InstanceOf<InvalidOperationException>());
        }

        [Test]
        public async Task SaveOvertakes_NullOrEmpty_ReturnsFailureResponse()
        {
            // Arrange
            var clientMock = new Mock<IOvertakeClient>();
            var uowMock = new Mock<IUnityOfWork>();
            var pitServiceMock = new Mock<IPitService>();
            var raceControlMock = new Mock<IRaceControlService>();

            var svc = new OvertakeService(clientMock.Object, uowMock.Object, pitServiceMock.Object, raceControlMock.Object);

            // Act
#pragma warning disable CS8625 // Suppress nullability warning for test
            var nullResult = await svc.SaveOvertakes((List<Overtake>?)null);
#pragma warning restore CS8625
            var emptyResult = await svc.SaveOvertakes(new List<Overtake>());

            // Assert
            Assert.That(nullResult.HasSuccess, Is.False);
            Assert.That(nullResult.Message, Is.EqualTo("Overtakes must be informed."));

            Assert.That(emptyResult.HasSuccess, Is.False);
            Assert.That(emptyResult.Message, Is.EqualTo("Overtakes must be informed."));
        }

        [Test]
        public async Task SaveOvertakes_SaveFails_ReturnsDaoResponse()
        {
            // Arrange
            var clientMock = new Mock<IOvertakeClient>();
            var uowMock = new Mock<IUnityOfWork>();
            var overtakeDaoMock = new Mock<IOvertakeDao>();
            var pitServiceMock = new Mock<IPitService>();
            var raceControlMock = new Mock<IRaceControlService>();

            var failResponse = new Response { HasSuccess = false, Message = "save failed" };
            overtakeDaoMock.Setup(d => d.SaveOvertakes(It.IsAny<List<Overtake>>())).ReturnsAsync(failResponse);
            uowMock.Setup(u => u.OvertakeDao).Returns(overtakeDaoMock.Object);

            var svc = new OvertakeService(clientMock.Object, uowMock.Object, pitServiceMock.Object, raceControlMock.Object);
            var list = new List<Overtake> { new Overtake() };

            // Act
            var result = await svc.SaveOvertakes(list);

            // Assert
            Assert.That(result, Is.SameAs(failResponse));
        }

        [Test]
        public async Task SaveOvertakes_Success_CommitsAndReturnsCommitResult()
        {
            // Arrange
            var clientMock = new Mock<IOvertakeClient>();
            var uowMock = new Mock<IUnityOfWork>();
            var overtakeDaoMock = new Mock<IOvertakeDao>();
            var pitServiceMock = new Mock<IPitService>();
            var raceControlMock = new Mock<IRaceControlService>();

            var successSave = new Response { HasSuccess = true, Message = "saved" };
            var commitResponse = new Response { HasSuccess = true, Message = "committed" };
            overtakeDaoMock.Setup(d => d.SaveOvertakes(It.IsAny<List<Overtake>>())).ReturnsAsync(successSave);
            uowMock.Setup(u => u.OvertakeDao).Returns(overtakeDaoMock.Object);
            uowMock.Setup(u => u.Commit()).ReturnsAsync(commitResponse);

            var svc = new OvertakeService(clientMock.Object, uowMock.Object, pitServiceMock.Object, raceControlMock.Object);
            var list = new List<Overtake> { new Overtake() };

            // Act
            var result = await svc.SaveOvertakes(list);

            // Assert
            Assert.That(result, Is.SameAs(commitResponse));
            overtakeDaoMock.Verify(d => d.SaveOvertakes(It.Is<List<Overtake>>(l => l == list)), Times.Once);
            uowMock.Verify(u => u.Commit(), Times.Once);
        }

        [Test]
        public async Task SaveOvertakes_Exception_ReturnsFailureResponse()
        {
            // Arrange
            var clientMock = new Mock<IOvertakeClient>();
            var uowMock = new Mock<IUnityOfWork>();
            var overtakeDaoMock = new Mock<IOvertakeDao>();
            var pitServiceMock = new Mock<IPitService>();
            var raceControlMock = new Mock<IRaceControlService>();

            overtakeDaoMock.Setup(d => d.SaveOvertakes(It.IsAny<List<Overtake>>())).ThrowsAsync(new Exception("boom"));
            uowMock.Setup(u => u.OvertakeDao).Returns(overtakeDaoMock.Object);

            var svc = new OvertakeService(clientMock.Object, uowMock.Object, pitServiceMock.Object, raceControlMock.Object);

            // Act
            var result = await svc.SaveOvertakes(new List<Overtake> { new Overtake() });

            // Assert
            Assert.That(result.HasSuccess, Is.False);
            Assert.That(result.Message, Is.Not.Null);
        }
    }
}
