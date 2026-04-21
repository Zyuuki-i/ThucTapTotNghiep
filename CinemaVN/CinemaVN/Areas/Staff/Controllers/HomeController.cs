using Microsoft.AspNetCore.Mvc;

namespace CinemaVN.Areas.Staff.Controllers
{
    [Area("Staff")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
