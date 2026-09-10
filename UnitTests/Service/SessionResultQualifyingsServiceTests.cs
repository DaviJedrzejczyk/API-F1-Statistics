using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dao.Interface;
using Entities.Class;
using Entities.Dtos.SessionResultDTOs;
using Moq;
using NUnit.Framework;
using Services.Impl;
using Shared.Responses;

namespace UnitTests.Service
{
    [TestFixture]
    public class SessionResultQualifyingsServiceTests
    {
        private Mock<IUnityOfWork> _unityMock = null!;
        private Mock<ISessionResultQualifyDao> _qualifyDaoMock = null!;

        [SetUp]
        public void SetUp()
        {
            _unityMock = new Mock<IUnityOfWork>();
            _qualifyDaoMock = new Mock<ISessionResultQualifyDao>();

            _unityMock.Setup(u => u.SessionResultQualifyDao).Returns(_qualifyDaoMock.Object);
        }

        [Test]
        public void Constructor_WithValidDependency_DoesNotThrow_And_UsesDependency()
        {
            // Arrange / Act
            var service = new SessionResultQualifyingsService(_unityMock.Object);

            // Assert - verify we can call a method which uses the unityOfWork
            _qualifyDaoMock.Setup(d => d.GetQualifyingResultBySessionKey(It.IsAny<int>())).ReturnsAsync(new DataResponse<SessionResultQualify> { HasSuccess = true, Itens = new List<SessionResultQualify>() });

            Assert.DoesNotThrowAsync(async () => await service.GetQualyBySessionKey(1));
            _qualifyDaoMock.Verify(d => d.GetQualifyingResultBySessionKey(1), Times.Once);
        }

        [Test]
        public async Task CreateListResultQualyfing_WithMultipleDurations_CreatesExpectedPhases_AndReturnsSuccess()
        {
            // Arrange
            var items = new List<SessionResultDto>
            {
                new() { SessionKey = 1, MeetingKey = 10, DriverNumber = 44, Duration = new List<double> { 0.0, 0.0, 0.2 }, GapToLeader = "0;0;0" }, // Q3
                new() { SessionKey = 2, MeetingKey = 11, DriverNumber = 7, Duration = new List<double> { 0.0, 0.3, 0.0 }, GapToLeader = "0;0;0" },  // Q2
                new() { SessionKey = 3, MeetingKey = 12, DriverNumber = 5, Duration = new List<double> { 0.5, 0.0, 0.0 }, GapToLeader = "0;0;0" }   // Q1
            };

            // SaveQualy flow: SaveQualifyResult succeeds and Commit succeeds
            _qualifyDaoMock.Setup(d => d.SaveQualifyResult(It.IsAny<List<SessionResultQualify>>())).ReturnsAsync(new Response { HasSuccess = true });
            var commitResp = new Response { HasSuccess = true, Message = "committed" };
            _unityMock.Setup(u => u.Commit()).ReturnsAsync(commitResp);

            var service = new SessionResultQualifyingsService(_unityMock.Object);

            // Act
            var result = await service.CreateListResultQualyfing(items);

            // Assert
            Assert.IsTrue(result.HasSuccess);
            Assert.IsNotNull(result.Itens);
            Assert.That(result.Itens.Count, Is.EqualTo(3));

            var r1 = result.Itens.First(i => i.SessionKey == 1);
            Assert.That(r1.QualifyingPhase, Is.EqualTo("Q3"));
            Assert.That(r1.Duration, Is.EqualTo(0.2));

            var r2 = result.Itens.First(i => i.SessionKey == 2);
            Assert.That(r2.QualifyingPhase, Is.EqualTo("Q2"));
            Assert.That(r2.Duration, Is.EqualTo(0.3));

            var r3 = result.Itens.First(i => i.SessionKey == 3);
            Assert.That(r3.QualifyingPhase, Is.EqualTo("Q1"));
            Assert.That(r3.Duration, Is.EqualTo(0.5));

            _qualifyDaoMock.Verify(d => d.SaveQualifyResult(It.Is<List<SessionResultQualify>>(l => l.Count == 3)), Times.Once);
            _unityMock.Verify(u => u.Commit(), Times.Once);
        }

        [Test]
        public async Task CreateListResultQualyfing_WhenSaveFails_ReturnsFailureWithMessageAndException()
        {
            // Arrange
            var items = new List<SessionResultDto>
            {
                new() { SessionKey = 1, MeetingKey = 10, DriverNumber = 44, Duration = new List<double> { 0.1, 0.0, 0.0 }, GapToLeader = "0;0;0" }
            };

            var ex = new InvalidOperationException("save error");
            var daoResponse = new Response { HasSuccess = false, Message = "save failed", Exception = ex };
            _qualifyDaoMock.Setup(d => d.SaveQualifyResult(It.IsAny<List<SessionResultQualify>>())).ReturnsAsync(daoResponse);

            var service = new SessionResultQualifyingsService(_unityMock.Object);

            // Act
            var result = await service.CreateListResultQualyfing(items);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("save failed"));
            Assert.That(result.Exception, Is.SameAs(ex));
        }

