using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Overtakes
{
    [ApiController]
    [Route("api/[controller]")]
    public class OvertakeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
