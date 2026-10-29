using AutoMapper;
using Entities.Class;
using Entities.Dtos;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Shared.Responses;
using WebApi.Config;
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
        /// <response code="200">A list of SessionResultListViewModel objects when neither error branch is taken. A failed response with a nonempty list still returns HTTP 200.</response>
        /// <response code="400">ErrorViewModel when the service fails with a null list, or for invalid input (model binding failure).</response>
        /// <response code="404">ErrorViewModel when the service fails with an empty list.</response>
        /// <response code="500">ErrorViewModel for unexpected server errors.</response>
        [HttpGet("result")]
        [ProducesResponseType(typeof(List<SessionResultListViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSessionResultBySessionKey(int sessionKey)
        {
            DataResponse<SessionResult> response = await _sessionResultService.GetSessionResultBySessionKeyApi(sessionKey);

            if (!response.HasSuccess)
            {
                if (response.Itens == null) return BadRequest(new ErrorViewModel(400, ApiErrorMessages.FromResponse(response)));
                else if (response.Itens.Count <= 0) return NotFound(new ErrorViewModel(404, ApiErrorMessages.FromResponse(response)));
            }

            var result = _mapper.Map<List<SessionResultListViewModel>>(response.Itens);

            return Ok(result);
        }

    }
}
