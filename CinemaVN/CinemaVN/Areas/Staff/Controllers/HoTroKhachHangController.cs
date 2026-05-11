using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaVN.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CinemaVN.Areas.Staff.Controllers
{
    [Area("Staff")]
    public class HoTroKhachHangController : Controller
    {

        private readonly CinemaVNContext _db;
        public HoTroKhachHangController(CinemaVNContext db) => _db = db;

        public IActionResult Index() => View();
        [HttpGet]
        public async Task<IActionResult> HoanVe()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Home", new { area = "" });

            var staff = await _db.NguoiDungs.FindAsync(userId);
            var dsLichSuHoan = await _db.HoaDons
                .Include(h => h.MaNdNavigation)
                .Include(h => h.Ves).ThenInclude(v => v.MaScNavigation).ThenInclude(s => s.MaPhimNavigation)
                .Where(h => h.MaCn == staff.MaCn && h.Ves.Any(v => v.TrangThai == 4 || v.TrangThai == 5))
                .OrderByDescending(h => h.NgayLap)
                .Take(50)
                .ToListAsync();

            return View(dsLichSuHoan);
        }
        [HttpGet]
        public async Task<IActionResult> TimHoaDonHoanVe(int maHD)
        {
            var hd = await _db.HoaDons
                .Include(h => h.Ves).ThenInclude(v => v.MaScNavigation).ThenInclude(s => s.MaPhimNavigation)
                .Include(h => h.Ves).ThenInclude(v => v.MaGheNavigation)
                .FirstOrDefaultAsync(h => h.MaHd == maHD);

            if (hd == null) return Json(new { success = false, message = "Không tìm thấy hóa đơn!" });
            if (hd.TrangThai == 3) return Json(new { success = false, message = "Hóa đơn này đã được hoàn tiền rồi!" });

            var firstVe = hd.Ves.FirstOrDefault();
            if (firstVe == null) return Json(new { success = false, message = "Hóa đơn không có vé!" });

            return Json(new
            {
                success = true,
                maHD = hd.MaHd,
                ngayDat = hd.NgayLap?.ToString("dd/MM/yyyy HH:mm"),
                phim = firstVe.MaScNavigation.MaPhimNavigation.TenPhim,
                suat = firstVe.MaScNavigation.GioBd?.ToString(@"hh\:mm"),
                ngayChieu = firstVe.MaScNavigation.NgayChieu?.ToString("dd/MM/yyyy"),
                tongTien = hd.TongTien?.ToString("N0") + " đ"
            });
        }

        [HttpPost]
        public async Task<IActionResult> YeuCauHoanVe(int maHD)
        {
            var hd = await _db.HoaDons.Include(h => h.Ves).ThenInclude(v => v.MaScNavigation).FirstOrDefaultAsync(h => h.MaHd == maHD);
            if (hd == null) return Json(new { success = false, message = "Lỗi hệ thống!" });

            DateTime bayGio = DateTime.Now;
            var suatChieu = hd.Ves.First().MaScNavigation;
            DateTime thoiDiemChieu = suatChieu.NgayChieu.Value.Date.Add(suatChieu.GioBd.Value);

            if (hd.NgayLap?.Date != bayGio.Date)
                return Json(new { success = false, message = "Chỉ được hoàn vé trong ngày đã đặt!" });
            if ((thoiDiemChieu - bayGio).TotalHours < 1)
                return Json(new { success = false, message = "Đã quá hạn hoàn vé (Phải trước giờ chiếu 1h)!" });

            foreach (var v in hd.Ves) { v.TrangThai = 4; } 
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Đã gửi yêu cầu hoàn tiền cho Quản lý chi nhánh duyệt!" });
        }

    }
}