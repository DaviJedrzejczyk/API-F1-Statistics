using Entities.Dtos;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Shared.Responses;
using WebApi.ViewModels.ErrorsViews;

namespace WebApi.Controllers.CarDatas
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarDataController : Controller
    {
        private readonly ICarDataService _carDataService;
        private readonly IDriverService _driverService;
        public CarDataController(ICarDataService carDataService, IDriverService driverService)
        {
            _carDataService = carDataService;
            _driverService = driverService;
        }

        /// <summary>
        /// Retrieves high-speed data for a specified session within a meeting.
        /// </summary>
        /// <param name="sessionKey">Identifier of the session.</param>
        /// <param name="meetingKey">Identifier of the meeting the session belongs to.</param>
        /// <returns>
        /// 200 OK with the high-speed data when implemented.
        /// </returns>
        [HttpGet("high-speed")]
        [ProducesResponseType(typeof(SessionDriverSpeedDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> HighSpeedInSession(int sessionKey, int meetingKey, int minimumSpeed)
        {
            DataResponse<SessionDriverSpeedDTO> response = await _carDataService.GetSortedHighSpeedsSession(sessionKey, meetingKey, minimumSpeed);

            if (!response.HasSuccess && response.Itens.Count <= 0)
                return NotFound();

            if (!response.HasSuccess && response.Exception != null)
                return BadRequest();

            return Ok(response.Itens);
        }
    }
}
