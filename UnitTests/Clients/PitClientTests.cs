using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Entities.Class;
using Entities.Dtos;
using ExternalApi.Impls;
using ExternalApi.Interfaces;
using Moq;
using NUnit.Framework;
using Shared.Responses;

namespace UnitTests.Impls
{
    [TestFixture]
    public class PitClientTests
    {
        [Test]
        public void Constructor_ValidDependencies_DoesNotThrow()
        {
            // Arrange
            var clientMock = new Mock<IF1ApiClient>();
            var mapperMock = new Mock<IMapper>();

            // Act & Assert - constructor should not throw
            Assert.DoesNotThrow(() => new PitClient(clientMock.Object, mapperMock.Object));
        }

        [Test]
        public async Task GetAllPitsSession_ClientReturnsFailure_ReturnsFailureMessage()
        {
            // Arrange
            var clientMock = new Mock<IF1ApiClient>();
            var mapperMock = new Mock<IMapper>();

            var singleResponse = new SingleResponse<string> { HasSuccess = false, Message = "fail" };
            clientMock.Setup(c => c.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(singleResponse);

            var sut = new PitClient(clientMock.Object, mapperMock.Object);

            // Act
            DataResponse<Pit> result = await sut.GetAllPitsSession(1);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("Error to search the pits with this session key."));
            clientMock.Verify(c => c.Get("pit?", "session_key=1", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetAllPitsSession_ItemNull_ReturnsSessionContainsNoPitsMessage()
        {
            // Arrange
            var clientMock = new Mock<IF1ApiClient>();
            var mapperMock = new Mock<IMapper>();

            var singleResponse = new SingleResponse<string> { HasSuccess = true, Item = null! };
            clientMock.Setup(c => c.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(singleResponse);

            var sut = new PitClient(clientMock.Object, mapperMock.Object);

            // Act
            DataResponse<Pit> result = await sut.GetAllPitsSession(2);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("The session dosen't contains pits."));
            clientMock.Verify(c => c.Get("pit?", "session_key=2", It.IsAny<CancellationToken>()), Times.Once);
            mapperMock.Verify(m => m.Map<List<Pit>>(It.IsAny<List<PitDto>>()), Times.Never);
        }

        [Test]
        public async Task GetAllPitsSession_DeserializeReturnsNull_ReturnsCannotDeserializeMessage()
        {
            // Arrange
            var clientMock = new Mock<IF1ApiClient>();
            var mapperMock = new Mock<IMapper>();

            // JSON "null" will deserialize to null for a List<T>
            var singleResponse = new SingleResponse<string> { HasSuccess = true, Item = "null" };
            clientMock.Setup(c => c.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(singleResponse);

            var sut = new PitClient(clientMock.Object, mapperMock.Object);

            // Act
            DataResponse<Pit> result = await sut.GetAllPitsSession(3);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("Cannot deserealize the JSON of pits."));
            mapperMock.Verify(m => m.Map<List<Pit>>(It.IsAny<List<PitDto>>()), Times.Never);
        }

        [Test]
        public async Task GetAllPitsSession_ValidJson_ReturnsMappedPits()
        {
            // Arrange
            var clientMock = new Mock<IF1ApiClient>();
            var mapperMock = new Mock<IMapper>();

            var dtoList = new List<PitDto>
            {
                new PitDto { Date = DateTime.UtcNow, DriverNumber = 7, LaneDuration = 1.2, LapNumber = 5, MeetingKey = 10, PitDuration = 2.5, SessionKey = 3, StopDuration = 0.5 }
            };

            string json = JsonSerializer.Serialize(dtoList);

            var singleResponse = new SingleResponse<string> { HasSuccess = true, Item = json };
            clientMock.Setup(c => c.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(singleResponse);

            var mappedList = new List<Pit> { new Pit { SessionKey = 3, DriverNumber = 7 } };
            mapperMock.Setup(m => m.Map<List<Pit>>(It.IsAny<List<PitDto>>())).Returns(mappedList);

            var sut = new PitClient(clientMock.Object, mapperMock.Object);

            // Act
            DataResponse<Pit> result = await sut.GetAllPitsSession(4);

            // Assert
            Assert.IsTrue(result.HasSuccess);
            Assert.IsNotNull(result.Itens);
            Assert.That(result.Itens.Count, Is.EqualTo(1));
            Assert.That(result.Itens[0].SessionKey, Is.EqualTo(3));
            mapperMock.Verify(m => m.Map<List<Pit>>(It.IsAny<List<PitDto>>()), Times.Once);
            clientMock.Verify(c => c.Get("pit?", "session_key=4", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetAllPitsSession_GetThrowsException_ReturnsFailureWithException()
        {
            // Arrange
            var clientMock = new Mock<IF1ApiClient>();
            var mapperMock = new Mock<IMapper>();

            clientMock.Setup(c => c.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("boom"));

            var sut = new PitClient(clientMock.Object, mapperMock.Object);

            // Act
            DataResponse<Pit> result = await sut.GetAllPitsSession(5);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.IsNotNull(result.Exception);
            Assert.That(result.Exception.Message, Is.EqualTo("boom"));
            clientMock.Verify(c => c.Get("pit?", "session_key=5", It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
