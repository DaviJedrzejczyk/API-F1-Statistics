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
        private readonly ISessionService _sessionService;

        public SessionController(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        [HttpGet("InsertSessions")]
        public async Task<IActionResult> InsertSessionsInDataBase(int meetingKey)
        {
            var response = await _sessionService.InsertSessions(meetingKey);

            if (!response.HasSuccess)
                return BadRequest(response.Message);

            return Ok(response);
        }

        [HttpGet("HighSpeed")]
        public Task<IActionResult> HighSpeedInSession(int sessionKey, int meetingKey)
        {
            // Implementation for getting high-speed data in a session
            throw new NotImplementedException();
        }
    }
}
