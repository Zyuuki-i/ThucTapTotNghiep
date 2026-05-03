using CinemaVN.Models;
using Microsoft.AspNetCore.Mvc;

namespace CinemaVN.Controllers
{
    public class PhimController : Controller
    {
        private CinemaVNContext db = new CinemaVNContext();
        public IActionResult Index(string? tab = "dang-chieu")
        {
            var query = db.Phims.Where(p => p.TrangThai == 0 || p.TrangThai == 1).AsQueryable();

            if (tab == "dang-chieu")
            {
                query = query.Where(p => p.TrangThai == 1);
            }
            else if (tab == "sap-chieu")
            {
                query = query.Where(p => p.TrangThai == 0);
            }

            ViewBag.CurrentTab = tab;

            var danhSachPhim = query.OrderByDescending(p => p.NgayChieu).ToList();

            return View(danhSachPhim);
        }

        public IActionResult ChiTiet(int id)
        {
            Phim? phim = db.Phims.FirstOrDefault(t=>t.MaPhim== id);
            if (phim == null) return RedirectToAction("Index");
            ViewBag.SuatChieus = db.SuatChieus
                    .Where(s => s.MaPhim == id && s.NgayChieu >= DateTime.Today)
                    .OrderBy(s => s.NgayChieu)
                    .ThenBy(s => s.GioBd)
                    .ToList();
            return View(phim);
        }
    }


}
