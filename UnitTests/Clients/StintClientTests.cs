using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Entities.Class;
using Entities.Dtos;
using ExternalApi.Interfaces;
using ExternalApi.Impls;
using Moq;
using NUnit.Framework;
using Shared.Responses;

namespace UnitTests.Impls
{
    [TestFixture]
    public class StintClientTests
    {
        [Test]
        public void Constructor_WithValidDependencies_DoesNotThrow()
        {
            // Arrange
            var apiMock = new Mock<IF1ApiClient>();
            var mapperMock = new Mock<IMapper>();

            // Act / Assert
            Assert.DoesNotThrow(() => new StintClient(apiMock.Object, mapperMock.Object));
        }

        [Test]
        public async Task GetAllStintsBySession_WithValidResponse_ReturnsSuccessDataResponseAndMapsItems()
        {
            // Arrange
            var apiMock = new Mock<IF1ApiClient>();
            var mapperMock = new Mock<IMapper>();

            var dto = new StintListDTO { Compound = "soft", DriverNumber = 7, SessionKey = 5 };
            var dtoList = new List<StintListDTO> { dto };
            var json = JsonSerializer.Serialize(dtoList);

            apiMock.Setup(a => a.Get("stints?", "session_key=5"))
                .ReturnsAsync(new SingleResponse<string> { Item = json });

            var mapped = new List<Stint> { new Stint { Compound = dto.Compound, DriverNumber = dto.DriverNumber, SessionKey = dto.SessionKey } };
            mapperMock.Setup(m => m.Map<List<Stint>>(It.IsAny<List<StintListDTO>>())).Returns(mapped);

            var client = new StintClient(apiMock.Object, mapperMock.Object);

            // Act
            DataResponse<Stint> result = await client.GetAllStintsBySession(5);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasSuccess);
            Assert.IsNotNull(result.Itens);
            Assert.That(result.Itens.Count, Is.EqualTo(1));
            Assert.That(result.Itens[0].DriverNumber, Is.EqualTo(7));

            apiMock.Verify(a => a.Get("stints?", "session_key=5"), Times.Once);
            mapperMock.Verify(m => m.Map<List<Stint>>(It.IsAny<List<StintListDTO>>()), Times.Once);
        }

        [Test]
        public async Task GetAllStintsBySession_WhenDeserializedIsNull_ReturnsFailureDataResponse()
        {
            // Arrange
            var apiMock = new Mock<IF1ApiClient>();
            var mapperMock = new Mock<IMapper>();

            apiMock.Setup(a => a.Get(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SingleResponse<string> { Item = "null" });

            var client = new StintClient(apiMock.Object, mapperMock.Object);

            // Act
            DataResponse<Stint> result = await client.GetAllStintsBySession(123);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("No stints found for the given session key."));

            mapperMock.Verify(m => m.Map<List<Stint>>(It.IsAny<List<StintListDTO>>()), Times.Never);
        }

        [Test]
        public async Task GetAllStintsBySession_WhenApiThrows_ReturnsFailureDataResponseWithException()
        {
            // Arrange
            var apiMock = new Mock<IF1ApiClient>();
            var mapperMock = new Mock<IMapper>();

            apiMock.Setup(a => a.Get(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            var client = new StintClient(apiMock.Object, mapperMock.Object);

            // Act
            DataResponse<Stint> result = await client.GetAllStintsBySession(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasSuccess);
            Assert.IsNotNull(result.Exception);
            Assert.That(result.Exception, Is.TypeOf<InvalidOperationException>());
            Assert.That(result.Message, Is.EqualTo("boom"));
        }
    }
}
