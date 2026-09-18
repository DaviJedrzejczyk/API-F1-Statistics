using AutoMapper;
using Entities.Dtos;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using WebApi.ViewModels;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LapsController : Controller
    {
        private readonly ILapFastSectorService _lapFastSectorService;
        private readonly ILapService _lapService;
        private readonly IMapper _mapper;

        public LapsController(ILapFastSectorService lapFastSectorService, IMapper mapper, ILapService lapService)
        {
            _lapFastSectorService = lapFastSectorService;
            _mapper = mapper;
            _lapService = lapService;
        }


        /// <summary>
        /// Get the fastest lap of a session by session key.
        /// </summary>
        /// <param name="sessionKey">The key of the session.</param>
        /// <returns>The fastest lap of the session.</returns>
        [HttpGet("fast_lap_session")]
        [ProducesResponseType(typeof(ErrorViewModel), 400)]
        [ProducesResponseType(typeof(ErrorViewModel), 404)]
        [ProducesResponseType(typeof(LapFastLapDto), 200)]
        public async Task<IActionResult> GetFastLapSession(int sessionKey)
        {
            var result = await _lapService.GetFastLapOfRaceBySessionKey(sessionKey);

            if (result.HasSuccess) return Ok(result.Item);

            if (!result.HasSuccess || result.Item == null)
                return NotFound(new ErrorViewModel(404, result.Message));

            return BadRequest(new ErrorViewModel(400, result.Message));
        }

        /// <summary>
        /// Get the fastest sectors of a session by session key. This endpoint just been call when you have this session in Database, otherwise you will get a 404 error. 
        /// If you want to get the fastest sectors of a session that is not in Database, you need to call the endpoint "/api/SessionResult/result" 
        /// and after this do you can call this endpoint.
        /// </summary>
        /// <param name="sessionKey">The key of the session.</param>
        /// <returns>The fastest sectors of the session.</returns>
        [HttpGet("fast_sectors_session")]
        [ProducesResponseType(typeof(ErrorViewModel), 400)]
        [ProducesResponseType(typeof(ErrorViewModel), 404)]
        [ProducesResponseType(typeof(List<LapFastSectorDto>), 200)]
        public async Task<IActionResult> GetFastSectorsSession(int sessionKey)
        {
            var result = await _lapFastSectorService.GetFastSectorsOfSession(sessionKey);
            if (result.HasSuccess) return Ok(_mapper.Map<List<LapFastSectorDto>>(result.Itens));

            if (!result.HasSuccess || result.Itens == null)
                return NotFound(new ErrorViewModel(404, result.Message));

            return BadRequest(new ErrorViewModel(400, result.Message));
        }
    }
}
