using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using WebApi.ViewModels;

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
        [HttpPost("insert-sessions")]
        [ProducesResponseType(typeof(SuccessViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertSessionsInDataBase(SessionKeyViewModel sessionKeyViewModel)
        {
            try
            {
                var response = await _sessionService.InsertSessions(sessionKeyViewModel.SessionKey);

                if (!response.HasSuccess)
                    return BadRequest(new ErrorViewModel(){ Message = response.Message, StatusCode = 400});

                return Ok(new SuccessViewModel() { StatusCode = 200, Message = response.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{sessionId}")]
        [ProducesResponseType(typeof(SuccessViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSessionById(int meetingkey, int sessionId)
        {
            var response = await _sessionService.GetSessionByMeetingKeySessionKey(meetingkey, sessionId);

            if (!response.HasSuccess && response.Item == null) return NotFound(new ErrorViewModel(404, response.Message));
            if (!response.HasSuccess && response.Exception != null) return BadRequest(new ErrorViewModel(400, response.Message));

            return Ok(new SuccessViewModel(200, response.Message));
        }
    }
}
