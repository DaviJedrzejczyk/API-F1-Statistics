using Entities.Dtos;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using WebApi.ViewModels.ErrorsViews;
using WebApi.ViewModels.SuccessViews;

namespace WebApi.Controllers.Drivers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DriverController : Controller
    {
        private readonly IDriverService _driverService;
        public DriverController(IDriverService driverService)
        {
            _driverService = driverService;
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
                return BadRequest(new ErrorViewModel(404, response.Message));

            return Ok(new SuccessViewModel(200, response.Message));
        }
    }
}
