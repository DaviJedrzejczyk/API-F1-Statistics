using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using WebApi.Config;
using WebApi.ViewModels;

namespace WebApi.Controllers.Sessions
{
    /// <summary>
    /// API controller that manages Session resources.
    /// Exposes endpoints to insert sessions for a meeting and to confirm a session lookup.
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
        /// <param name="keyViewModel">Request body containing the meeting identifier in the meeting_key property.</param>
        /// <returns>An insertion confirmation, or an HTTP error result.</returns>
        /// <response code="200">SuccessViewModel containing the service message and status_code 200.</response>
        /// <response code="400">ErrorViewModel when insertion fails or the body is invalid; the message is generic when the service reports an exception.</response>
        /// <response code="415">ErrorViewModel when the request content type is not supported.</response>
        /// <response code="500">ErrorViewModel for unexpected server errors.</response>
        [HttpPost("insert-sessions")]
        [ProducesResponseType(typeof(SuccessViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status415UnsupportedMediaType)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertSessionsInDataBase(SessionMeetingKeyViewModel keyViewModel)
        {
            try
            {
                var response = await _sessionService.InsertSessions(keyViewModel.MeetingKey);

                if (!response.HasSuccess)
                    return BadRequest(new ErrorViewModel(400, ApiErrorMessages.FromResponse(response)));

                return Ok(new SuccessViewModel() { StatusCode = 200, Message = response.Message });
            }
            catch (Exception)
            {
                return BadRequest(new ErrorViewModel(400, ApiErrorMessages.RequestFailed));
            }
        }

        /// <summary>
        /// Looks up a session using its meeting and session keys and returns a confirmation message.
        /// </summary>
        /// <param name="meetingkey">Identifier of the meeting, supplied in the query string as meetingkey.</param>
        /// <param name="sessionKey">Identifier of the session, supplied in the route.</param>
        /// <returns>A lookup confirmation without session data, or an HTTP error result.</returns>
        /// <response code="200">SuccessViewModel containing only status_code 200 and the service message when neither error branch is taken.</response>
        /// <response code="400">ErrorViewModel when the service fails with a non-null item and an exception; invalid query or route values also return 400.</response>
        /// <response code="404">ErrorViewModel when the service fails with a null item.</response>
        /// <response code="500">ErrorViewModel for unexpected server errors.</response>
        [HttpGet("{sessionKey}")]
        [ProducesResponseType(typeof(SuccessViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSessionBySessionKey(int meetingkey, int sessionKey)
        {
            var response = await _sessionService.GetSessionByMeetingKeySessionKey(meetingkey, sessionKey);

            if (!response.HasSuccess && response.Item == null) return NotFound(new ErrorViewModel(404, ApiErrorMessages.FromResponse(response)));
            if (!response.HasSuccess && response.Exception != null) return BadRequest(new ErrorViewModel(400, ApiErrorMessages.FromResponse(response)));

            return Ok(new SuccessViewModel(200, response.Message));
        }
    }
}
