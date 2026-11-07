using Entities.Dtos;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Services.Models;
using Shared.Responses;
using WebApi.Config;
using WebApi.ViewModels;

namespace WebApi.Controllers;

/// <summary>Imports historical lap catalogs and persists three-driver selection previews.</summary>
[ApiController]
[Route("api/Laps/{sessionKey:int}")]
[ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status409Conflict)]
[ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status502BadGateway)]
[ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status503ServiceUnavailable)]
[ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status500InternalServerError)]
public sealed class LapCatalogController : ControllerBase
{
    private readonly ILapCatalogService _service;

    public LapCatalogController(ILapCatalogService service)
    {
        _service = service;
    }

    /// <summary>Imports missing or demonstrably incomplete catalogs for exactly three session participants.</summary>
    /// <remarks>Session and driver metadata must already be persisted. Failed enrichment is retried on subsequent imports. Missing measurements remain selectable.</remarks>
    /// <param name="sessionKey">Historical session identifier.</param>
    /// <param name="request">Exactly three driver_numbers participating in the session.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("catalog/import")]
    [ProducesResponseType(typeof(LapCatalogDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Import(int sessionKey, [FromBody] LapCatalogRequest request, CancellationToken cancellationToken)
    {
        SingleResponse<LapCatalogDto> response = await _service.GetCatalog(sessionKey, request.DriverNumbers,
            null, null, null, true, cancellationToken);
        return Result(response);
    }

    /// <summary>Reopens three persisted driver catalogs, filtering laps before choosing each suggestion.</summary>
    /// <remarks>Never calls OpenF1. Repeat driverNumbers three times in the query. hasAlerts=false means no identified alerts, not official validity.</remarks>
    /// <param name="sessionKey">Historical session identifier.</param>
    /// <param name="driverNumbers">Exactly three distinct session participant numbers.</param>
    /// <param name="lapNumber">Optional exact lap number filter.</param>
    /// <param name="compound">Optional case-insensitive tyre compound filter.</param>
    /// <param name="hasAlerts">Optional filter for the presence of identified alerts.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("catalog")]
    [ProducesResponseType(typeof(LapCatalogDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(int sessionKey, [FromQuery] int[] driverNumbers, [FromQuery] int? lapNumber,
        [FromQuery] string? compound, [FromQuery] bool? hasAlerts, CancellationToken cancellationToken)
    {
        SingleResponse<LapCatalogDto> response = await _service.GetCatalog(sessionKey, driverNumbers,
            lapNumber, compound, hasAlerts, false, cancellationToken);
        return Result(response);
    }

    /// <summary>Persists a preview of three independently selected laps, preserving all identified alerts.</summary>
    /// <remarks>Selections may include missing duration, pit laps or deleted times. Confirmation requires this preview's identifier.</remarks>
    /// <param name="sessionKey">Historical session identifier.</param>
    /// <param name="request">Three selections containing driver_number and lap_number.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("selection/preview")]
    [ProducesResponseType(typeof(LapSelectionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Preview(int sessionKey, [FromBody] LapSelectionRequest request, CancellationToken cancellationToken)
    {
        SingleResponse<LapSelectionDto> response = await _service.Preview(sessionKey, request, cancellationToken);
        return Result(response);
    }

    /// <summary>Confirms exactly the persisted preview and saves it as this session's latest selection.</summary>
    /// <param name="sessionKey">Historical session identifier.</param>
    /// <param name="previewId">Identifier returned by the preview endpoint.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("selection/{previewId:guid}/confirm")]
    [ProducesResponseType(typeof(LapSelectionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Confirm(int sessionKey, Guid previewId, CancellationToken cancellationToken)
    {
        SingleResponse<LapSelectionDto> response = await _service.Confirm(sessionKey, previewId, cancellationToken);
        return Result(response);
    }

    /// <summary>Reopens a persisted preview or confirmed selection without external requests.</summary>
    /// <param name="sessionKey">Historical session identifier.</param>
    /// <param name="previewId">Persisted preview identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("selection/{previewId:guid}")]
    [ProducesResponseType(typeof(LapSelectionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPreview(int sessionKey, Guid previewId, CancellationToken cancellationToken)
    {
        SingleResponse<LapSelectionDto> response = await _service.GetSelection(sessionKey, previewId, cancellationToken);
        return Result(response);
    }

    /// <summary>Reopens the session's latest confirmed selection without external requests.</summary>
    /// <param name="sessionKey">Historical session identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("selection")]
    [ProducesResponseType(typeof(LapSelectionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSelection(int sessionKey, CancellationToken cancellationToken)
    {
        SingleResponse<LapSelectionDto> response = await _service.GetSelection(sessionKey, null, cancellationToken);
        return Result(response);
    }

    private IActionResult Result<T>(SingleResponse<T> response)
    {
        if (response.HasSuccess)
        {
            return Ok(response.Item);
        }
        if (response.Exception is LapCatalogException failure)
        {
            return StatusCode(failure.StatusCode, new ErrorViewModel(failure.StatusCode, failure.Message));
        }
        return StatusCode(503, new ErrorViewModel(503, ApiErrorMessages.RequestFailed));
    }
}
