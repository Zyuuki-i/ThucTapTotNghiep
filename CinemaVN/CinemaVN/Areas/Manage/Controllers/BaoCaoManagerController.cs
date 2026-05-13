using CinemaVN.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaVN.Areas.Manage.Controllers
{
    [Area("Manage")]
    public class BaoCaoManagerController : Controller
    {
        private readonly CinemaVNContext _db;

        public BaoCaoManagerController(CinemaVNContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await _db.NguoiDungs
                .Include(x => x.MaCnNavigation)
                .FirstOrDefaultAsync(x => x.MaNd == userId.Value);

            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.TenChiNhanh = user.MaCnNavigation?.TenCn ?? "Chi nhánh";

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetData(DateTime? tuNgay, DateTime? denNgay)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return Json(new { });
            }

            var user = await _db.NguoiDungs
                .Include(x => x.MaCnNavigation)
                .FirstOrDefaultAsync(x => x.MaNd == userId.Value);

            if (user == null || string.IsNullOrEmpty(user.MaCn))
            {
                return Json(new { });
            }

            tuNgay ??= DateTime.Today.AddDays(-30);
            denNgay ??= DateTime.Today;

            var branchId = user.MaCn;

            var hoaDons = await _db.HoaDons
                .Include(h => h.Ves)
                    .ThenInclude(v => v.MaScNavigation)
                        .ThenInclude(sc => sc.MaPhimNavigation)
                .Include(h => h.ChiTietDichVus)
                .Where(h =>
                    h.MaCn == branchId &&
                    h.NgayLap.HasValue &&
                    h.NgayLap.Value.Date >= tuNgay.Value.Date &&
                    h.NgayLap.Value.Date <= denNgay.Value.Date)
                .ToListAsync();

            var tongDoanhThu = hoaDons.Sum(h => h.TongTien ?? 0);

            var doanhThuVe = hoaDons.Sum(h =>
                h.Ves.Sum(v => v.Gia ?? 0));

            var doanhThuFB = await _db.ChiTietDichVus
                .Where(x =>
                    x.MaHdNavigation.MaCn == branchId &&
                    x.MaHdNavigation.NgayLap >= tuNgay &&
                    x.MaHdNavigation.NgayLap <= denNgay)
                .SumAsync(x => (decimal?)(x.DonGia * x.SoLuong)) ?? 0;

            var dailyRevenue = hoaDons
                .GroupBy(h => h.NgayLap!.Value.Date)
                .Select(g => new
                {
                    day = g.Key.ToString("dd/MM"),
                    revenue = g.Sum(x => x.TongTien ?? 0)
                })
                .OrderBy(x => x.day)
                .ToList();

            var occupancy = hoaDons
                .SelectMany(h => h.Ves)
                .GroupBy(v => v.MaSc)
                .Select(g => new
                {
                    suat = "SC " + g.Key,
                    rate = g.Count()
                })
                .OrderByDescending(x => x.rate)
                .Take(5)
                .ToList();

            var staffRefunds = await _db.HoaDons
                .Include(h => h.MaNdNavigation)
                .Where(h =>
                    h.MaCn == branchId &&
                    h.NgayLap >= tuNgay &&
                    h.NgayLap <= denNgay)
                .GroupBy(h => h.MaNdNavigation!.HoTen)
                .Select(g => new
                {
                    name = g.Key,
                    count = g.Count()
                })
                .OrderByDescending(x => x.count)
                .Take(5)
                .ToListAsync();

            var localMovies = hoaDons
                .SelectMany(h => h.Ves)
                .Where(v => v.MaScNavigation?.MaPhimNavigation != null)
                .GroupBy(v => v.MaScNavigation.MaPhimNavigation!.TenPhim)
                .Select(g => new
                {
                    item = g.Key,
                    qty = g.Count()
                })
                .OrderByDescending(x => x.qty)
                .Take(5)
                .ToList();

            return Json(new
            {
                tongDoanhThu,
                doanhThuVe,
                doanhThuFB,
                dailyRevenue,
                occupancy,
                staffRefunds,
                localMovies
            });
        }
        public async Task<IActionResult> ExportReport(DateTime? tuNgay, DateTime? denNgay)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await _db.NguoiDungs
                .Include(x => x.MaCnNavigation)
                .FirstOrDefaultAsync(x => x.MaNd == userId.Value);

            if (user == null || string.IsNullOrEmpty(user.MaCn))
            {
                return RedirectToAction("Index", "Home");
            }

            tuNgay ??= DateTime.Today.AddDays(-30);
            denNgay ??= DateTime.Today;

            var branchId = user.MaCn;

            var hoaDons = await _db.HoaDons
                .Include(h => h.Ves)
                    .ThenInclude(v => v.MaScNavigation)
                        .ThenInclude(sc => sc.MaPhimNavigation)
                .Include(h => h.ChiTietDichVus)
                .Where(h =>
                    h.MaCn == branchId &&
                    h.NgayLap.HasValue &&
                    h.NgayLap.Value.Date >= tuNgay.Value.Date &&
                    h.NgayLap.Value.Date <= denNgay.Value.Date)
                .ToListAsync();

            var tongDoanhThu = hoaDons.Sum(h => h.TongTien ?? 0);

            var doanhThuVe = hoaDons.Sum(h =>
                h.Ves.Sum(v => v.Gia ?? 0));

            var doanhThuFB = await _db.ChiTietDichVus
                .Where(x =>
                    x.MaHdNavigation.MaCn == branchId &&
                    x.MaHdNavigation.NgayLap >= tuNgay &&
                    x.MaHdNavigation.NgayLap <= denNgay)
                .SumAsync(x => (decimal?)(x.DonGia * x.SoLuong)) ?? 0;

            var dailyRevenue = hoaDons
                .GroupBy(h => h.NgayLap!.Value.Date)
                .Select(g => new
                {
                    day = g.Key.ToString("dd/MM"),
                    revenue = g.Sum(x => x.TongTien ?? 0)
                })
                .OrderBy(x => x.day)
                .ToList();

            var topMovies = hoaDons
                .SelectMany(h => h.Ves)
                .Where(v => v.MaScNavigation?.MaPhimNavigation != null)
                .GroupBy(v => v.MaScNavigation.MaPhimNavigation!.TenPhim)
                .Select(g => new
                {
                    item = g.Key,
                    qty = g.Count()
                })
                .OrderByDescending(x => x.qty)
                .Take(5)
                .ToList();

            var staffPerformance = await _db.HoaDons
                .Include(h => h.MaNdNavigation)
                .Where(h =>
                    h.MaCn == branchId &&
                    h.NgayLap >= tuNgay &&
                    h.NgayLap <= denNgay)
                .GroupBy(h => h.MaNdNavigation!.HoTen)
                .Select(g => new
                {
                    name = g.Key,
                    count = g.Count()
                })
                .OrderByDescending(x => x.count)
                .Take(5)
                .ToListAsync();

            ViewBag.TenChiNhanh = user.MaCnNavigation?.TenCn;
            ViewBag.ManagerName = user.HoTen;
            ViewBag.TuNgay = tuNgay.Value.ToString("dd/MM/yyyy");
            ViewBag.DenNgay = denNgay.Value.ToString("dd/MM/yyyy");

            ViewBag.TongDoanhThu = tongDoanhThu;
            ViewBag.DoanhThuVe = doanhThuVe;
            ViewBag.DoanhThuFB = doanhThuFB;

            ViewBag.TopMovies = topMovies;
            ViewBag.StaffPerformance = staffPerformance;

            ViewBag.RevenueLabels = JsonSerializer.Serialize(
                dailyRevenue.Select(x => x.day)
            );

            ViewBag.RevenueData = JsonSerializer.Serialize(
                dailyRevenue.Select(x => x.revenue)
            );

            ViewBag.MovieLabels = JsonSerializer.Serialize(
                topMovies.Select(x => x.item)
            );

            ViewBag.MovieData = JsonSerializer.Serialize(
                topMovies.Select(x => x.qty)
            );

            return View();
        }
    }
}