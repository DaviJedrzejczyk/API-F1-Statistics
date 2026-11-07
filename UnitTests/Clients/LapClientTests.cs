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
        public async Task GetAllLapsSessionByDriver_NullAndAbsentStartAndPitOutRemainUnknown()
        {
            Mock<IF1ApiClient> client = new();
            client.Setup(client => client.Get("laps?", It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SingleResponse<string>
                {
                    HasSuccess = true,
                    Item = "[{\"date_start\":null,\"is_pit_out_lap\":null},{},{\"is_pit_out_lap\":false}]"
                });

            DataResponse<LapListDto> response = await new LapClient(client.Object).GetAllLapsSessionByDriver(100, 1);

            Assert.That(response.HasSuccess, Is.True);
            Assert.That(response.Itens[0].DateStart, Is.Null);
            Assert.That(response.Itens[0].IsPitOutLap, Is.Null);
            Assert.That(response.Itens[1].IsPitOutLap, Is.Null);
            Assert.That(response.Itens[2].IsPitOutLap, Is.False);
        }

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
            mockF1Client.Setup(m => m.Get("laps?", It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(singleResponse);

            var sut = new LapClient(mockF1Client.Object);

            // Act
            DataResponse<LapListDto> result = await sut.GetAllLapsSessionByDriver(123, 7);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.HasSuccess);
            Assert.IsNotNull(result.Itens);
            Assert.That(result.Itens.Count, Is.EqualTo(1));
            Assert.That(result.Itens[0].DriverNumber, Is.EqualTo(7));
            mockF1Client.Verify(m => m.Get("laps?", "session_key=123&driver_number=7", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetAllLapsSessionByDriver_PreservesMissingZeroAndSegments()
        {
            var client = new Mock<IF1ApiClient>();
            client.Setup(x => x.Get("laps?", It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SingleResponse<string> { HasSuccess = true, Item = "[{\"lap_duration\":0,\"duration_sector_1\":null,\"duration_sector_2\":0,\"i1_speed\":0,\"i2_speed\":null,\"segments_sector_1\":[0,null,2049],\"segments_sector_2\":[],\"segments_sector_3\":null},{}]" });
            var result = await new LapClient(client.Object).GetAllLapsSessionByDriver(1, 2);
            Assert.That(result.HasSuccess, Is.True);
            var lap = result.Itens[0];
            Assert.Multiple(() => {
                Assert.That(lap.LapDuration, Is.EqualTo(0));
                Assert.That(lap.DurationSector1, Is.Null);
                Assert.That(lap.DurationSector2, Is.EqualTo(0));
                Assert.That(lap.I1Speed, Is.EqualTo(0));
                Assert.That(lap.I2Speed, Is.Null);
                Assert.That(lap.SegmentsSector1, Is.EqualTo(new int?[] { 0, null, 2049 }));
                Assert.That(lap.SegmentsSector2, Is.Empty);
                Assert.That(lap.SegmentsSector3, Is.Null);
                Assert.That(result.Itens[1].LapDuration, Is.Null);
            });
        }

        [Test]
        public async Task GetAllLapsSessionByDriver_ApiReturnsFailure_ReturnsFailureDataResponse()
        {
            // Arrange
            var mockF1Client = new Mock<IF1ApiClient>();
            var ex = new Exception("api error");
            var singleResponse = new SingleResponse<string> { HasSuccess = false, Message = "err", Exception = ex };
            mockF1Client.Setup(m => m.Get("laps?", It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(singleResponse);

            var sut = new LapClient(mockF1Client.Object);

            // Act
            DataResponse<LapListDto> result = await sut.GetAllLapsSessionByDriver(1, 2);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("err"));
            Assert.That(result.Exception, Is.EqualTo(ex));
            mockF1Client.Verify(m => m.Get("laps?", "session_key=1&driver_number=2", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetAllLapsSessionByDriver_ApiReturnsNullJson_ReturnsFailureDueToDeserialization()
        {
            // Arrange
            var mockF1Client = new Mock<IF1ApiClient>();
            var singleResponse = new SingleResponse<string> { HasSuccess = true, Message = "ok", Item = "null" };
            mockF1Client.Setup(m => m.Get("laps?", It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(singleResponse);

            var sut = new LapClient(mockF1Client.Object);

            // Act
            DataResponse<LapListDto> result = await sut.GetAllLapsSessionByDriver(9, 9);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Message, Is.EqualTo("Failed to deserialize lap list."));
            mockF1Client.Verify(m => m.Get("laps?", "session_key=9&driver_number=9", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetAllLapsSessionByDriver_ApiThrowsException_ReturnsFailureWithException()
        {
            // Arrange
            var mockF1Client = new Mock<IF1ApiClient>();
            var thrown = new InvalidOperationException("boom");
            mockF1Client.Setup(m => m.Get("laps?", It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(thrown);

            var sut = new LapClient(mockF1Client.Object);

            // Act
            DataResponse<LapListDto> result = await sut.GetAllLapsSessionByDriver(5, 6);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.HasSuccess);
            Assert.That(result.Exception, Is.TypeOf<InvalidOperationException>());
            Assert.That(result.Message, Is.EqualTo("boom"));
            mockF1Client.Verify(m => m.Get("laps?", "session_key=5&driver_number=6", It.IsAny<CancellationToken>()), Times.Once);
        }    }
}
