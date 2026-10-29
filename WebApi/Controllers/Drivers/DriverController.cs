using AutoMapper;
using Entities.Class;
using Entities.Dtos;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using WebApi.Config;
using WebApi.ViewModels;

namespace WebApi.Controllers.Drivers
{
    /// <summary>
    /// API controller that retrieves drivers for sessions.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DriverController : Controller
    {
        private readonly IDriverService _driverService;
        private readonly IMapper _mapper;
        /// <summary>
        /// Creates a new instance of <see cref="DriverController"/>.
        /// </summary>
        /// <param name="driverService">Service used to retrieve drivers.</param>
        /// <param name="mapper">AutoMapper instance for mapping drivers to view models.</param>
        public DriverController(IDriverService driverService, IMapper mapper)
        {
            _driverService = driverService;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves all drivers for the specified session.
        /// </summary>
        /// <param name="sessionKey">Identifier of the session, supplied in the query string.</param>
        /// <returns>A list of driver view models, or an HTTP error result.</returns>
        /// <response code="200">A list of DriverListViewModel objects.</response>
        /// <response code="400">ErrorViewModel when the service returns an exception or a null list, or for invalid input (model binding failure); the message is generic when the service reports an exception.</response>
        /// <response code="404">ErrorViewModel when the service returns an empty list without an exception.</response>
        /// <response code="500">ErrorViewModel for unexpected server errors.</response>
        [HttpGet("drivers-session")]
        [ProducesResponseType(typeof(List<DriverListViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllDriversSession(int sessionKey)
        {
            var response = await _driverService.GetAllDriversSession(sessionKey);

            if(response.Exception != null || response.Itens == null)
                return BadRequest(new ErrorViewModel(400, ApiErrorMessages.FromResponse(response)));

            if (response.Itens.Count == 0)
                return NotFound(new ErrorViewModel(404, ApiErrorMessages.FromResponse(response)));

            return Ok(_mapper.Map<List<DriverListViewModel>>(response.Itens));
        }
    }
}
