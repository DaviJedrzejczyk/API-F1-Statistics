using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Dao.Interface;
using ExternalApi.Interfaces;
using Services.Interfaces;
using Services.Impl;
using Entities.Class;
using Shared.Responses;
using Entities.Dtos;

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
        public async Task GetOvertakesSessionApi_CompleteWindowSkipsUpstream()
        {
            var client = new Mock<IOvertakeClient>();
            var uow = new Mock<IUnityOfWork>();
            var items = new List<Overtake> { new() { SessionKey = 1 } };
            uow.Setup(x => x.GetImportWindow("overtakes", "1", It.IsAny<CancellationToken>())).ReturnsAsync(new global::Dao.Cache.ImportWindow { IsComplete = true, CompletedAtUtc = DateTimeOffset.UtcNow.AddMinutes(-1), ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(1), Payload = System.Text.Json.JsonSerializer.Serialize(items) });
            var result = await new OvertakeService(client.Object, uow.Object, Mock.Of<IPitService>(), Mock.Of<IRaceControlService>()).GetOvertakesSessionApi(1);
            Assert.That(result.HasSuccess, Is.True);
            Assert.That(result.Itens.Count, Is.EqualTo(1));
            client.Verify(x => x.GetOvertakesSession(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task GetOvertakesSessionApi_PitFailure_DoesNotCommit()
        {
            var client = new Mock<IOvertakeClient>();
            var uow = new Mock<IUnityOfWork>();
            var pits = new Mock<IPitService>();
            uow.Setup(x => x.GetImportWindow(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((global::Dao.Cache.ImportWindow?)null);
            var transaction = new Mock<IImportTransaction>();
            uow.Setup(x => x.BeginImport("overtakes", "3", It.IsAny<CancellationToken>())).ReturnsAsync(transaction.Object);
            client.Setup(x => x.GetOvertakesSession(3, It.IsAny<CancellationToken>())).ReturnsAsync(new DataResponse<Overtake> { HasSuccess = true, Itens = new() { new() { SessionKey = 3 } } });
            pits.Setup(x => x.GetPitsBySessionKeyApi(3, It.IsAny<CancellationToken>())).ReturnsAsync(new DataResponse<Pit> { HasSuccess = false, Message = "pit failure" });
            var result = await new OvertakeService(client.Object, uow.Object, pits.Object, Mock.Of<IRaceControlService>()).GetOvertakesSessionApi(3);
            Assert.That(result.HasSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("pit failure"));
            uow.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task GetOvertakesSessionApi_RaceControlFailure_DoesNotCommit()
        {
            var client = new Mock<IOvertakeClient>();
            var uow = new Mock<IUnityOfWork>();
            var pits = new Mock<IPitService>();
            var controls = new Mock<IRaceControlService>();
            uow.Setup(x => x.GetImportWindow(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((global::Dao.Cache.ImportWindow?)null);
            var transaction = new Mock<IImportTransaction>();
            uow.Setup(x => x.BeginImport("overtakes", "3", It.IsAny<CancellationToken>())).ReturnsAsync(transaction.Object);
            client.Setup(x => x.GetOvertakesSession(3, It.IsAny<CancellationToken>())).ReturnsAsync(new DataResponse<Overtake> { HasSuccess = true, Itens = new() { new() { SessionKey = 3 } } });
            pits.Setup(x => x.GetPitsBySessionKeyApi(3, It.IsAny<CancellationToken>())).ReturnsAsync(new DataResponse<Pit> { HasSuccess = true, Itens = new() });
            controls.Setup(x => x.GetRaceControlsBySessionFlags(3, It.IsAny<string[]>(), It.IsAny<CancellationToken>())).ReturnsAsync(new DataResponse<RaceControlFilterDto> { HasSuccess = false, Message = "rc fail" });
            var result = await new OvertakeService(client.Object, uow.Object, pits.Object, controls.Object).GetOvertakesSessionApi(3);
            Assert.That(result.HasSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo("rc fail"));
            uow.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Never);
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
