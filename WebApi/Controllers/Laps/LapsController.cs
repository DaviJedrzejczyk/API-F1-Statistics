using AutoMapper;
using Entities.Dtos;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
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

        /// <summary>
        /// Creates a new instance of <see cref="LapsController"/>.
        /// </summary>
        /// <param name="lapFastSectorService">Service used to retrieve the fastest sectors.</param>
        /// <param name="mapper">AutoMapper instance for mapping sectors to view models.</param>
        /// <param name="lapService">Service used to retrieve the fastest lap.</param>
        public LapsController(ILapFastSectorService lapFastSectorService, IMapper mapper, ILapService lapService)
        {
            _lapFastSectorService = lapFastSectorService;
            _mapper = mapper;
            _lapService = lapService;
        }


        /// <summary>
        /// Retrieves the fastest lap for the specified session.
        /// </summary>
        /// <param name="sessionKey">Identifier of the session, supplied in the query string.</param>
        /// <returns>The fastest lap, or an HTTP error result.</returns>
        /// <response code="200">A LapFastLapDto when the service succeeds.</response>
        /// <response code="400">ValidationProblemDetails for invalid input.</response>
        /// <response code="404">ErrorViewModel whenever the service reports a failure.</response>
        /// <remarks>The final BadRequest branch is unreachable: a service failure always returns 404.</remarks>
        [HttpGet("fast_lap_session")]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(typeof(ErrorViewModel), 404)]
        [ProducesResponseType(typeof(LapFastLapDto), 200)]
        public async Task<IActionResult> GetFastLapSession(int sessionKey)
        {
            var result = await _lapService.GetFastLapOfRaceBySessionKey(sessionKey);

            if (result.HasSuccess) return Ok(result.Item);

            if (!result.HasSuccess || result.Item == null)
                return NotFound(new ErrorViewModel(404, result.Message));

            return BadRequest(new ErrorViewModel(400, result.Message));
        }

        /// <summary>
        /// Retrieves the fastest sectors already stored for the specified session.
        /// </summary>
        /// <param name="sessionKey">Identifier of the session, supplied in the query string.</param>
        /// <returns>A list of fastest-sector view models, or an HTTP error result.</returns>
        /// <response code="200">A list of LapFastSectorViewModel objects.</response>
        /// <response code="400">ValidationProblemDetails for invalid input.</response>
        /// <response code="404">ErrorViewModel whenever the service reports a failure, including missing stored sectors.</response>
        /// <remarks>
        /// This action only reads stored sectors. GET /api/Laps/fast_lap_session can populate them while retrieving laps.
        /// The final BadRequest branch is unreachable: a service failure always returns 404.
        /// </remarks>
        [HttpGet("fast_sectors_session")]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(typeof(ErrorViewModel), 404)]
        [ProducesResponseType(typeof(List<LapFastSectorViewModel>), 200)]
        public async Task<IActionResult> GetFastSectorsSession(int sessionKey)
        {
            var result = await _lapFastSectorService.GetFastSectorsOfSession(sessionKey);
            if (result.HasSuccess) return Ok(_mapper.Map<List<LapFastSectorViewModel>>(result.Itens));

            if (!result.HasSuccess || result.Itens == null)
                return NotFound(new ErrorViewModel(404, result.Message));

            return BadRequest(new ErrorViewModel(400, result.Message));
        }
    }
}
