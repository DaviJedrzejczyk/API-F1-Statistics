using AutoMapper;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Shared.Responses;
using WebApi.ViewModels.ErrorsViews;
using WebApi.ViewModels.SessionResultsViews;

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

        [HttpGet("fastest-lap")]
        public async Task<IActionResult> GetFastestLapsBySession(int sessionKey)
        {
            DataResponse<SessionResult> response = await _sessionResultService.GetSessionResultBySessionKeyApi(sessionKey);

            if (!response.HasSuccess)
            {
                if (response.Itens == null) return BadRequest(new ErrorViewModel(404, response.Message));
                else if (response.Itens.Count <= 0) return NotFound(new ErrorViewModel(400, response.Message));
            }

            return Ok(_mapper.Map<List<SessionResultListViewModel>>(response.Itens));
        }

    }
}
