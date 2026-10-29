using Entities.Dtos;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Shared.Responses;
using WebApi.Config;
using WebApi.ViewModels;

namespace WebApi.Controllers.CarDatas
{
    /// <summary>
    /// API controller that retrieves car speed data for sessions.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CarDataController : Controller
    {
        private readonly ICarDataService _carDataService;
        private readonly IDriverService _driverService;
        /// <summary>
        /// Creates a new instance of <see cref="CarDataController"/>.
        /// </summary>
        /// <param name="carDataService">Service used to retrieve car speed data.</param>
        /// <param name="driverService">Driver service supplied to the controller.</param>
        public CarDataController(ICarDataService carDataService, IDriverService driverService)
        {
            _carDataService = carDataService;
            _driverService = driverService;
        }

        /// <summary>
        /// Retrieves the highest recorded speeds per driver for the specified session, sorted by speed.
        /// </summary>
        /// <param name="sessionKey">Identifier of the session, supplied in the query string.</param>
        /// <param name="minimumSpeed">Minimum speed in km/h used to filter car data, supplied in the query string.</param>
        /// <returns>A list of driver speeds, or an HTTP error result.</returns>
        /// <response code="200">A list of SessionDriverSpeedDTO objects.</response>
        /// <response code="400">ErrorViewModel when the service fails without a list or with an exception and a nonempty list; the message is generic when the service reports an exception.</response>
        /// <response code="404">ErrorViewModel when the service fails with an empty list. This check takes precedence over the exception check.</response>
        /// <response code="500">ErrorViewModel for unexpected server errors.</response>
        [HttpGet("high-speed")]
        [ProducesResponseType(typeof(List<SessionDriverSpeedDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> HighSpeedInSession(int sessionKey, int minimumSpeed)
        {
            DataResponse<SessionDriverSpeedDTO> response = await _carDataService.GetSortedHighSpeedsSession(sessionKey, minimumSpeed);

            if (!response.HasSuccess && response.Itens == null)
                return BadRequest(new ErrorViewModel(400, ApiErrorMessages.FromResponse(response)));

            if (!response.HasSuccess && response.Itens.Count <= 0)
                return NotFound(new ErrorViewModel(404, ApiErrorMessages.FromResponse(response)));

            if (!response.HasSuccess && response.Exception != null)
                return BadRequest(new ErrorViewModel(400, ApiErrorMessages.RequestFailed));

            return Ok(response.Itens);
        }
    }
}
