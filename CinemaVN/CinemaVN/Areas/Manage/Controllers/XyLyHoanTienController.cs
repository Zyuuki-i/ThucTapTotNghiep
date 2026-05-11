using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaVN.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CinemaVN.Areas.Manage.Controllers
{
    [Area("Manage")]
    public class XyLyHoanTienController : Controller
    {
        private readonly CinemaVNContext _db;
        public XyLyHoanTienController(CinemaVNContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var manager = await _db.NguoiDungs.FindAsync(userId);
            if (manager == null) return RedirectToAction("Login", "Home", new { area = "" });

            var dsChoDuyet = await _db.HoaDons
                .Include(h => h.MaNdNavigation)
                .Include(h => h.MaKmNavigation)
                .Include(h => h.Ves).ThenInclude(v => v.MaScNavigation).ThenInclude(s => s.MaPhimNavigation)
                .Where(h => h.MaCn == manager.MaCn && h.Ves.Any(v => v.TrangThai == 4))
                .OrderByDescending(h => h.NgayLap)
                .ToListAsync();

            return View(dsChoDuyet);
        }

        [HttpPost]
        public async Task<IActionResult> DuyetHoanTien(int maHD)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var hd = await _db.HoaDons
                    .Include(h => h.Ves)
                    .Include(h => h.MaKmNavigation)
                    .Include(h => h.MaNdNavigation)
                    .FirstOrDefaultAsync(h => h.MaHd == maHD);

                if (hd == null) return Json(new { success = false, message = "Không tìm thấy hóa đơn!" });
                hd.TrangThai = 3; 
                foreach (var v in hd.Ves)
                {
                    v.TrangThai = 5;
                }
                if (hd.MaNdNavigation != null)
                {
                    var kh = hd.MaNdNavigation;
                    if (hd.MaKmNavigation != null && (hd.MaKmNavigation.SoDiem ?? 0) > 0)
                    {
                        kh.DiemHoiVien += hd.MaKmNavigation.SoDiem;
                    }

                    int diemDaCong = (int)Math.Floor((double)(hd.TongTien / 1000));
                    kh.DiemHoiVien -= diemDaCong;

                    if (kh.DiemHoiVien < 0) kh.DiemHoiVien = 0;
                    _db.NguoiDungs.Update(kh);
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return Json(new { success = true, message = "Đã phê duyệt hoàn tiền và cập nhật điểm khách hàng!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Lỗi xử lý: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> TuChoiHoanTien(int maHD)
        {
            var hd = await _db.HoaDons.Include(h => h.Ves).FirstOrDefaultAsync(h => h.MaHd == maHD);
            if (hd == null) return Json(new { success = false });

            foreach (var v in hd.Ves) { v.TrangThai = 1; }
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Đã từ chối yêu cầu hoàn tiền." });
        }
        public async Task<IActionResult> LichSuHoan()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var manager = await _db.NguoiDungs.FindAsync(userId);
            if (manager == null) return RedirectToAction("Login", "Home", new { area = "" });
            var dsLichSu = await _db.HoaDons
                .Include(h => h.MaNdNavigation)
                .Include(h => h.MaKmNavigation)
                .Include(h => h.Ves).ThenInclude(v => v.MaScNavigation).ThenInclude(s => s.MaPhimNavigation)
                .Where(h => h.MaCn == manager.MaCn && h.TrangThai == 3)
                .OrderByDescending(h => h.NgayLap)
                .ToListAsync();

            return View(dsLichSu);
        }
    }
}