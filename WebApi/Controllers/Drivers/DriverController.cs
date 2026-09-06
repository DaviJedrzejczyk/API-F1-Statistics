using AutoMapper;
using Entities;
using Entities.Dtos.DriverDTOs;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using WebApi.ViewModels.DriversViews;
using WebApi.ViewModels.ErrorsViews;
using WebApi.ViewModels.SuccessViews;

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
        /// Inserts all drivers of current session selected
        /// </summary>
        /// <param name="model">View model containing the meeting key and session key to insert drivers for.</param>
        /// <returns>
        /// 200 OK with a Response object when insertion succeeds.
        /// 400 BadRequest when model is invalid or insertion failed.
        /// </returns>
        [HttpPost("insert-drivers-session")]
        [ProducesResponseType(typeof(SuccessViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertDrivers(DriverInsertDTO model)
        {
            var response = await _driverService.InsertDrivers(model);

            if (!response.HasSuccess)
                return BadRequest(new ErrorViewModel(400, response.Message));

            return Ok(new SuccessViewModel(200, response.Message));
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
