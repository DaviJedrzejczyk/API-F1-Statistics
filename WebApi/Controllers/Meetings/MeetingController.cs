using AutoMapper;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Shared.Responses;
using System.Text.Json;
using WebApi.Controllers.Constants;
using WebApi.ViewModels.Errors;
using WebApi.ViewModels.Meetings;
using WebApi.ViewModels.Success;

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
        /// <param name="model">View model containing the Year to insert tracks for.</param>
        /// <returns>
        /// 200 OK with a Response object when insertion succeeds.
        /// 400 BadRequest when model is invalid or insertion failed.
        /// </returns>
        [HttpPost("tracks-season")]
        [ProducesResponseType(typeof(SuccessViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status500InternalServerError)]
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
        /// Search a especify meeting in database
        /// </summary>
        /// <param name="meetingKey">Identifier of meeting</param>
        /// <returns>Return the meeting</returns>
        /// <returns code="200">The meeting has be found</returns>
        /// <returns code="404">The meeting was not found or have a problem to found</returns>
        [HttpGet("{meetingKey}")]
        [ProducesResponseType(typeof(Meeting)       , StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status500InternalServerError)]
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
        /// <param name="year">The year to retrieve meeting tracks for.</param>
        /// <returns>
        /// 200 OK with a list of MeetingViewModel when found.
        /// 400 BadRequest when the service reports a failure.
        /// </returns>
        [HttpGet("all-tracks/{year}")]
        [ProducesResponseType(typeof(List<MeetingViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status500InternalServerError)]
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
