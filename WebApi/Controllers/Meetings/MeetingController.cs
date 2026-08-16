using AutoMapper;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using System.Text.Json;
using WebApi.Controllers.Constants;
using WebApi.ViewModels;

namespace WebApi.Controllers.Meetings
{
    public class MeetingController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IMeetingService _meetingService;
        private readonly IMapper _mapper;

        public MeetingController(HttpClient httpClient, IMeetingService meetingService, IMapper mapper)
        {
            httpClient.BaseAddress = new Uri(F1ApiURL.URL_API_F1);
            _httpClient            = httpClient;
            _meetingService        = meetingService;
            _mapper                = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTracksOfCurrentSeason()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync("meetings?year=" + DateTime.Now.Year.ToString());

                if (!response.IsSuccessStatusCode)
                    throw new Exception("Failed to retrieve tracks.");

                string tracks = await response.Content.ReadAsStringAsync();
                
                List<MeetingViewModel>? meetingViewModels = JsonSerializer.Deserialize<List<MeetingViewModel>>(tracks) ?? throw new Exception("Failed to deserialize tracks.");
                
                await _meetingService.InsertTracksOfCurrentYear(_mapper.Map<List<Meeting>>(meetingViewModels));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }
    }
}
