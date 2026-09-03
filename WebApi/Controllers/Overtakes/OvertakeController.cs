using AutoMapper;
using Entities.Dtos;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using WebApi.ViewModels.ErrorsViews;
using WebApi.ViewModels.OvertakeViews;
using WebApi.ViewModels.SuccessViews;

namespace WebApi.Controllers.Overtakes
{
    [ApiController]
    [Route("api/[controller]")]
    public class OvertakeController : Controller
    {
        private readonly IOvertakeService _overtakeService;
        private readonly IMapper _mapper;

        public OvertakeController(IOvertakeService overtakeService, IMapper mapper)
        {
            _overtakeService = overtakeService;
            _mapper = mapper;
        }

        /// <summary>
        /// Get overtakes by session key
        /// </summary>
        /// <param name="sessionKey"></param>
        /// <returns>
        /// 200 OK - Returns a list of overtakes for the specified session key
        /// 400 Bad Request - Returns an error message if the request is invalid
        /// </returns>
        [HttpGet("get-overtakes-session/{sessionKey}")]
        [ProducesResponseType(typeof(OvertakeCountViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetOvertakesSession(int sessionKey)
        {
            var response = await _overtakeService.GetOvertakesSessionApi(sessionKey);
            if (!response.HasSuccess) return BadRequest(new ErrorViewModel(400, response.Message));

            return Ok(new OvertakeCountViewModel(response.Itens.Count));
        }
    }
}
