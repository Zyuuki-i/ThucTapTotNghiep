using System.Diagnostics;
using CinemaVN.DatModels;
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


        public IActionResult Search(string keyword)
        {
            var model = new SearchResultViewModel();
            model.Keyword = keyword;

            if (!string.IsNullOrEmpty(keyword))
            {

                string key = keyword.ToLower();

                model.DanhSachPhim = db.Phims
                    .Where(p => p.TenPhim.ToLower().Contains(key))
                    .ToList();

                model.DanhSachRap = db.ChiNhanhs
                    .Where(r => r.TenCn.ToLower().Contains(key) || r.DiaChi.ToLower().Contains(key) || r.MaCn.ToLower().Contains(key))
                    .ToList();
            }
            model.tongKQ = model.DanhSachRap.Count + model.DanhSachPhim.Count;
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
