using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaVN.Models; 
using QRCoder; 
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CinemaVN.Areas.Staff.Controllers
{
    [Area("Staff")]
    public class BanVeController : Controller
    {
        private readonly CinemaVNContext _db;

        public BanVeController(CinemaVNContext db)
        {
            _db = db;
        }

        // 1. MỞ GIAO DIỆN QUẦY BÁN VÉ
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Home", new { area = "" });

            // Lấy thông tin Staff và Chi nhánh
            var staff = _db.NguoiDungs.Include(u => u.MaCnNavigation).FirstOrDefault(u => u.MaNd == userId);
            if (staff == null || staff.MaCn == null) return RedirectToAction("Login", "Home", new { area = "" });

            ViewBag.TenNhanVien = staff.HoTen;
            ViewBag.TenChiNhanh = staff.MaCnNavigation?.TenCn;

            // Lấy danh sách phim ĐANG CHIẾU hôm nay tại Chi nhánh
            var today = DateTime.Today;
            ViewBag.DanhSachPhim = _db.SuatChieus
                .Include(s => s.MaPhimNavigation)
                .Include(s => s.MaPcNavigation)
                .Where(s => s.MaPcNavigation.MaCn == staff.MaCn && s.NgayChieu == today)
                .Select(s => s.MaPhimNavigation)
                .Distinct()
                .ToList();

            // Lấy danh sách dịch vụ (Bắp nước)
            ViewBag.DanhSachDichVu = _db.DichVus.ToList();

            return View();
        }

        // 2. API TÌM KHÁCH HÀNG
        [HttpGet]
        public async Task<IActionResult> TimKhachHang(string sdt)
        {
            try
            {
                if (string.IsNullOrEmpty(sdt)) return Json(new { success = false, message = "Vui lòng nhập SĐT." });

                var kh = await _db.NguoiDungs.FirstOrDefaultAsync(u => u.Sdt == sdt);
                if (kh == null) return Json(new { success = false, message = "Không tìm thấy khách hàng!" });

                return Json(new { success = true, maKh = kh.MaNd, tenKh = kh.HoTen, diem = kh.DiemHoiVien ?? 0 });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi Server: " + ex.Message });
            }
        }
        [HttpGet]
        //Danh sách ghế
        public async Task<IActionResult> GetDanhSachGhe(int maSC)
        {
            // 1. Tìm thông tin suất chiếu để biết đang chiếu ở Phòng nào (MaPc)
            var suatChieu = await _db.SuatChieus.FindAsync(maSC);
            if (suatChieu == null) return Json(new { success = false, message = "Không tìm thấy suất chiếu!" });

            // 2. Lấy toàn bộ ghế của Phòng chiếu đó, gộp kèm thông tin Phụ thu từ Loại Ghế
            var danhSachGhe = await _db.Ghes
                .Include(g => g.MaLgNavigation)
                .Where(g => g.MaPc == suatChieu.MaPc)
                .OrderBy(g => g.Hang).ThenBy(g => g.SoGhe) // Sắp xếp A->Z, 1->9
                .ToListAsync();

            // 3. Lấy danh sách ID các ghế ĐÃ BÁN của suất chiếu này (Trạng thái > 0)
            var gheDaBan = await _db.Ves
                .Where(v => v.MaSc == maSC && v.TrangThai > 0)
                .Select(v => v.MaGhe)
                .ToListAsync();

            // 4. Gom nhóm theo Hàng (A, B, C...) để gửi xuống View dễ vẽ giao diện
            var soDoGhe = danhSachGhe.GroupBy(g => g.Hang).Select(group => new
            {
                hang = group.Key, // Ví dụ: 'A'
                seats = group.Select(g => new
                {
                    maGhe = g.MaGhe,
                    tenGhe = g.Hang + g.SoGhe.ToString(), // Ví dụ: "A1"
                    soGhe = g.SoGhe, // Ví dụ: 1
                    gia = 80000 + (g.MaLgNavigation?.PhuThu ?? 0), // Giá cơ bản 80k + Phụ thu VIP
                    loaiGhe = g.MaLgNavigation?.TenLg ?? "Thường", // 'Thường' hoặc 'VIP'
                    daBan = gheDaBan.Contains(g.MaGhe) // true nếu ghế nằm trong mảng đã bán
                }).ToList()
            }).ToList();

            return Json(new { success = true, data = soDoGhe });
        }
        // 3. API THANH TOÁN (TIỀN MẶT)
        [HttpPost]
        public async Task<IActionResult> ThanhToanTienMat([FromBody] PaymentRequest data)
        {
            var staffId = HttpContext.Session.GetInt32("UserId");
            var staff = await _db.NguoiDungs.FindAsync(staffId);
            if (staff == null) return Json(new { success = false, message = "Lỗi phiên đăng nhập. Hãy F5 lại trang." });

            int diemThuong = 0;
            // 1 Mã QR dùng chung cho tất cả các ghế trong 1 hóa đơn
            string sharedSessionKey = Guid.NewGuid().ToString("N").Substring(0, 15).ToUpper();

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // TẠO HÓA ĐƠN
                var hoaDon = new HoaDon
                {
                    MaCn = staff.MaCn,
                    MaNd = data.MaKhachHang,
                    NgayLap = DateTime.Now,
                    TongTien = data.TongTienSauGiam,
                    TrangThai = 1 // 1: Đã thanh toán
                };
                _db.HoaDons.Add(hoaDon);
                await _db.SaveChangesAsync();

                // TẠO VÉ & LƯU QR CODE
                foreach (var maGhe in data.DanhSachMaGhe)
                {
                    var ve = new Ve
                    {
                        MaHd = hoaDon.MaHd,
                        MaSc = data.MaSuatChieu,
                        MaGhe = maGhe,
                        Gia = 80000, 
                        TrangThai = 1, 
                        SessionKey = sharedSessionKey
                    };
                    _db.Ves.Add(ve);
                }

                // TÍCH ĐIỂM (100.000đ = 100 điểm => 1 điểm = 1000đ)
                if (data.MaKhachHang != null && data.TongTienSauGiam > 0)
                {
                    var kh = await _db.NguoiDungs.FindAsync(data.MaKhachHang);
                    if (kh != null)
                    {
                        diemThuong = (int)Math.Floor((double)(data.TongTienSauGiam / 1000));
                        kh.DiemHoiVien = (kh.DiemHoiVien ?? 0) + diemThuong;
                        _db.NguoiDungs.Update(kh);
                    }
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, maHD = hoaDon.MaHd, sessionKey = sharedSessionKey, diemThuong = diemThuong });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.InnerException?.Message ?? ex.Message });
            }
        }

        // 4. API TẠO ẢNH MÃ QR TỪ CHUỖI
        [HttpGet]
        public IActionResult RenderQR(string key)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(key, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            return File(qrCode.GetGraphic(20), "image/png");
        }
    }

    public class PaymentRequest
    {
        public int MaSuatChieu { get; set; }
        public List<int> DanhSachMaGhe { get; set; }
        public int? MaKhachHang { get; set; }
        public decimal TongTienSauGiam { get; set; }
    }
}