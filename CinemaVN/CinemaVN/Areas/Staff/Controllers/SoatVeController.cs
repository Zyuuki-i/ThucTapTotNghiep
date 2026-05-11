using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaVN.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CinemaVN.Areas.Staff.Controllers
{
    [Area("Staff")]
    public class SoatVeController : Controller
    {
        private readonly CinemaVNContext _db;

        public SoatVeController(CinemaVNContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Home", new { area = "" });
            var staff = _db.NguoiDungs.Include(u => u.MaCnNavigation).FirstOrDefault(u => u.MaNd == userId);
            if (staff == null || staff.MaCn == null) return RedirectToAction("Login", "Home", new { area = "" });

            ViewBag.TenNhanVien = staff.HoTen;
            ViewBag.TenChiNhanh = staff.MaCnNavigation?.TenCn;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> KiemTraVe([FromBody] QrRequest data)
        {
            try
            {
                string inputData = data?.QrCode?.Trim();
                if (string.IsNullOrEmpty(inputData))
                    return Json(new { success = false, message = "Vui lòng nhập hoặc quét mã hóa đơn!" });

                int maHD = 0;

                if (inputData.StartsWith("MaHD:") && inputData.Contains("|CinemaVN"))
                {
                    string maHdStr = inputData.Split('|')[0].Replace("MaHD:", "").Trim();
                    if (!int.TryParse(maHdStr, out maHD))
                        return Json(new { success = false, message = "Dữ liệu QR bị lỗi định dạng!" });
                }
                else
                {
                    if (!int.TryParse(inputData, out maHD))
                        return Json(new { success = false, message = "Mã hóa đơn nhập tay phải là chữ số!" });
                }

                var hoaDon = await _db.HoaDons
                    .Include(h => h.MaNdNavigation)
                    .Include(h => h.Ves).ThenInclude(v => v.MaScNavigation).ThenInclude(s => s.MaPhimNavigation)
                    .Include(h => h.Ves).ThenInclude(v => v.MaScNavigation).ThenInclude(s => s.MaPcNavigation)
                    .Include(h => h.Ves).ThenInclude(v => v.MaGheNavigation)
                    .FirstOrDefaultAsync(h => h.MaHd == maHD);

                if (hoaDon == null)
                    return Json(new { success = false, message = $"Không tìm thấy Hóa đơn #{maHD} trên hệ thống!" });

                var ves = hoaDon.Ves.ToList();
                if (!ves.Any())
                    return Json(new { success = false, message = "Hóa đơn này không chứa vé xem phim nào!" });

                var suatChieu = ves.First().MaScNavigation;

                if (suatChieu.NgayChieu?.Date != DateTime.Today)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Sai ngày! Vé này chiếu vào ngày {suatChieu.NgayChieu?.ToString("dd/MM/yyyy")}.",
                        phim = suatChieu.MaPhimNavigation?.TenPhim
                    });
                }

                bool allScanned = ves.All(v => v.TrangThai == 2);
                if (allScanned)
                    return Json(new { success = false, message = "CẢNH BÁO: Vé này ĐÃ ĐƯỢC SOÁT trước đó. Có dấu hiệu dùng lại vé cũ!" });

                foreach (var v in ves)
                {
                    if (v.TrangThai == 1) v.TrangThai = 2;
                }
                await _db.SaveChangesAsync();

                string tenGhe = string.Join(", ", ves.Select(v => v.MaGheNavigation.Hang + v.MaGheNavigation.SoGhe));

                return Json(new
                {
                    success = true,
                    message = "Soát vé THÀNH CÔNG!",
                    maHD = maHD,
                    phim = suatChieu.MaPhimNavigation?.TenPhim,
                    rap = suatChieu.MaPcNavigation?.TenPc,
                    suat = suatChieu.GioBd?.ToString(@"hh\:mm"),
                    ghe = tenGhe,
                    khach = hoaDon.MaNdNavigation?.HoTen ?? "Khách mua tại quầy",
                    soVe = ves.Count
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi Server: " + ex.Message });
            }
        }

    }

    public class QrRequest
    {
        public string QrCode { get; set; }
    }
}