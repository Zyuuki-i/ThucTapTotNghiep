using CinemaVN.Models;
using Microsoft.AspNetCore.Mvc;

namespace CinemaVN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class KhuyenMaiController : Controller
    {
        private readonly CinemaVNContext db = new();
        public IActionResult Index()
        {
            return View();
        }
    }
}
