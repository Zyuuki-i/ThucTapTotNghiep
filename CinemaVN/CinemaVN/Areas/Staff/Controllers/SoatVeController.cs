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
        // 1. GIAO DIỆN QUÉT MÃ QR
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Home", new { area = "" });

            var staff = _db.NguoiDungs.Include(u => u.MaCnNavigation).FirstOrDefault(u => u.MaNd == userId);
            if (staff == null) return RedirectToAction("Login", "Home", new { area = "" });

            ViewBag.TenNhanVien = staff.HoTen;
            ViewBag.TenChiNhanh = staff.MaCnNavigation?.TenCn;

            return View();
        }
        // 2. API XỬ LÝ SOÁT VÉ (NHẬN TỪ AJAX)
        [HttpPost]
        public async Task<IActionResult> XuLyQuetVe(string qrCode)
        {
            if (string.IsNullOrEmpty(qrCode))
            {
                return Json(new { success = false, message = "Mã QR trống hoặc không hợp lệ!" });
            }

            // 1. BÓC TÁCH LẤY MÃ HÓA ĐƠN (MaHD) TỪ MÃ QR HOẶC NHẬP TAY
            int maHD = 0;
            if (qrCode.StartsWith("MaHD:"))
            {
                // Xử lý mã QR (VD: "MaHD:105|CinemaVN" -> Lấy số 105)
                var parts = qrCode.Split('|');
                var idPart = parts[0].Replace("MaHD:", "");
                int.TryParse(idPart, out maHD);
            }
            else
            {
                // Xử lý nhập tay (Nhân viên nhập thẳng số 105 vào ô)
                int.TryParse(qrCode, out maHD);
            }

            if (maHD == 0)
            {
                return Json(new { success = false, message = "Mã hóa đơn không đúng định dạng!" });
            }

            // 2. TÌM TẤT CẢ VÉ THUỘC MÃ HÓA ĐƠN NÀY
           
            var ves = await _db.Ves
                .Include(v => v.MaScNavigation).ThenInclude(s => s.MaPhimNavigation)
                .Include(v => v.MaScNavigation).ThenInclude(s => s.MaPcNavigation)
                .Include(v => v.MaGheNavigation)
                .Where(v => v.MaHd == maHD)
                .ToListAsync();

            if (!ves.Any())
            {
                return Json(new { success = false, message = $"Không tìm thấy vé nào cho Hóa Đơn #{maHD}!" });
            }

            var suatChieu = ves.First().MaScNavigation;
            var phim = suatChieu?.MaPhimNavigation;
            var phong = suatChieu?.MaPcNavigation;

            // 3. KIỂM TRA NGÀY CHIẾU (Bảo mật: Khách không thể lấy vé ngày mai đi xem hôm nay)
            if (suatChieu != null && suatChieu.NgayChieu?.Date != DateTime.Today)
            {
                string ngayChieu = suatChieu.NgayChieu?.ToString("dd/MM/yyyy") ?? "Không xác định";
                return Json(new { success = false, message = $"Từ chối! Hóa đơn này mua vé cho ngày {ngayChieu}, không phải hôm nay." });
            }

            // 4. LỌC CÁC VÉ CÓ TRẠNG THÁI = 1 (HỢP LỆ: Đã thanh toán, chưa soát vé)
            var veHopLe = ves.Where(v => v.TrangThai == 1).ToList();

            if (!veHopLe.Any())
            {
                // Bắt lỗi cụ thể để báo cho nhân viên biết
                if (ves.All(v => v.TrangThai == 2))
                {
                    return Json(new { success = false, message = $"Hóa đơn #{maHD} ĐÃ ĐƯỢC SOÁT VÉ trước đó!" });
                }
                return Json(new { success = false, message = $"Hóa đơn #{maHD} chưa thanh toán hoặc đã bị hủy!" });
            }

            // 5. TIẾN HÀNH SOÁT VÉ: CHUYỂN TRẠNG THÁI TỪ 1 SANG 2
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                foreach (var ve in veHopLe)
                {
                    ve.TrangThai = 2; // 2: Đã dùng (Đã soát vé)
                    _db.Ves.Update(ve);
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                // Lấy tên các ghế gộp thành chuỗi (VD: "A1, A2, E2 E3")
                string danhSachGhe = string.Join(", ", veHopLe.Select(v => v.MaGheNavigation?.Hang + v.MaGheNavigation?.SoGhe));

                // Trả về dữ liệu thành công cho giao diện
                return Json(new
                {
                    success = true,
                    maHD = maHD,
                    phim = phim?.TenPhim,
                    rap = phong?.TenPc,
                    suat = suatChieu?.GioBd?.ToString(@"hh\:mm"),
                    ghe = danhSachGhe,
                    soLuong = veHopLe.Count
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}