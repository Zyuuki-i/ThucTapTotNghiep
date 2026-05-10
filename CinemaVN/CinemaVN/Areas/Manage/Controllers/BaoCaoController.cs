using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaVN.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace CinemaVN.Areas.Manage.Controllers
{
    [Area("Manage")]
    public class BaoCaoController : Controller
    {
        private readonly CinemaVNContext _db;

        public BaoCaoController(CinemaVNContext db)
        {
            _db = db;
        }
        public class ThongKeNgay
        {
            public string Ngay { get; set; }
            public int SoVe { get; set; }
            public int SoDichVu { get; set; }
            public decimal DoanhThu { get; set; }
        }

        public IActionResult Index(DateTime? fromDate, DateTime? toDate)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var user = _db.NguoiDungs.Include(u => u.MaCnNavigation).FirstOrDefault(u => u.MaNd == userId);
            if (user == null || user.MaCn == null) return RedirectToAction("Login", "Home", new { area = "" });

            ViewBag.TenChiNhanh = user.MaCnNavigation.TenCn;
            DateTime start = fromDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime end = toDate ?? DateTime.Now.Date;
            DateTime endOfDay = end.AddDays(1).AddTicks(-1);

            var hoaDons = _db.HoaDons
                .Include(h => h.Ves)
                .Include(h => h.ChiTietDichVus)
                .Where(h => h.MaCn == user.MaCn && h.NgayLap >= start && h.NgayLap <= endOfDay)
                .ToList();
            ViewBag.TongDoanhThu = hoaDons.Sum(h => h.TongTien ?? 0);
            ViewBag.TongVe = hoaDons.Sum(h => h.Ves.Count);
            ViewBag.TongDichVu = hoaDons.Sum(h => h.ChiTietDichVus.Sum(c => c.SoLuong));
            ViewBag.TongDonHang = hoaDons.Count;
            var dataTheoNgay = hoaDons
                .GroupBy(h => h.NgayLap.Value.Date)
                .Select(g => new ThongKeNgay
                {
                    Ngay = g.Key.ToString("dd/MM/yyyy"),
                    SoVe = g.Sum(h => h.Ves.Count),
                    SoDichVu = g.Sum(h => h.ChiTietDichVus.Sum(c => c.SoLuong)),

                    DoanhThu = g.Sum(h => h.TongTien ?? 0)
                })
                .OrderBy(x => DateTime.ParseExact(x.Ngay, "dd/MM/yyyy", null))
                .ToList();

            ViewBag.ChartLabels = dataTheoNgay.Select(x => x.Ngay).ToList();
            ViewBag.ChartData = dataTheoNgay.Select(x => x.DoanhThu).ToList();
            ViewBag.FromDate = start.ToString("yyyy-MM-dd");
            ViewBag.ToDate = end.ToString("yyyy-MM-dd");

            return View(dataTheoNgay);
        }
    }
}