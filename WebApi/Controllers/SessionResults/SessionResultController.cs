using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.SessionResults
{
    public class SessionResultController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
