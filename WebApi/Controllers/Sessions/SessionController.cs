using Microsoft.AspNetCore.Mvc;
using WebApi.Controllers.Constants;
using WebApi.ViewModels;
using System.Text.Json;

namespace WebApi.Controllers.Sessions
{
    public class SessionController : Controller
    {
        private readonly HttpClient _httpClient;
        public SessionController(HttpClient httpClient)
        {
            httpClient.BaseAddress = new Uri(F1ApiURL.URL_API_F1);
            this._httpClient = httpClient;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTracksOfSeason()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync("meetings?year=" + DateTime.Now.Year.ToString());
                
                if (!response.IsSuccessStatusCode)
                    throw new Exception("Failed to retrieve tracks.");

                string tracks = await response.Content.ReadAsStringAsync();
                List<SessionViewModel>? sessionViewModels = JsonSerializer.Deserialize<List<SessionViewModel>>(tracks) ?? throw new Exception("Failed to deserialize tracks.");
                //TODO: Implementar a forma de mapear para uma lista de Session e chamar o Service.

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
            return Ok();
        }
    }
}