        [Test]
        public async Task CreateListResultQualyfing_WhenDurationNull_ThrowsInsideMapping_ReturnsFailureResponse()
        {
            // Arrange - this will cause x.Duration![phase] to throw
            var items = new List<SessionResultDto>
            {
                new() { SessionKey = 1, MeetingKey = 10, DriverNumber = 44, Duration = null! }
            };

            var service = new SessionResultQualifyingsService(_unityMock.Object);

            // Act
            var result = await service.CreateListResultQualyfing(items);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.IsNotNull(result.Exception);
        }

        [Test]
        public async Task GetQualyBySessionKey_WhenDaoReturnsSuccess_ReturnsSuccessWithItems()
        {
            // Arrange
            int key = 123;
            var items = new List<SessionResultQualify> { new() { SessionKey = key } };
            var daoResponse = new DataResponse<SessionResultQualify> { HasSuccess = true, Itens = items };
            _qualifyDaoMock.Setup(d => d.GetQualifyingResultBySessionKey(key)).ReturnsAsync(daoResponse);

            var service = new SessionResultQualifyingsService(_unityMock.Object);

            // Act
            var result = await service.GetQualyBySessionKey(key);

            // Assert
            Assert.IsTrue(result.HasSuccess);
            Assert.That(result.Itens, Is.SameAs(items));
        }

        [Test]
        public async Task GetQualyBySessionKey_WhenDaoReturnsFailureWithException_ReturnsFailureWithSameException()
        {
            // Arrange
            int key = 5;
            var ex = new Exception("db error");
            var daoResponse = new DataResponse<SessionResultQualify> { HasSuccess = false, Message = "db fail", Exception = ex };
            _qualifyDaoMock.Setup(d => d.GetQualifyingResultBySessionKey(key)).ReturnsAsync(daoResponse);

            var service = new SessionResultQualifyingsService(_unityMock.Object);

            // Act
            var result = await service.GetQualyBySessionKey(key);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Exception, Is.SameAs(ex));
            Assert.That(result.Message, Is.EqualTo("db fail"));
        }

        [Test]
        public async Task GetQualyBySessionKey_WhenDaoReturnsNoItems_ReturnsFailureWithNotFoundMessage()
        {
            // Arrange
            int key = 6;
            var daoResponse = new DataResponse<SessionResultQualify> { HasSuccess = false, Itens = new List<SessionResultQualify>() };
            _qualifyDaoMock.Setup(d => d.GetQualifyingResultBySessionKey(key)).ReturnsAsync(daoResponse);

            var service = new SessionResultQualifyingsService(_unityMock.Object);

            // Act
            var result = await service.GetQualyBySessionKey(key);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("No qualifying results found for the specified session key."));
        }

        [Test]
        public async Task SaveQualys_WhenSaveFails_ReturnsDaoResponseWithoutCallingCommit()
        {
            // Arrange
            var list = new List<SessionResultQualify> { new() };
            var daoResponse = new Response { HasSuccess = false, Message = "save failed" };
            _qualifyDaoMock.Setup(d => d.SaveQualifyResult(list)).ReturnsAsync(daoResponse);

            var service = new SessionResultQualifyingsService(_unityMock.Object);

            // Act
            var result = await service.SaveQualys(list);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("save failed"));
            _unityMock.Verify(u => u.Commit(), Times.Never);
        }

        [Test]
        public async Task SaveQualys_WhenSaveSucceeds_CallsCommitAndReturnsCommitResponse()
        {
            // Arrange
            var list = new List<SessionResultQualify> { new() };
            _qualifyDaoMock.Setup(d => d.SaveQualifyResult(list)).ReturnsAsync(new Response { HasSuccess = true });
            var commitResp = new Response { HasSuccess = true, Message = "committed" };
            _unityMock.Setup(u => u.Commit()).ReturnsAsync(commitResp);

            var service = new SessionResultQualifyingsService(_unityMock.Object);

            // Act
            var result = await service.SaveQualys(list);

            // Assert
            Assert.IsTrue(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("committed"));
            _unityMock.Verify(u => u.Commit(), Times.Once);
        }

        [Test]
        public async Task SaveQualys_WhenDaoThrows_ReturnsFailureResponseWithException()
        {
            // Arrange
            var list = new List<SessionResultQualify> { new() };
            var ex = new InvalidOperationException("boom");
            _qualifyDaoMock.Setup(d => d.SaveQualifyResult(It.IsAny<List<SessionResultQualify>>())).ThrowsAsync(ex);

            var service = new SessionResultQualifyingsService(_unityMock.Object);

            // Act
            var result = await service.SaveQualys(list);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.IsNotNull(result.Exception);
            Assert.IsInstanceOf<InvalidOperationException>(result.Exception);
        }
    }
}
