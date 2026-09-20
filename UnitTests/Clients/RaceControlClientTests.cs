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

namespace UnitTests
{
    [TestFixture]
    public class RaceControlClientTests
    {
        [Test]
        public void Constructor_WithValidDependencies_DoesNotThrow()
        {
            // Arrange
            var mockClient = new Mock<IF1ApiClient>();
            var mockMapper = new Mock<IMapper>();

            // Act & Assert
            Assert.DoesNotThrow(() => new RaceControlClient(mockClient.Object, mockMapper.Object));
        }

        [Test]
        public async Task GetRaceControlsBySessionFlags_ResponseNotSuccess_ReturnsFailureDataResponse()
        {
            // Arrange
            var mockClient = new Mock<IF1ApiClient>();
            var mockMapper = new Mock<IMapper>();

            var response = new SingleResponse<string>
            {
                HasSuccess = false,
                Message = "error",
                Exception = new InvalidOperationException("client error"),
                Item = null!
            };

            mockClient
                .Setup(c => c.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var sut = new RaceControlClient(mockClient.Object, mockMapper.Object);

            // Act
            var result = await sut.GetRaceControlsBySessionFlags(123, new[] { "red", "yellow" });

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("error"));
            Assert.IsNotNull(result.Exception);
            Assert.That(result.Exception.Message, Is.EqualTo("client error"));

            // verify that client was invoked with expected constructed query
            mockClient.Verify(c => c.Get("race_control?", "session_key=123&flag=red&flag=yellow", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetRaceControlsBySessionFlags_DeserializeReturnsNull_ReturnsFailureDataResponseWithMessage()
        {
            // Arrange
            var mockClient = new Mock<IF1ApiClient>();
            var mockMapper = new Mock<IMapper>();

            var response = new SingleResponse<string>
            {
                HasSuccess = true,
                Message = "ok",
                Exception = null!,
                Item = "null"
            };

            mockClient
                .Setup(c => c.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var sut = new RaceControlClient(mockClient.Object, mockMapper.Object);

            // Act
            var result = await sut.GetRaceControlsBySessionFlags(5, new[] { "blue" });

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("Failed to deserialize race controls"));

            mockClient.Verify(c => c.Get("race_control?", "session_key=5&flag=blue", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetRaceControlsBySessionFlags_SuccessResponse_MapsAndReturnsSuccessDataResponse()
        {
            // Arrange
            var mockClient = new Mock<IF1ApiClient>();
            var mockMapper = new Mock<IMapper>();

            var dto = new RaceControlDto
            {
                Category = "cat",
                Date = new DateTime(2020, 1, 1),
                DriverNumber = 77,
                Flag = "red",
                LapNumber = 10,
                MeetingKey = 2,
                Message = "msg",
                QualifyingPhase = null,
                Scope = "scope",
                Sector = null,
                SessionKey = 42
            };

            var dtos = new List<RaceControlDto> { dto };
            var json = JsonSerializer.Serialize(dtos);

            var response = new SingleResponse<string>
            {
                HasSuccess = true,
                Message = "ok",
                Exception = null!,
                Item = json
            };

            mockClient
                .Setup(c => c.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var mapped = new List<RaceControl>
            {
                new RaceControl
                {
                    Category = dto.Category,
                    Date = dto.Date,
                    DriverNumber = dto.DriverNumber,
                    Flag = dto.Flag,
                    LapNumber = dto.LapNumber,
                    MeetingKey = dto.MeetingKey,
                    Message = dto.Message,
                    QualifyingPhase = dto.QualifyingPhase,
                    Scope = dto.Scope,
                    Sector = dto.Sector,
                    SessionKey = dto.SessionKey
                }
            };

            mockMapper
                .Setup(m => m.Map<List<RaceControl>>(It.IsAny<List<RaceControlDto>>()))
                .Returns(mapped);

            var sut = new RaceControlClient(mockClient.Object, mockMapper.Object);

            // Act
            var result = await sut.GetRaceControlsBySessionFlags(7, new[] { "green" });

            // Assert
            Assert.IsTrue(result.HasSuccess);
            Assert.IsNotNull(result.Itens);
            Assert.That(result.Itens.Count, Is.EqualTo(1));
            Assert.That(result.Itens[0].Category, Is.EqualTo("cat"));

            mockClient.Verify(c => c.Get("race_control?", "session_key=7&flag=green", It.IsAny<CancellationToken>()), Times.Once);
            mockMapper.Verify(m => m.Map<List<RaceControl>>(It.IsAny<List<RaceControlDto>>()), Times.Once);
        }
    }
}
