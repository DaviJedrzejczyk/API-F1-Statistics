using AutoMapper;
using Entities.Class;
using Entities.Dtos;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using WebApi.ViewModels;

namespace WebApi.Controllers.Drivers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DriverController : Controller
    {
        private readonly IDriverService _driverService;
        private readonly IMapper _mapper;
        public DriverController(IDriverService driverService, IMapper mapper)
        {
            _driverService = driverService;
            _mapper = mapper;
        }

        /// <summary>
        /// Get all drivers of a session
        /// </summary>
        /// <param name="meetingKey">The weekend chosen</param>
        /// <param name="sessionKey">The session chosen</param>
        /// <returns>
        /// 200 OK - A list with all drivers of this session
        /// 400 Bad Request - Ocurred an error when trying to find all drivers
        /// 404 Not Found - The session and meeting chosen dosent find the drivers
        /// </returns>
        [HttpGet("drivers-session")]
        [ProducesResponseType(typeof(List<DriverListViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllDriversSession(int sessionKey)
        {
            var response = await _driverService.GetAllDriversSession(sessionKey);

            if(response.Exception != null || response.Itens == null)
                return BadRequest(new ErrorViewModel(400, response.Message));

            if (response.Itens.Count == 0)
                return NotFound(new ErrorViewModel(404, response.Message));

            return Ok(_mapper.Map<List<DriverListViewModel>>(response.Itens));
        }
    }
}
