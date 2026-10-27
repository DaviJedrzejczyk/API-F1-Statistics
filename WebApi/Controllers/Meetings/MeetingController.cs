using AutoMapper;
using Entities.Class;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Shared.Responses;
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
        /// <response code="400">A string containing the service failure or caught exception message; ValidationProblemDetails for invalid input.</response>
        [HttpPost("tracks-season")]
        [ProducesResponseType(typeof(SuccessViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> InsertAllTracksOfCurrentSeason(MeetingYearViewModel model)
        {
            try
            {
                Response response = await _meetingService.InsertTracksOfCurrentYear(model.Year);

                if (!response.HasSuccess)
                    return BadRequest(response.Message);

                return Ok(new SuccessViewModel() { StatusCode = 200, Message = response.Message} );
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves a meeting from the database by its key.
        /// </summary>
        /// <param name="meetingKey">Identifier of the meeting, supplied in the route.</param>
        /// <returns>The meeting view model, or an HTTP error result.</returns>
        /// <response code="200">The mapped MeetingViewModel.</response>
        /// <response code="400">A string containing a caught exception message; ValidationProblemDetails for invalid input.</response>
        /// <response code="404">A string containing the service message when no meeting is returned.</response>
        [HttpGet("{meetingKey}")]
        [ProducesResponseType(typeof(MeetingViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMeetingByKey(int meetingKey)
        {
            try
            {
                SingleResponse<Meeting> response = await _meetingService.GetMeetingByKey(meetingKey);

                if (response.Item == null)
                    return NotFound(response.Message);

                return Ok(_mapper.Map<MeetingViewModel>(response.Item));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves all meeting tracks for the specified year.
        /// </summary>
        /// <param name="year">Season year to retrieve meeting tracks for, supplied in the route.</param>
        /// <returns>A list of meeting view models, or an HTTP error result.</returns>
        /// <response code="200">A list of MeetingViewModel objects.</response>
        /// <response code="400">DataResponse&lt;Meeting&gt; when the service reports failure, a string for a caught exception, or ValidationProblemDetails for invalid input.</response>
        [HttpGet("all-tracks/{year}")]
        [ProducesResponseType(typeof(List<MeetingViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DataResponse<Meeting>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllTracksOfCurrentYear(int year)
        {
            try
            {
                DataResponse<Meeting> response = await _meetingService.GetAllTracksOfCurrentYear(year);

                if (!response.HasSuccess)
                    return BadRequest(response);

                return Ok(_mapper.Map<List<MeetingViewModel>>(response.Itens));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
