using AutoMapper;
using Entities.Dtos;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using WebApi.Config;
using WebApi.ViewModels;

namespace WebApi.Controllers.Overtakes
{
    /// <summary>
    /// API controller that counts overtakes for sessions.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class OvertakeController : Controller
    {
        private readonly IOvertakeService _overtakeService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Creates a new instance of <see cref="OvertakeController"/>.
        /// </summary>
        /// <param name="overtakeService">Service used to retrieve overtakes.</param>
        /// <param name="mapper">AutoMapper instance supplied to the controller.</param>
        public OvertakeController(IOvertakeService overtakeService, IMapper mapper)
        {
            _overtakeService = overtakeService;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves the overtake count for the specified session.
        /// </summary>
        /// <param name="sessionKey">Identifier of the session, supplied in the route.</param>
        /// <returns>The overtake count and meeting/session identifiers, or an HTTP error result.</returns>
        /// <response code="200">OvertakeCountViewModel using identifiers from the first overtake and the total number of returned overtakes.</response>
        /// <response code="400">ErrorViewModel when the service reports a failure, or for invalid input (model binding failure); the message is generic when the service reports an exception.</response>
        /// <response code="500">ErrorViewModel for unexpected server errors.</response>
        [HttpGet("get-overtakes-session/{sessionKey}")]
        [ProducesResponseType(typeof(OvertakeCountViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOvertakesSession(int sessionKey)
        {
            var response = await _overtakeService.GetOvertakesSessionApi(sessionKey);
            if (!response.HasSuccess) return BadRequest(new ErrorViewModel(400, ApiErrorMessages.FromResponse(response)));

            return Ok(new OvertakeCountViewModel(response.Itens[0].MeetingKey, response.Itens[0].SessionKey, response.Itens.Count));
        }
    }
}
