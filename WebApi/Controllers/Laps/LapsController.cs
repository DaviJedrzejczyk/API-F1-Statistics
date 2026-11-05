using AutoMapper;
using Entities.Dtos;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Services.Models;
using WebApi.Config;
using WebApi.ViewModels;

namespace WebApi.Controllers
{
    /// <summary>
    /// API controller that retrieves the fastest laps and sectors for sessions.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class LapsController : Controller
    {
        private readonly ILapFastSectorService _lapFastSectorService;
        private readonly ILapService _lapService;
        private readonly IMapper _mapper;
        private readonly ILapCatalogService _catalog;

        /// <summary>
        /// Creates a new instance of <see cref="LapsController"/>.
        /// </summary>
        /// <param name="lapFastSectorService">Service used to retrieve the fastest sectors.</param>
        /// <param name="mapper">AutoMapper instance for mapping sectors to view models.</param>
        /// <param name="lapService">Service used to retrieve the fastest lap.</param>
        public LapsController(ILapFastSectorService lapFastSectorService, IMapper mapper, ILapService lapService, ILapCatalogService catalog)
        {
            _lapFastSectorService = lapFastSectorService;
            _mapper = mapper;
            _lapService = lapService;
            _catalog = catalog;
        }

        // Retains direct construction used by existing controller tests.
        public LapsController(ILapFastSectorService lapFastSectorService, IMapper mapper, ILapService lapService)
            : this(lapFastSectorService, mapper, lapService, null!) { }

        /// <summary>Imports all laps for exactly three drivers of a completed historical session.</summary>
        /// <param name="sessionKey">Historical session key.</param>
        /// <param name="request">Exactly three distinct driver numbers.</param>
        /// <param name="lapNumber">Optional lap number filter.</param>
        /// <param name="compound">Optional tyre compound filter.</param>
        /// <param name="hasAlerts">Optional alert presence filter.</param>
        /// <param name="cancellationToken">Request cancellation token.</param>
        [HttpPost("catalog/{sessionKey:int}")]
        [ProducesResponseType(typeof(LapCatalogResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ImportCatalog(int sessionKey, [FromBody] LapCatalogRequest request,
            [FromQuery] int? lapNumber, [FromQuery] string? compound, [FromQuery] bool? hasAlerts,
            CancellationToken cancellationToken)
        {
            try { return Ok(await _catalog.Get(sessionKey, request.DriverNumbers, lapNumber, compound, hasAlerts, true, cancellationToken)); }
            catch (LapCatalogException ex) { return StatusCode(ex.StatusCode, new ErrorViewModel(ex.StatusCode, ex.Message)); }
        }

        /// <summary>Reads a persisted three-driver lap catalog with optional filters.</summary>
        /// <param name="sessionKey">Historical session key.</param>
        /// <param name="driverNumbers">Exactly three distinct driver numbers.</param>
        /// <param name="lapNumber">Optional lap number filter.</param>
        /// <param name="compound">Optional tyre compound filter.</param>
        /// <param name="hasAlerts">Optional alert presence filter.</param>
        /// <param name="cancellationToken">Request cancellation token.</param>
        [HttpGet("catalog/{sessionKey:int}")]
        [ProducesResponseType(typeof(LapCatalogResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCatalog(int sessionKey, [FromQuery] int[] driverNumbers,
            [FromQuery] int? lapNumber, [FromQuery] string? compound, [FromQuery] bool? hasAlerts,
            CancellationToken cancellationToken)
        {
            try { return Ok(await _catalog.Get(sessionKey, driverNumbers, lapNumber, compound, hasAlerts, false, cancellationToken)); }
            catch (LapCatalogException ex) { return StatusCode(ex.StatusCode, new ErrorViewModel(ex.StatusCode, ex.Message)); }
        }

        /// <summary>Previews three independently chosen laps without saving the selection.</summary>
        /// <param name="sessionKey">Historical session key.</param>
        /// <param name="request">One driver and lap number per selection.</param>
        /// <param name="cancellationToken">Request cancellation token.</param>
        [HttpPost("catalog/{sessionKey:int}/preview")]
        [ProducesResponseType(typeof(LapSelectionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PreviewSelection(int sessionKey, [FromBody] LapSelectionRequest request,
            CancellationToken cancellationToken)
        {
            try { return Ok(await _catalog.Select(sessionKey, request, false, cancellationToken)); }
            catch (LapCatalogException ex) { return StatusCode(ex.StatusCode, new ErrorViewModel(ex.StatusCode, ex.Message)); }
        }

        /// <summary>Confirms and persists a previewed three-lap selection.</summary>
        /// <param name="sessionKey">Historical session key.</param>
        /// <param name="request">One driver and lap number per selection.</param>
        /// <param name="cancellationToken">Request cancellation token.</param>
        [HttpPost("catalog/{sessionKey:int}/confirm")]
        [ProducesResponseType(typeof(LapSelectionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConfirmSelection(int sessionKey, [FromBody] LapSelectionRequest request,
            CancellationToken cancellationToken)
        {
            try { return Ok(await _catalog.Select(sessionKey, request, true, cancellationToken)); }
            catch (LapCatalogException ex) { return StatusCode(ex.StatusCode, new ErrorViewModel(ex.StatusCode, ex.Message)); }
        }

        /// <summary>Reopens the persisted selection for a session.</summary>
        /// <param name="sessionKey">Historical session key.</param>
        /// <param name="cancellationToken">Request cancellation token.</param>
        [HttpGet("catalog/{sessionKey:int}/selection")]
        [ProducesResponseType(typeof(LapSelectionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSelection(int sessionKey, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _catalog.GetConfirmed(sessionKey, cancellationToken);
                return result == null ? NotFound(new ErrorViewModel(404, "No confirmed selection for this session.")) : Ok(result);
            }
            catch (LapCatalogException ex) { return StatusCode(ex.StatusCode, new ErrorViewModel(ex.StatusCode, ex.Message)); }
        }


        /// <summary>
        /// Retrieves the fastest lap for the specified session.
        /// </summary>
        /// <param name="sessionKey">Identifier of the session, supplied in the query string.</param>
        /// <returns>The fastest lap, or an HTTP error result.</returns>
        /// <response code="200">A LapFastLapDto when the service succeeds.</response>
        /// <response code="400">ErrorViewModel for invalid input (model binding failure).</response>
        /// <response code="404">ErrorViewModel whenever the service reports a failure; the message is generic when the service reports an exception.</response>
        /// <response code="500">ErrorViewModel for unexpected server errors.</response>
        [HttpGet("fast_lap_session")]
        [ProducesResponseType(typeof(LapFastLapDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFastLapSession(int sessionKey, CancellationToken cancellationToken = default)
        {
            var result = await _lapService.GetFastLapOfRaceBySessionKey(sessionKey, cancellationToken);

            if (result.HasSuccess) return Ok(result.Item);

            return NotFound(new ErrorViewModel(404, ApiErrorMessages.FromResponse(result)));
        }

        /// <summary>
        /// Retrieves the fastest sectors already stored for the specified session.
        /// </summary>
        /// <param name="sessionKey">Identifier of the session, supplied in the query string.</param>
        /// <returns>A list of fastest-sector view models, or an HTTP error result.</returns>
        /// <response code="200">A list of LapFastSectorViewModel objects.</response>
        /// <response code="400">ErrorViewModel for invalid input (model binding failure).</response>
        /// <response code="404">ErrorViewModel whenever the service reports a failure, including missing stored sectors; the message is generic when the service reports an exception.</response>
        /// <response code="500">ErrorViewModel for unexpected server errors.</response>
        /// <remarks>
        /// This action only reads stored sectors. GET /api/Laps/fast_lap_session can populate them while retrieving laps.
        /// </remarks>
        [HttpGet("fast_sectors_session")]
        [ProducesResponseType(typeof(List<LapFastSectorViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFastSectorsSession(int sessionKey, CancellationToken cancellationToken = default)
        {
            var result = await _lapFastSectorService.GetFastSectorsOfSession(sessionKey, cancellationToken);
            if (result.HasSuccess) return Ok(_mapper.Map<List<LapFastSectorViewModel>>(result.Itens));

            return NotFound(new ErrorViewModel(404, ApiErrorMessages.FromResponse(result)));
        }
    }
}
