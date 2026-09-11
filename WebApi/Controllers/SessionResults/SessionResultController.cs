using AutoMapper;
using Entities.Class;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Shared.Responses;
using WebApi.ViewModels;

namespace WebApi.Controllers.SessionResults
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionResultController : Controller
    {
        private readonly ISessionResultService _sessionResultService;
        private readonly IMapper _mapper;
        public SessionResultController(ISessionResultService sessionResultService, IMapper mapper)
        {
            _sessionResultService = sessionResultService;
            _mapper = mapper;
        }

        /// <summary>
        /// Get the result of session by session key.
        /// </summary>
        /// <param name="sessionKey"></param>
        /// <returns>
        /// 200 OK - Returns a list of SessionResultListViewModel objects representing the result of the session for the specified session key.
        /// 400 Bad Request - Returns an ErrorViewModel if the request is invalid or if there are no results for the specified session key.
        /// 404 Not Found - Returns an ErrorViewModel if the specified session key does not exist or if there are no results for the specified session key.
        /// </returns>
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
