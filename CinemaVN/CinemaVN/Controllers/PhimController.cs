using Microsoft.AspNetCore.Mvc;

namespace CinemaVN.Controllers
{
    public class PhimController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
