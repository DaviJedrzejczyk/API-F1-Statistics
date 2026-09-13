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
    public class MeetingClientTests
    {
        [Test]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            // Arrange
            var f1ApiMock = new Mock<IF1ApiClient>();
            var mapperMock = new Mock<IMapper>();

            // Act
            var client = new MeetingClient(f1ApiMock.Object, mapperMock.Object);

            // Assert
            Assert.IsNotNull(client);
            Assert.IsInstanceOf<IMeetingClient>(client);
        }

        [Test]
        public async Task GetMeetingsByYear_WhenApiReturnsFailure_ReturnsFailureDataResponse()
        {
            // Arrange
            var f1ApiMock = new Mock<IF1ApiClient>();
            var mapperMock = new Mock<IMapper>();

            var expectedEx = new InvalidOperationException("boom");
            var apiResponse = new SingleResponse<string>
            {
                HasSuccess = false,
                Message = "api failed",
                Exception = expectedEx,
                Item = string.Empty
            };

            f1ApiMock.Setup(f => f.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(apiResponse);

            var client = new MeetingClient(f1ApiMock.Object, mapperMock.Object);

            // Act
            var result = await client.GetMeetingsByYear(1999);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("api failed"));
            Assert.That(result.Exception, Is.SameAs(expectedEx));

            f1ApiMock.Verify(f => f.Get("meetings?", "year=1999"), Times.Once);
            mapperMock.Verify(m => m.Map<List<Meeting>>(It.IsAny<object>()), Times.Never);
        }

        [Test]
        public async Task GetMeetingsByYear_WhenApiReturnsInvalidJson_ReturnsFailureDataResponse_WithDeserializeException()
        {
            // Arrange
            var f1ApiMock = new Mock<IF1ApiClient>();
            var mapperMock = new Mock<IMapper>();

            // "null" will deserialize to null and trigger the null-coalescing exception in the method
            var apiResponse = new SingleResponse<string>
            {
                HasSuccess = true,
                Message = "ok",
                Item = "null"
            };

            f1ApiMock.Setup(f => f.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(apiResponse);

            var client = new MeetingClient(f1ApiMock.Object, mapperMock.Object);

            // Act
            var result = await client.GetMeetingsByYear(2001);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("An error occurred while retrieving meetings."));
            Assert.IsNotNull(result.Exception);
            Assert.That(result.Exception.Message, Is.EqualTo("Failed to deserialize tracks."));

            f1ApiMock.Verify(f => f.Get("meetings?", "year=2001"), Times.Once);
            mapperMock.Verify(m => m.Map<List<Meeting>>(It.IsAny<object>()), Times.Never);
        }

        [Test]
        public async Task GetMeetingsByYear_WhenApiReturnsSuccess_ReturnsMappedMeetings()
        {
            // Arrange
            var f1ApiMock = new Mock<IF1ApiClient>();
            var mapperMock = new Mock<IMapper>();

            var dtos = new List<MeetingDto>
            {
                new MeetingDto { MeetingKey = 1, MeetingName = "A", Year = 2020 },
                new MeetingDto { MeetingKey = 2, MeetingName = "B", Year = 2020 }
            };

            var json = JsonSerializer.Serialize(dtos);

            var apiResponse = new SingleResponse<string>
            {
                HasSuccess = true,
                Message = "ok",
                Item = json
            };

            var mapped = new List<Meeting>
            {
                new Meeting { MeetingKey = 1, MeetingName = "A", Year = 2020 },
                new Meeting { MeetingKey = 2, MeetingName = "B", Year = 2020 }
            };

            f1ApiMock.Setup(f => f.Get(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(apiResponse);
            mapperMock.Setup(m => m.Map<List<Meeting>>(It.IsAny<object>())).Returns(mapped);

            var client = new MeetingClient(f1ApiMock.Object, mapperMock.Object);

            // Act
            var result = await client.GetMeetingsByYear(2020);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasSuccess);
            Assert.IsNotNull(result.Itens);
            Assert.That(result.Itens.Count, Is.EqualTo(2));
            Assert.That(result.Itens[0].MeetingKey, Is.EqualTo(1));
            Assert.That(result.Itens[1].MeetingKey, Is.EqualTo(2));

            f1ApiMock.Verify(f => f.Get("meetings?", "year=2020"), Times.Once);
            mapperMock.Verify(m => m.Map<List<Meeting>>(It.IsAny<object>()), Times.Once);
        }
    }
}
