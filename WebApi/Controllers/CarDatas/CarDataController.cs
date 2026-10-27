using Entities.Dtos;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Shared.Responses;
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
        /// <response code="400">ProblemDetails when the service fails with an exception and a nonempty list; ValidationProblemDetails for invalid input.</response>
        /// <response code="404">ProblemDetails when the service fails with an empty list. This check takes precedence over the exception check.</response>
        /// <remarks>The action returns bodyless error results; the API controller error filter supplies ProblemDetails.</remarks>
        [HttpGet("high-speed")]
        [ProducesResponseType(typeof(List<SessionDriverSpeedDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> HighSpeedInSession(int sessionKey, int minimumSpeed)
        {
            DataResponse<SessionDriverSpeedDTO> response = await _carDataService.GetSortedHighSpeedsSession(sessionKey, minimumSpeed);

            if (!response.HasSuccess && response.Itens.Count <= 0)
                return NotFound();

            if (!response.HasSuccess && response.Exception != null)
                return BadRequest();

            return Ok(response.Itens);
        }
    }
}
