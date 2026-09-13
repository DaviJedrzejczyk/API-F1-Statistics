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
    public class CarDataClientTests
    {
        [Test]
        public void Constructor_WithDependencies_DoesNotThrow()
        {
            // Arrange
            var f1Mock = new Mock<IF1ApiClient>();
            var mapperMock = new Mock<IMapper>();

            // Act & Assert
            Assert.DoesNotThrow(() => new CarDataClient(f1Mock.Object, mapperMock.Object));
        }

        [Test]
        public async Task GetHighSpeedsSession_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var f1Mock = new Mock<IF1ApiClient>();
            var mapperMock = new Mock<IMapper>();

            var single = new SingleResponse<string>
            {
                HasSuccess = false,
                Message = "api failure",
                Exception = new InvalidOperationException("api-ex")
            };

            f1Mock.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(single);

            var sut = new CarDataClient(f1Mock.Object, mapperMock.Object);

            // Act
            var result = await sut.GetHighSpeedsSession(1, 100);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("api failure"));
            Assert.IsInstanceOf<InvalidOperationException>(result.Exception);
        }

        [Test]
        public async Task GetHighSpeedsSession_WhenDeserializationFails_ReturnsFailureResponseWithException()
        {
            // Arrange
            var f1Mock = new Mock<IF1ApiClient>();
            var mapperMock = new Mock<IMapper>();

            // Provide JSON that cannot deserialize to List<CarDataDto> (null)
            var single = new SingleResponse<string>
            {
                HasSuccess = true,
                Item = "null"
            };

            f1Mock.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(single);

            var sut = new CarDataClient(f1Mock.Object, mapperMock.Object);

            // Act
            var result = await sut.GetHighSpeedsSession(2, 200);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.IsNotNull(result.Exception);
            Assert.That(result.Exception.Message, Is.EqualTo("Failed to deserialize car data."));
        }

        [Test]
        public async Task GetHighSpeedsSession_WhenApiThrowsException_ReturnsFailureResponse()
        {
            // Arrange
            var f1Mock = new Mock<IF1ApiClient>();
            var mapperMock = new Mock<IMapper>();

            f1Mock.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new Exception("boom"));

            var sut = new CarDataClient(f1Mock.Object, mapperMock.Object);

            // Act
            var result = await sut.GetHighSpeedsSession(3, 300);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("boom"));
            Assert.IsInstanceOf<Exception>(result.Exception);
        }

        [Test]
        public async Task GetHighSpeedsSession_WhenApiReturnsSuccess_MapsAndReturnsData()
        {
            // Arrange
            var f1Mock = new Mock<IF1ApiClient>();
            var mapperMock = new Mock<IMapper>();

            var dtoList = new List<CarDataDto>
            {
                new CarDataDto { SessionKey = 1, DriverNumber = 7, Speed = 220 }
            };

            var json = JsonSerializer.Serialize(dtoList);

            var single = new SingleResponse<string>
            {
                HasSuccess = true,
                Item = json
            };

            f1Mock.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(single);

            var mapped = new List<CarData>
            {
                new CarData { SessionKey = 1, DriverNumber = 7, Speed = 220 }
            };

            mapperMock.Setup(m => m.Map<List<CarData>>(It.IsAny<List<CarDataDto>>())).Returns(mapped);

            var sut = new CarDataClient(f1Mock.Object, mapperMock.Object);

            // Act
            var result = await sut.GetHighSpeedsSession(4, 250);

            // Assert
            Assert.IsTrue(result.HasSuccess);
            Assert.IsNotNull(result.Itens);
            Assert.That(result.Itens.Count, Is.EqualTo(1));
            Assert.That(result.Itens[0].DriverNumber, Is.EqualTo(7));
            Assert.That(result.Itens[0].Speed, Is.EqualTo(220));
        }
    }
}
