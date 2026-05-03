using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaVN.Models; // Thay bằng namespace Models của bạn
using System.Text.Json;

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

        // Helper lấy User hiện tại
        private NguoiDung GetCurrentUser()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            return _db.NguoiDungs.Include(u => u.MaCnNavigation).FirstOrDefault(u => u.MaNd == userId);
        }

        public IActionResult Index()
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null) return RedirectToAction("Login", "Home", new { area = "" });

            ViewBag.TenChiNhanh = currentUser.MaCnNavigation?.TenCn;

            // 1. THIẾT LẬP THỜI GIAN (Lấy tháng hiện tại)
            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            // 2. LẤY DỮ LIỆU HÓA ĐƠN CỦA CHI NHÁNH TRONG THÁNG (Giả định bạn có bảng HoaDons)
            var hoaDonsTrongThang = _db.HoaDons
                .Where(hd => hd.MaCn == currentUser.MaCn && hd.NgayLap >= firstDayOfMonth && hd.NgayLap <= lastDayOfMonth)
                .ToList();
=
            ViewBag.TongDoanhThu = hoaDonsTrongThang.Sum(hd => hd.TongTien) ?? 0;
            ViewBag.TongDonHang = hoaDonsTrongThang.Count;

            // Đếm số nhân sự đang hoạt động tại chi nhánh
            ViewBag.TongNhanSu = _db.NguoiDungs.Count(n => n.MaCn == currentUser.MaCn && n.TrangThai == true);
            var sevenDaysAgo = today.AddDays(-6);
            var doanhThu7Ngay = _db.HoaDons
                .Where(hd => hd.MaCn == currentUser.MaCn && hd.NgayLap >= sevenDaysAgo && hd.NgayLap <= today)
                .GroupBy(hd => hd.NgayLap.Value.Date)
                .Select(g => new { Ngay = g.Key.ToString("dd/MM"), DoanhThu = g.Sum(hd => hd.TongTien) })
                .ToList();

            // Đảm bảo đủ 7 ngày kể cả ngày không có doanh thu
            var chartLabels = new List<string>();
            var chartData = new List<decimal>();
            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                chartLabels.Add(date.ToString("dd/MM"));
                var dtDay = doanhThu7Ngay.FirstOrDefault(d => d.Ngay == date.ToString("dd/MM"));
                chartData.Add(dtDay?.DoanhThu ?? 0);
            }

            ViewBag.ChartLabels = JsonSerializer.Serialize(chartLabels);
            ViewBag.ChartData = JsonSerializer.Serialize(chartData);


            //var topDichVu = _db.ChiTietDichVus
            //    .Include(ct => ct.MaHDNavigation)
            //    .Include(ct => ct.MaDvNavigation)
            //    .Where(ct => ct.MaHDNavigation.MaCn == currentUser.MaCn
            //              && ct.MaHDNavigation.NgayLap >= firstDayOfMonth
            //              && ct.MaHDNavigation.NgayLap <= lastDayOfMonth)
            //    .GroupBy(ct => new { ct.MaDv, ct.MaDvNavigation.TenDv, ct.MaDvNavigation.HinhAnh })
            //    .Select(g => new
            //    {
            //        MaDV = g.Key.MaDv,
            //        TenDV = g.Key.TenDv,
            //        HinhAnh = g.Key.HinhAnh,
            //        TongSoLuong = g.Sum(ct => ct.SoLuong),
            //        TongTienMangLai = g.Sum(ct => ct.ThanhTien)
            //    })
            //    .OrderByDescending(x => x.TongSoLuong)
            //    .Take(5)
            //    .ToList();

            //ViewBag.TopDichVu = topDichVu;

            return View();
        }
    }
}