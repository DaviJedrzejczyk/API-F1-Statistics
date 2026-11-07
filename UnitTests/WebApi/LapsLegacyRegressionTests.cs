using System.Text.Json;
using AutoMapper;
using Entities.Class;
using Entities.Dtos;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Services.Interfaces;
using Shared.Responses;
using WebApi.Controllers;
using WebApi.ViewModels;

namespace UnitTests.WebApi;

[TestFixture]
public sealed class LapsLegacyRegressionTests
{
    private Mock<ILapService> _laps = null!;
    private Mock<ILapFastSectorService> _sectors = null!;
    private Mock<IMapper> _mapper = null!;
    private LapsController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _laps = new Mock<ILapService>(MockBehavior.Strict);
        _sectors = new Mock<ILapFastSectorService>(MockBehavior.Strict);
        _mapper = new Mock<IMapper>(MockBehavior.Strict);
        _controller = new LapsController(_sectors.Object, _mapper.Object, _laps.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _controller.Dispose();
    }

    [Test]
    public async Task FastestLap_PreservesTheExistingSuccessPayloadAndCancellationToken()
    {
        using CancellationTokenSource cancellation = new();
        LapFastLapDto lap = new() { SessionKey = 2200, MeetingKey = 1100, DriverNumber = 11, LapDuration = 91.25, IsFastLap = true };
        _laps.Setup(service => service.GetFastLapOfRaceBySessionKey(2200, cancellation.Token))
            .ReturnsAsync(new SingleResponse<LapFastLapDto> { HasSuccess = true, Item = lap });

        IActionResult response = await _controller.GetFastLapSession(2200, cancellation.Token);

        Assert.That(response, Is.TypeOf<OkObjectResult>());
        Assert.That(((OkObjectResult)response).Value, Is.SameAs(lap));
        using JsonDocument json = JsonDocument.Parse(JsonSerializer.Serialize(((OkObjectResult)response).Value));
        Assert.That(json.RootElement.GetProperty("duration").GetDouble(), Is.EqualTo(91.25));
        Assert.That(json.RootElement.GetProperty("driver_number").GetInt32(), Is.EqualTo(11));
        _laps.VerifyAll();
        Assert.That(_sectors.Invocations, Is.Empty);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task FastestLap_ServiceFailureStillReturns404WithThePublicErrorContract(bool exception)
    {
        SingleResponse<LapFastLapDto> failure = new()
        {
            HasSuccess = false,
            Message = "No laps found for the given session key.",
            Exception = exception ? new InvalidOperationException("private database details") : null!
        };
        _laps.Setup(service => service.GetFastLapOfRaceBySessionKey(2200, It.IsAny<CancellationToken>()))
            .ReturnsAsync(failure);

        IActionResult response = await _controller.GetFastLapSession(2200);

        AssertPublicError(response, exception ? "The request could not be completed." : failure.Message);
        Assert.That(_mapper.Invocations, Is.Empty);
    }

    [Test]
    public async Task FastestSectors_EmptyPersistedListStillReturns200WithoutCalculatingFastestLap()
    {
        List<LapFastSector> stored = [];
        List<LapFastSectorViewModel> mapped = [];
        _sectors.Setup(service => service.GetFastSectorsOfSession(2200, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DataResponse<LapFastSector> { HasSuccess = true, Itens = stored });
        _mapper.Setup(mapper => mapper.Map<List<LapFastSectorViewModel>>(stored)).Returns(mapped);

        IActionResult response = await _controller.GetFastSectorsSession(2200);

        Assert.That(response, Is.TypeOf<OkObjectResult>());
        Assert.That(((OkObjectResult)response).Value, Is.SameAs(mapped));
        Assert.That(_laps.Invocations, Is.Empty);
    }

    [Test]
    public async Task FastestSectors_PreservesPartialSectorObservationAndMapping()
    {
        List<LapFastSector> stored =
        [
            new() { SessionKey = 2200, DriverNumber = 11, Sector = 1, Duration = 29.5, DriverName = "Fixture Driver", Observation = "Partial sectors" }
        ];
        List<LapFastSectorViewModel> mapped =
        [
            new() { SessionKey = 2200, DriverNumber = 11, Sector = 1, Duration = 29.5, DriverName = "Fixture Driver", Observation = "Partial sectors" }
        ];
        _sectors.Setup(service => service.GetFastSectorsOfSession(2200, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DataResponse<LapFastSector> { HasSuccess = true, Itens = stored });
        _mapper.Setup(mapper => mapper.Map<List<LapFastSectorViewModel>>(stored)).Returns(mapped);

        IActionResult response = await _controller.GetFastSectorsSession(2200);

        Assert.That(response, Is.TypeOf<OkObjectResult>());
        Assert.That(((OkObjectResult)response).Value, Is.SameAs(mapped));
        using JsonDocument json = JsonDocument.Parse(JsonSerializer.Serialize(((OkObjectResult)response).Value));
        Assert.That(json.RootElement[0].GetProperty("observation").GetString(), Is.EqualTo("Partial sectors"));
        Assert.That(_laps.Invocations, Is.Empty);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task FastestSectors_ServiceFailureStillReturns404WithoutMapping(bool exception)
    {
        DataResponse<LapFastSector> failure = new()
        {
            HasSuccess = false,
            Message = "Stored sectors unavailable.",
            Exception = exception ? new InvalidOperationException("private database details") : null!
        };
        _sectors.Setup(service => service.GetFastSectorsOfSession(2200, It.IsAny<CancellationToken>()))
            .ReturnsAsync(failure);

        IActionResult response = await _controller.GetFastSectorsSession(2200);

        AssertPublicError(response, exception ? "The request could not be completed." : failure.Message);
        Assert.That(_mapper.Invocations, Is.Empty);
        Assert.That(_laps.Invocations, Is.Empty);
    }

    private static void AssertPublicError(IActionResult response, string expectedMessage)
    {
        Assert.That(response, Is.TypeOf<NotFoundObjectResult>());
        NotFoundObjectResult error = (NotFoundObjectResult)response;
        Assert.That(error.StatusCode, Is.EqualTo(404));
        Assert.That(error.Value, Is.TypeOf<ErrorViewModel>());
        using JsonDocument json = JsonDocument.Parse(JsonSerializer.Serialize(error.Value));
        Assert.That(json.RootElement.GetProperty("status_code").GetInt32(), Is.EqualTo(404));
        Assert.That(json.RootElement.GetProperty("message").GetString(), Is.EqualTo(expectedMessage));
        Assert.That(json.RootElement.GetRawText(), Does.Not.Contain("private database"));
    }
}
