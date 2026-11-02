using Entities.Class;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using WebApi.Config;
using WebApi.ViewModels;

namespace WebApi.Controllers.Pits;

/// <summary>Queries and imports pit data for a session.</summary>
[ApiController]
[Route("api/[controller]")]
public class PitController : ControllerBase
{
    private readonly IPitService service;
    /// <summary>Creates the controller.</summary>
    /// <param name="service">The resource service.</param>
    public PitController(IPitService service) => this.service = service;

    /// <summary>Reads persisted pit data for a session.</summary>
    /// <param name="sessionKey">Session identifier.</param>
    /// <response code="200">The persisted records, possibly empty.</response>
    /// <response code="400">The request or service failed.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpGet("{sessionKey}")]
    [ProducesResponseType(typeof(List<Pit>), 200)]
    [ProducesResponseType(typeof(ErrorViewModel), 400)]
    [ProducesResponseType(typeof(ErrorViewModel), 500)]
    public async Task<IActionResult> Get(int sessionKey)
    {
        var response = await service.GetPitsBySessionKeyDb(sessionKey);
        if (!response.HasSuccess || response.Itens == null) return BadRequest(new ErrorViewModel(400, ApiErrorMessages.FromResponse(response)));
        return Ok(response.Itens);
    }

    /// <summary>Imports the pit window for a session.</summary>
    /// <param name="sessionKey">Session identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="200">The imported records, possibly empty.</response>
    /// <response code="400">The request or import failed.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpPost("{sessionKey}/import")]
    [ProducesResponseType(typeof(List<Pit>), 200)]
    [ProducesResponseType(typeof(ErrorViewModel), 400)]
    [ProducesResponseType(typeof(ErrorViewModel), 500)]
    public async Task<IActionResult> Import(int sessionKey, CancellationToken cancellationToken)
    {
        var response = await service.GetPitsBySessionKeyApi(sessionKey, cancellationToken);
        if (!response.HasSuccess || response.Itens == null) return BadRequest(new ErrorViewModel(400, ApiErrorMessages.FromResponse(response)));
        return Ok(response.Itens);
    }
}
