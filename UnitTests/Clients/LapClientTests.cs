using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Entities.Dtos;
using ExternalApi.Impls;
using ExternalApi.Interfaces;
using Moq;
using NUnit.Framework;
using Shared.Responses;

namespace UnitTests.Impls
{
    [TestFixture]
    public class LapClientTests
    {
        [Test]
        public void Constructor_WithDependency_CreatesInstance()
        {
            // Arrange
            var mockF1Client = new Mock<IF1ApiClient>();

            // Act
            var sut = new LapClient(mockF1Client.Object);

            // Assert
            Assert.IsNotNull(sut);
            Assert.That(sut, Is.InstanceOf<ILapClient>());
        }

        [Test]
        public async Task GetAllLapsSessionByDriver_ApiReturnsValidJson_ReturnsSuccessDataResponse()
        {
            // Arrange
            var mockF1Client = new Mock<IF1ApiClient>();
            var expectedList = new List<LapListDto>
            {
                new LapListDto { DriverNumber = 7, LapNumber = 1, DateStart = DateTime.UtcNow }
            };
            string json = JsonSerializer.Serialize(expectedList);
            var singleResponse = new SingleResponse<string> { HasSuccess = true, Message = "ok", Item = json };
            mockF1Client.Setup(m => m.Get("laps?", It.IsAny<string>())).ReturnsAsync(singleResponse);

            var sut = new LapClient(mockF1Client.Object);

            // Act
            DataResponse<LapListDto> result = await sut.GetAllLapsSessionByDriver(123, 7);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasSuccess);
            Assert.IsNotNull(result.Itens);
            Assert.That(result.Itens.Count, Is.EqualTo(1));
            Assert.That(result.Itens[0].DriverNumber, Is.EqualTo(7));
            mockF1Client.Verify(m => m.Get("laps?", "session_key=123&driver_number=7"), Times.Once);
        }

        [Test]
        public async Task GetAllLapsSessionByDriver_ApiReturnsFailure_ReturnsFailureDataResponse()
        {
            // Arrange
            var mockF1Client = new Mock<IF1ApiClient>();
            var ex = new Exception("api error");
            var singleResponse = new SingleResponse<string> { HasSuccess = false, Message = "err", Exception = ex };
            mockF1Client.Setup(m => m.Get("laps?", It.IsAny<string>())).ReturnsAsync(singleResponse);

            var sut = new LapClient(mockF1Client.Object);

            // Act
            DataResponse<LapListDto> result = await sut.GetAllLapsSessionByDriver(1, 2);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("err"));
            Assert.That(result.Exception, Is.EqualTo(ex));
            mockF1Client.Verify(m => m.Get("laps?", "session_key=1&driver_number=2"), Times.Once);
        }

        [Test]
        public async Task GetAllLapsSessionByDriver_ApiReturnsNullJson_ReturnsFailureDueToDeserialization()
        {
            // Arrange
            var mockF1Client = new Mock<IF1ApiClient>();
            var singleResponse = new SingleResponse<string> { HasSuccess = true, Message = "ok", Item = "null" };
            mockF1Client.Setup(m => m.Get("laps?", It.IsAny<string>())).ReturnsAsync(singleResponse);

            var sut = new LapClient(mockF1Client.Object);

            // Act
            DataResponse<LapListDto> result = await sut.GetAllLapsSessionByDriver(9, 9);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("Failed to deserialize lap list."));
            mockF1Client.Verify(m => m.Get("laps?", "session_key=9&driver_number=9"), Times.Once);
        }

        [Test]
        public async Task GetAllLapsSessionByDriver_ApiThrowsException_ReturnsFailureWithException()
        {
            // Arrange
            var mockF1Client = new Mock<IF1ApiClient>();
            var thrown = new InvalidOperationException("boom");
            mockF1Client.Setup(m => m.Get("laps?", It.IsAny<string>())).ThrowsAsync(thrown);

            var sut = new LapClient(mockF1Client.Object);

            // Act
            DataResponse<LapListDto> result = await sut.GetAllLapsSessionByDriver(5, 6);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Exception, Is.TypeOf<InvalidOperationException>());
            Assert.That(result.Message, Is.EqualTo("boom"));
            mockF1Client.Verify(m => m.Get("laps?", "session_key=5&driver_number=6"), Times.Once);
        }    }
}
