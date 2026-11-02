using AutoMapper;
using Entities.Class;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Shared.Responses;
using WebApi.Config;
using WebApi.ViewModels;

namespace WebApi.Controllers.Meetings
{
    /// <summary>
    /// API controller that manages Meeting resources.
    /// Exposes endpoints to insert tracks for a season and to retrieve meeting information.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MeetingController : Controller
    {
        private readonly IMeetingService _meetingService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Creates a new instance of <see cref="MeetingController"/>.
        /// </summary>
        /// <param name="meetingService">Service used to manage meetings.</param>
        /// <param name="mapper">AutoMapper instance for mapping domain models to view models.</param>
        public MeetingController(IMeetingService meetingService, IMapper mapper)
        {
            _meetingService = meetingService;
            _mapper = mapper;
        }

        /// <summary>
        /// Inserts all tracks for the specified meeting year into the database.
        /// </summary>
        /// <param name="model">Request body containing the season year in the year property.</param>
        /// <returns>An insertion confirmation, or an HTTP error result.</returns>
        /// <response code="200">SuccessViewModel containing the service message and status_code 200.</response>
        /// <response code="400">ErrorViewModel when the service fails or the body is invalid; the message is generic when the service reports an exception.</response>
        /// <response code="415">ErrorViewModel when the request content type is not supported.</response>
        /// <response code="500">ErrorViewModel for unexpected server errors.</response>
        [HttpPost("tracks-season")]
        [ProducesResponseType(typeof(SuccessViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status415UnsupportedMediaType)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertAllTracksOfCurrentSeason(MeetingYearViewModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                Response response = await _meetingService.InsertTracksOfCurrentYear(model.Year, cancellationToken);

                if (!response.HasSuccess)
                    return BadRequest(new ErrorViewModel(400, ApiErrorMessages.FromResponse(response)));

                return Ok(new SuccessViewModel() { StatusCode = 200, Message = response.Message} );
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception)
            {
                return BadRequest(new ErrorViewModel(400, ApiErrorMessages.RequestFailed));
            }
        }

        /// <summary>
        /// Retrieves a meeting from the database by its key.
        /// </summary>
        /// <param name="meetingKey">Identifier of the meeting, supplied in the route.</param>
        /// <returns>The meeting view model, or an HTTP error result.</returns>
        /// <response code="200">The mapped MeetingViewModel.</response>
        /// <response code="400">ErrorViewModel when a caught exception occurs or the route value is invalid.</response>
        /// <response code="404">ErrorViewModel when no meeting is returned.</response>
        /// <response code="500">ErrorViewModel for unexpected server errors.</response>
        [HttpGet("{meetingKey}")]
        [ProducesResponseType(typeof(MeetingViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMeetingByKey(int meetingKey, CancellationToken cancellationToken = default)
        {
            try
            {
                SingleResponse<Meeting> response = await _meetingService.GetMeetingByKey(meetingKey, cancellationToken);

                if (response.Item == null)
                    return NotFound(new ErrorViewModel(404, ApiErrorMessages.FromResponse(response)));

                return Ok(_mapper.Map<MeetingViewModel>(response.Item));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception)
            {
                return BadRequest(new ErrorViewModel(400, ApiErrorMessages.RequestFailed));
            }
        }

        /// <summary>
        /// Retrieves all meeting tracks for the specified year.
        /// </summary>
        /// <param name="year">Season year to retrieve meeting tracks for, supplied in the route.</param>
        /// <returns>A list of meeting view models, or an HTTP error result.</returns>
        /// <response code="200">A list of MeetingViewModel objects.</response>
        /// <response code="400">ErrorViewModel when the service reports failure or a caught exception occurs; the message is generic when the service reports an exception.</response>
        /// <response code="500">ErrorViewModel for unexpected server errors.</response>
        [HttpGet("all-tracks/{year}")]
        [ProducesResponseType(typeof(List<MeetingViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllTracksOfCurrentYear(int year, CancellationToken cancellationToken = default)
        {
            try
            {
                DataResponse<Meeting> response = await _meetingService.GetAllTracksOfCurrentYear(year, cancellationToken);

                if (!response.HasSuccess)
                    return BadRequest(new ErrorViewModel(400, ApiErrorMessages.FromResponse(response)));

                return Ok(_mapper.Map<List<MeetingViewModel>>(response.Itens));
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception)
            {
                return BadRequest(new ErrorViewModel(400, ApiErrorMessages.RequestFailed));
            }
        }
    }
}
