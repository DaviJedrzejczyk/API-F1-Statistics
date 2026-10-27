using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    /// <summary>
    /// MVC controller for the home view.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Creates the view result for the home page.
        /// </summary>
        /// <returns>A ViewResult for the default Index view.</returns>
        /// <remarks>This MVC action has no attribute route, and the application does not currently map a conventional MVC route.</remarks>
        public IActionResult Index()
        {
            return View();
        }
    }
}
