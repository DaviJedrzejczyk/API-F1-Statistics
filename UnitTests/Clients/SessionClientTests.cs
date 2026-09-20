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
    public class SessionClientTests
    {
        [Test]
        public void Constructor_WithValidDependencies_DoesNotThrow()
        {
            // Arrange
            var apiMock = new Mock<IF1ApiClient>();
            var mapperMock = new Mock<IMapper>();

            // Act / Assert
            Assert.DoesNotThrow(() => new SessionClient(apiMock.Object, mapperMock.Object));
            var instance = new SessionClient(apiMock.Object, mapperMock.Object);
            Assert.IsInstanceOf<ISessionClient>(instance);
        }

        [Test]
        public async Task GetSessionsByMeetingKey_ApiReturnsFailure_ReturnsFailureDataResponse()
        {
            // Arrange
            var apiMock = new Mock<IF1ApiClient>();
            var mapperMock = new Mock<IMapper>();
            int meetingKey = 42;

            var apiResponse = new SingleResponse<string> { HasSuccess = false, Message = "api error", Exception = new InvalidOperationException("upstream") };
            apiMock.Setup(a => a.Get("sessions?", $"meeting_key={meetingKey}", It.IsAny<CancellationToken>())).ReturnsAsync(apiResponse);

            var client = new SessionClient(apiMock.Object, mapperMock.Object);

            // Act
            var result = await client.GetSessionsByMeetingKey(meetingKey);

            // Assert
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("api error"));
            Assert.IsNotNull(result.Exception);
            Assert.IsInstanceOf<InvalidOperationException>(result.Exception);
            apiMock.Verify(a => a.Get("sessions?", $"meeting_key={meetingKey}", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetSessionsByMeetingKey_ApiReturnsSuccess_DeserializesAndMaps_ReturnsSuccessDataResponse()
        {
            // Arrange
            var apiMock = new Mock<IF1ApiClient>();
            var mapperMock = new Mock<IMapper>();
            int meetingKey = 7;

            var dtoList = new List<SessionDto>
            {
                new SessionDto { SessionKey = 1, MeetingKey = meetingKey, CountryName = "X" }
            };
            string json = JsonSerializer.Serialize(dtoList);

            var apiResponse = new SingleResponse<string> { HasSuccess = true, Item = json };
            apiMock.Setup(a => a.Get("sessions?", $"meeting_key={meetingKey}", It.IsAny<CancellationToken>())).ReturnsAsync(apiResponse);

            var mapped = new List<Session>
            {
                new Session { SessionKey = 1, MeetingKey = meetingKey, CountryName = "X" }
            };
            mapperMock.Setup(m => m.Map<List<Session>>(It.IsAny<List<SessionDto>>())).Returns(mapped);

            var client = new SessionClient(apiMock.Object, mapperMock.Object);

            // Act
            var result = await client.GetSessionsByMeetingKey(meetingKey);

            // Assert
            Assert.IsTrue(result.HasSuccess);
            Assert.IsNotNull(result.Itens);
            Assert.That(result.Itens.Count, Is.EqualTo(mapped.Count));
            Assert.That(result.Itens[0].SessionKey, Is.EqualTo(mapped[0].SessionKey));
            mapperMock.Verify(m => m.Map<List<Session>>(It.IsAny<List<SessionDto>>()), Times.Once);
            apiMock.Verify(a => a.Get("sessions?", $"meeting_key={meetingKey}", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void GetSessionsByMeetingKey_ApiThrows_ThrowsInvalidOperationException()
        {
            // Arrange
            var apiMock = new Mock<IF1ApiClient>();
            var mapperMock = new Mock<IMapper>();
            int meetingKey = 99;

            var inner = new Exception("boom");
            apiMock.Setup(a => a.Get("sessions?", $"meeting_key={meetingKey}", It.IsAny<CancellationToken>())).ThrowsAsync(inner);

            var client = new SessionClient(apiMock.Object, mapperMock.Object);

            // Act / Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await client.GetSessionsByMeetingKey(meetingKey))!;
            Assert.That(ex.Message, Is.EqualTo("Error occurred while fetching sessions."));
            Assert.That(ex.InnerException, Is.SameAs(inner));
            apiMock.Verify(a => a.Get("sessions?", $"meeting_key={meetingKey}", It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
