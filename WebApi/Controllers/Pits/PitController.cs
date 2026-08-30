using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Pits
{
    [ApiController]
    [Route("api/[controller]")]
    public class PitController : Controller
    {
        public ActionResult Index()
        {
            return Ok();
        }
    }
}
