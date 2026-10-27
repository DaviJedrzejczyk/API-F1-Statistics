using AutoMapper;
using Entities.Class;
using Entities.Dtos;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Shared.Responses;
using WebApi.ViewModels;

namespace WebApi.Controllers.SessionResults
{
    /// <summary>
    /// API controller that retrieves session results.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SessionResultController : Controller
    {
        private readonly ISessionResultService _sessionResultService;
        private readonly IMapper _mapper;
        /// <summary>
        /// Creates a new instance of <see cref="SessionResultController"/>.
        /// </summary>
        /// <param name="sessionResultService">Service used to retrieve session results.</param>
        /// <param name="mapper">AutoMapper instance for mapping results to view models.</param>
        public SessionResultController(ISessionResultService sessionResultService, IMapper mapper)
        {
            _sessionResultService = sessionResultService;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves the results for the specified session.
        /// </summary>
        /// <param name="sessionKey">Identifier of the session, supplied in the query string.</param>
        /// <returns>A list of session-result view models, or an HTTP error result.</returns>
        /// <response code="200">A list of SessionResultListViewModel objects when neither error branch is taken.</response>
        /// <response code="400">ErrorViewModel with status_code 404 when the service fails with a null list; ValidationProblemDetails for invalid input.</response>
        /// <response code="404">ErrorViewModel with status_code 400 when the service fails with an empty list.</response>
        /// <remarks>The status_code in these service-error payloads differs from the HTTP status. A failed response with a nonempty list still returns HTTP 200.</remarks>
        [HttpGet("result")]
        [ProducesResponseType(typeof(List<SessionResultListViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSessionResultBySessionKey(int sessionKey)
        {
            DataResponse<SessionResult> response = await _sessionResultService.GetSessionResultBySessionKeyApi(sessionKey);

            if (!response.HasSuccess)
            {
                if (response.Itens == null) return BadRequest(new ErrorViewModel(404, response.Message));
                else if (response.Itens.Count <= 0) return NotFound(new ErrorViewModel(400, response.Message));
            }

            var result = _mapper.Map<List<SessionResultListViewModel>>(response.Itens);

            return Ok(result);
        }

    }
}
