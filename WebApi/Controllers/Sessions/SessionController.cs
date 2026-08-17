using AutoMapper;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using System.Text.Json;
using WebApi.Controllers.Constants;
using WebApi.ViewModels;

namespace WebApi.Controllers.Sessions
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ISessionService _sessionService;
        private readonly IMapper _mapper;

        public SessionController(HttpClient httpClient, ISessionService sessionService, IMapper mapper)
        {
            httpClient.BaseAddress = new Uri(F1ApiURL.URL_API_F1);
            _httpClient = httpClient;
            _sessionService = sessionService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> InsertSessionsInDataBase(int meetingKey)
        {
            HttpResponseMessage responseMsg = await _httpClient.GetAsync("sessions?meeting_key=" + meetingKey);

            if (!responseMsg.IsSuccessStatusCode)
                throw new Exception("Failed to retrieve sessions.");

            string sessions = await responseMsg.Content.ReadAsStringAsync();

            List<SessionViewModel>? sessionViewModels = JsonSerializer.Deserialize<List<SessionViewModel>>(sessions) ?? throw new Exception("Failed to deserialize sessions.");

            var response = await _sessionService.InsertSessions(_mapper.Map<List<Session>>(sessionViewModels));

            if (!response.HasSuccess)
                return BadRequest(response.Message);

            return Ok(response);
        }

        [HttpGet]
        public Task<IActionResult> HighSpeedInSession(int sessionKey, int meetingKey)
        {
            // Implementation for getting high-speed data in a session
            throw new NotImplementedException();
        }
    }
}
