using AutoMapper;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Shared.Responses;
using System.Text.Json;
using WebApi.Controllers.Constants;
using WebApi.ViewModels;

namespace WebApi.Controllers.Meetings
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeetingController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IMeetingService _meetingService;
        private readonly IMapper _mapper;

        public MeetingController(HttpClient httpClient, IMeetingService meetingService, IMapper mapper)
        {
            httpClient.BaseAddress = new Uri(F1ApiURL.URL_API_F1);
            _httpClient = httpClient;
            _meetingService = meetingService;
            _mapper = mapper;
        }

        [HttpGet("insertAllTracksOfCurrentSeason")]
        public async Task<IActionResult> InsertAllTracksOfCurrentSeason()
        {
            try
            {
                HttpResponseMessage responseMsg = await _httpClient.GetAsync("meetings?year=" + DateTime.Now.Year.ToString());

                if (!responseMsg.IsSuccessStatusCode)
                    throw new Exception("Failed to retrieve tracks.");

                string tracks = await responseMsg.Content.ReadAsStringAsync();

                List<MeetingViewModel>? meetingViewModels = JsonSerializer.Deserialize<List<MeetingViewModel>>(tracks) ?? throw new Exception("Failed to deserialize tracks.");

                Response response = await _meetingService.InsertTracksOfCurrentYear(_mapper.Map<List<Meeting>>(meetingViewModels));

                if (!response.HasSuccess)
                    return BadRequest(response.Message);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("getByKey")]
        public async Task<IActionResult> GetMeetingByKey(int meetingKey)
        {
            try
            {
                SingleResponse<Meeting> response = await _meetingService.GetMeetingByKey(meetingKey);

                if (response.Item == null)
                    return NotFound(response.Message);

                return Ok(_mapper.Map<MeetingViewModel>(response.Item));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("AllTracksOfCurrentYear")]
        public async Task<IActionResult> GetAllTracksOfCurrentYear(int year)
        {
            try
            {
                DataResponse<Meeting> response = await _meetingService.GetAllTracksOfCurrentYear(year);

                if (!response.HasSuccess)
                    return BadRequest(response);

                return Ok(_mapper.Map<List<MeetingViewModel>>(response.Itens));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
