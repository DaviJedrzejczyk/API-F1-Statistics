using System;
using System.Collections.Generic;
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
    public class DriverClientTests
    {
        [Test]
        public async Task GetAllDriversSessionSelected_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var mockApi = new Mock<IF1ApiClient>();
            var mockMapper = new Mock<IMapper>();

            var apiException = new Exception("api-exception");
            var failure = ResponseFactory.CreateInstance().CreateFailureSingleResponse<string>("api failed", apiException);
            mockApi.Setup(a => a.Get("drivers?", It.IsAny<string>())).ReturnsAsync(failure);

            var client = new DriverClient(mockApi.Object, mockMapper.Object);

            // Act
            var result = await client.GetAllDriversSessionSelected(new DriverInsertDTO { SessionKey = 2 });

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("api failed"));
            Assert.IsNotNull(result.Exception);
            Assert.That(result.Exception.Message, Is.EqualTo("api-exception"));
            mockApi.Verify(a => a.Get("drivers?", "session_key=2"), Times.Once);
            mockMapper.Verify(m => m.Map<List<Driver>>(It.IsAny<List<DriverDto>>()), Times.Never);
        }

        [Test]
        public async Task GetAllDriversSessionSelected_WhenDeserializationFails_ReturnsFailureResponse()
        {
            // Arrange
            var mockApi = new Mock<IF1ApiClient>();
            var mockMapper = new Mock<IMapper>();

            // JSON that will deserialize to null for a List<DriverDto>
            var jsonNull = "null";
            var successWithNull = new Shared.Responses.SingleResponse<string> { HasSuccess = true, Message = "OK", Item = jsonNull };
            mockApi.Setup(a => a.Get("drivers?", It.IsAny<string>())).ReturnsAsync(successWithNull);

            var client = new DriverClient(mockApi.Object, mockMapper.Object);

            // Act
            var result = await client.GetAllDriversSessionSelected(new DriverInsertDTO { SessionKey = 3 });

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasSuccess);
            Assert.IsTrue(result.Message.Contains("Failed to deserialize"));
            mockApi.Verify(a => a.Get("drivers?", "session_key=3"), Times.Once);
            mockMapper.Verify(m => m.Map<List<Driver>>(It.IsAny<List<DriverDto>>()), Times.Never);
        }

        [Test]
        public async Task GetAllDriversSessionSelected_WhenApiReturnsDrivers_ReturnsMappedDrivers()
        {
            // Arrange
            var mockApi = new Mock<IF1ApiClient>();
            var mockMapper = new Mock<IMapper>();

            var driverDtoJson = "[ { \"driver_number\": 42, \"broadcast_name\": \"BN\", \"first_name\": \"F\", \"full_name\": \"FF\", \"headshot_url\": \"H\", \"last_name\": \"L\", \"meeting_key\": 1, \"name_acronym\": \"NA\", \"session_key\": 4, \"team_colour\": \"C\", \"team_name\": \"T\" } ]";
            var apiResponse = new SingleResponse<string>{ HasSuccess = true, Item = driverDtoJson };

            mockApi.Setup(a => a.Get("drivers?", It.IsAny<string>())).ReturnsAsync(apiResponse);

            var expectedDrivers = new List<Driver> { new Driver { DriverNumber = 42, FirstName = "F" } };

            // Setup mapper using It.IsAny<object>() and inspect the runtime argument in Returns callback
            mockMapper
                .Setup(m => m.Map<List<Driver>>(It.IsAny<object>()))
                .Returns((object src) =>
                {
                    var list = src as List<DriverDto>;
                    if (list != null && list.Count == 1 && list[0].DriverNumber == 42)
                        return expectedDrivers;
                    return new List<Driver>();
                });

            var client = new DriverClient(mockApi.Object, mockMapper.Object);

            // Act
            var result = await client.GetAllDriversSessionSelected(new DriverInsertDTO { SessionKey = 4 });

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasSuccess);
            Assert.IsNotNull(result.Itens);
            Assert.That(result.Itens.Count, Is.EqualTo(1));
            Assert.That(result.Itens[0].DriverNumber, Is.EqualTo(42));
            mockApi.Verify(a => a.Get("drivers?", "session_key=4"), Times.Once);

            // Verify mapper was called once (argument inspected in Returns)
            mockMapper.Verify(m => m.Map<List<Driver>>(It.IsAny<object>()), Times.Once);
        }
    }
}
