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
    public class OvertakeClientTests
    {
        [Test]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            // Arrange
            var mockClient = new Mock<IF1ApiClient>();
            var mockMapper = new Mock<IMapper>();

            // Act
            var client = new OvertakeClient(mockClient.Object, mockMapper.Object);

            // Assert
            Assert.IsNotNull(client);
            Assert.IsInstanceOf<IOvertakeClient>(client);
        }

        [Test]
        public async Task GetOvertakesSession_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var mockClient = new Mock<IF1ApiClient>();
            var mockMapper = new Mock<IMapper>();

            var apiResponse = new SingleResponse<string>
            {
                HasSuccess = false,
                Message = "api error",
                Exception = new InvalidOperationException("bad")
            };

            mockClient.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(apiResponse);

            var client = new OvertakeClient(mockClient.Object, mockMapper.Object);

            // Act
            var result = await client.GetOvertakesSession(123);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("api error"));
            Assert.IsNotNull(result.Exception);
            Assert.IsInstanceOf<InvalidOperationException>(result.Exception);
        }

        [Test]
        public async Task GetOvertakesSession_WhenApiReturnsEmptyList_ReturnsNotFoundFailure()
        {
            // Arrange
            var mockClient = new Mock<IF1ApiClient>();
            var mockMapper = new Mock<IMapper>();

            var apiResponse = new SingleResponse<string>
            {
                HasSuccess = true,
                Message = "ok",
                Item = "[]"
            };

            mockClient.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(apiResponse);

            var client = new OvertakeClient(mockClient.Object, mockMapper.Object);

            // Act
            var result = await client.GetOvertakesSession(1);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("Overtakes in this session not found!"));
        }

        [Test]
        public async Task GetOvertakesSession_WhenApiReturnsValidJson_ReturnsMappedSuccess()
        {
            // Arrange
            var mockClient = new Mock<IF1ApiClient>();
            var mockMapper = new Mock<IMapper>();

            var dto = new OvertakeDto
            {
                MeetingKey = 10,
                SessionKey = 20,
                OvertakingDriverNumber = 1,
                OvertakedDriverNumber = 2,
                Date = new DateTime(2020,1,1),
                Position = 5
            };

            var json = JsonSerializer.Serialize(new List<OvertakeDto> { dto });

            var apiResponse = new SingleResponse<string>
            {
                HasSuccess = true,
                Message = "ok",
                Item = json
            };

            var mapped = new List<Overtake>
            {
                new Overtake
                {
                    MeetingKey = dto.MeetingKey,
                    SessionKey = dto.SessionKey,
                    OvertakingDriverNumber = dto.OvertakingDriverNumber,
                    OvertakedDriverNumber = dto.OvertakedDriverNumber,
                    Date = dto.Date,
                    Position = dto.Position
                }
            };

            mockClient.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(apiResponse);
            mockMapper.Setup(m => m.Map<List<Overtake>>(It.IsAny<List<OvertakeDto>>())).Returns(mapped);

            var client = new OvertakeClient(mockClient.Object, mockMapper.Object);

            // Act
            var result = await client.GetOvertakesSession(2);

            // Assert
            Assert.IsTrue(result.HasSuccess);
            Assert.IsNotNull(result.Itens);
            Assert.That(result.Itens.Count, Is.EqualTo(1));
            Assert.That(result.Itens[0].MeetingKey, Is.EqualTo(dto.MeetingKey));
            Assert.That(result.Itens[0].Position, Is.EqualTo(dto.Position));
        }

        [Test]
        public async Task GetOvertakesSession_WhenClientThrowsException_ReturnsFailureWithException()
        {
            // Arrange
            var mockClient = new Mock<IF1ApiClient>();
            var mockMapper = new Mock<IMapper>();

            var boom = new Exception("boom");
            mockClient.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(boom);

            var client = new OvertakeClient(mockClient.Object, mockMapper.Object);

            // Act
            var result = await client.GetOvertakesSession(5);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.IsNotNull(result.Exception);
            Assert.That(result.Message, Is.EqualTo("boom"));
        }
    }
}
