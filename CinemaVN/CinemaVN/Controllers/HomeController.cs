using System.Diagnostics;
using CinemaVN.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaVN.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private CinemaVNContext db = new CinemaVNContext();

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewBag.Banners = db.Banners.Where(b => b.TrangThai == true).ToList();
            ViewBag.PhimDangChieu = db.Phims
                .Where(p => p.TrangThai == 1)
                .Take(8)
                .ToList();

            ViewBag.PhimSapChieu = db.Phims
                .Where(p => p.TrangThai == 0)
                .Take(4)
                .ToList();

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
