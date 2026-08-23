using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using WebApi.ViewModels.Errors;
using WebApi.ViewModels.Success;

namespace WebApi.Controllers.Sessions
{
    /// <summary>
    /// API controller that manages Session resources.
    /// Exposes endpoints to insert sessions for a meeting and to retrieve session-specific data.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SessionController : Controller
    {
        private readonly ISessionService _sessionService;

        /// <summary>
        /// Creates a new instance of <see cref="SessionController"/>.
        /// </summary>
        /// <param name="sessionService">Service used to manage sessions.</param>
        public SessionController(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        /// <summary>
        /// Inserts all sessions for the specified meeting into the database.
        /// </summary>
        /// <param name="meetingKey">Identifier of the meeting to insert sessions for.</param>
        /// <returns>
        /// 200 OK with a SuccessViewModel when insertion succeeds.
        /// 400 BadRequest when insertion fails.
        /// </returns>
        [HttpGet("InsertSessions")]
        [ProducesResponseType(typeof(SuccessViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertSessionsInDataBase(int meetingKey)
        {
            try
            {
                var response = await _sessionService.InsertSessions(meetingKey);

                if (!response.HasSuccess)
                    return BadRequest(response.Message);

                return Ok(new SuccessViewModel() { StatusCode = 200, Message = response.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves high-speed data for a specified session within a meeting.
        /// </summary>
        /// <param name="sessionKey">Identifier of the session.</param>
        /// <param name="meetingKey">Identifier of the meeting the session belongs to.</param>
        /// <returns>
        /// 200 OK with the high-speed data when implemented.
        /// </returns>
        [HttpGet("HighSpeed")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> HighSpeedInSession(int sessionKey, int meetingKey)
        {
            // Implementation for getting high-speed data in a session
            throw new NotImplementedException();
        }
    }
}
