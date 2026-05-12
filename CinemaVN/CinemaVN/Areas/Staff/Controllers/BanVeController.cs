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

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Home", new { area = "" });
            var staff = _db.NguoiDungs.Include(u => u.MaCnNavigation).FirstOrDefault(u => u.MaNd == userId);
            if (staff == null || staff.MaCn == null) return RedirectToAction("Login", "Home", new { area = "" });

            ViewBag.TenNhanVien = staff.HoTen;
            ViewBag.TenChiNhanh = staff.MaCnNavigation?.TenCn;

            var today = DateTime.Today;
            ViewBag.DanhSachPhim = _db.SuatChieus
                .Include(s => s.MaPhimNavigation)
                .Include(s => s.MaPcNavigation)
                .Where(s => s.MaPcNavigation.MaCn == staff.MaCn && s.NgayChieu == today)
                .Select(s => s.MaPhimNavigation)
                .Distinct()
                .ToList();

            ViewBag.DanhSachDichVu = _db.DichVus.ToList();
            ViewBag.DanhSachKhuyenMai = _db.KhuyenMais
                .Where(k => k.NgayBd <= today && k.NgayKt >= today && k.TrangThai == 1)
                .ToList();

            return View();
        }

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
        public async Task<IActionResult> KiemTraVoucher(string code, decimal tongTien, int? maKH)
        {
            try
            {
                if (string.IsNullOrEmpty(code)) return Json(new { success = false, message = "Vui lòng chọn mã khuyến mãi." });
                if (maKH == null) return Json(new { success = false, message = "Vui lòng tìm khách hàng thành viên để sử dụng ưu đãi!" });

                var today = DateTime.Now;
                var km = await _db.KhuyenMais.FirstOrDefaultAsync(k => k.Code == code && k.NgayBd <= today && k.NgayKt >= today && k.TrangThai == 1);

                if (km == null) return Json(new { success = false, message = "Mã khuyến mãi không tồn tại hoặc đã hết hạn!" });

                if (tongTien < (km.DieuKien ?? 0))
                    return Json(new { success = false, message = $"Hóa đơn phải từ {km.DieuKien?.ToString("N0")}đ để áp dụng mã này." });

                int diemYeuCau = km.SoDiem ?? 0;
                if (diemYeuCau > 0)
                {
                    var kh = await _db.NguoiDungs.FindAsync(maKH);
                    if (kh == null || (kh.DiemHoiVien ?? 0) < diemYeuCau)
                        return Json(new { success = false, message = $"Khách hàng không đủ điểm! Cần {diemYeuCau} điểm." });
                }

                return Json(new
                {
                    success = true,
                    maKM = km.MaKm,
                    phanTram = km.PhanTramGiam,
                    toiDa = km.GiamToiDa,
                    diemTru = diemYeuCau 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi Server: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSuatChieu(int maPhim)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                var staff = await _db.NguoiDungs.FindAsync(userId);
                var today = DateTime.Today;

                var suatChieus = await _db.SuatChieus
                    .Include(s => s.MaPcNavigation)
                    .Where(s => s.MaPcNavigation.MaCn == staff.MaCn && s.MaPhim == maPhim && s.NgayChieu == today)
                    .OrderBy(s => s.GioBd)
                    .Select(s => new
                    {
                        maSc = s.MaSc,
                        gioBd = s.GioBd.HasValue ? s.GioBd.Value.ToString(@"hh\:mm") : "00:00",
                        tenRap = s.MaPcNavigation.TenPc
                    })
                    .ToListAsync();

                return Json(new { success = true, data = suatChieus });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDanhSachGhe(int maSC)
        {
            var suatChieu = await _db.SuatChieus.FindAsync(maSC);
            if (suatChieu == null) return Json(new { success = false, message = "Không tìm thấy suất chiếu!" });

            var danhSachGhe = await _db.Ghes
                .Include(g => g.MaLgNavigation)
                .Where(g => g.MaPc == suatChieu.MaPc)
                .OrderBy(g => g.Hang).ThenBy(g => g.SoGhe)
                .ToListAsync();

            var now = DateTime.Now;
            var gheDaBan = await _db.Ves
                .Where(v => v.MaSc == maSC && (v.TrangThai == 1 || (v.TrangThai == 0 && v.ThoiGianGiu > now)))
                .Select(v => v.MaGhe)
                .ToListAsync();

            var soDoGhe = danhSachGhe.GroupBy(g => g.Hang).Select(group => new
            {
                hang = group.Key,
                seats = group.Select(g => new
                {
                    maGhe = g.MaGhe,
                    tenGhe = g.Hang + g.SoGhe.ToString(),
                    soGhe = g.SoGhe,
                    gia = 80000 + (g.MaLgNavigation?.PhuThu ?? 0),
                    loaiGhe = g.MaLgNavigation?.TenLg ?? "Thường",
                    daBan = gheDaBan.Contains(g.MaGhe)
                }).ToList()
            }).ToList();

            return Json(new { success = true, data = soDoGhe });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLockGhe(int maSC, int maGhe, bool isLock)
        {
            var ve = await _db.Ves.FirstOrDefaultAsync(v => v.MaSc == maSC && v.MaGhe == maGhe);

            if (isLock)
            {
                if (ve != null)
                {
                    if (ve.TrangThai == 1 || (ve.TrangThai == 0 && ve.ThoiGianGiu > DateTime.Now))
                        return Json(new { success = false, message = "Ghế đã bị người khác chọn!" });

                    ve.TrangThai = 0;
                    ve.ThoiGianGiu = DateTime.Now.AddMinutes(5); 
                    _db.Ves.Update(ve);
                }
                else
                {
                    _db.Ves.Add(new Ve { MaSc = maSC, MaGhe = maGhe, TrangThai = 0, ThoiGianGiu = DateTime.Now.AddMinutes(5) });
                }
            }
            else
            {
                if (ve != null && ve.TrangThai == 0)
                {
                    _db.Ves.Remove(ve);
                }
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> ThanhToanTienMat([FromBody] PaymentRequest data)
        {
            var staffId = HttpContext.Session.GetInt32("UserId");
            var staff = await _db.NguoiDungs.FindAsync(staffId);
            if (staff == null) return Json(new { success = false, message = "Lỗi phiên đăng nhập. Hãy F5 lại trang." });

            int diemThuong = 0;

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var hoaDon = new HoaDon
                {
                    MaCn = staff.MaCn,
                    MaNd = data.MaKhachHang,
                    MaKm = data.MaKhuyenMai,
                    NgayLap = DateTime.Now,
                    TongTien = data.TongTienSauGiam,
                    TrangThai = 1
                };
                _db.HoaDons.Add(hoaDon);
                await _db.SaveChangesAsync();

                if (data.DanhSachDichVu != null && data.DanhSachDichVu.Any())
                {
                    foreach (var dv in data.DanhSachDichVu)
                    {
                        _db.ChiTietDichVus.Add(new ChiTietDichVu
                        {
                            MaHd = hoaDon.MaHd,
                            MaDv = dv.MaDV,
                            SoLuong = dv.SoLuong,
                            DonGia = dv.DonGia
                        });
                    }
                }

                var holdVes = await _db.Ves.Where(v => v.MaSc == data.MaSuatChieu && data.DanhSachMaGhe.Contains(v.MaGhe ?? 0) && v.TrangThai == 0).ToListAsync();
                if (holdVes.Any()) _db.Ves.RemoveRange(holdVes);
                await _db.SaveChangesAsync();

                foreach (var maGhe in data.DanhSachMaGhe)
                {
                    _db.Ves.Add(new Ve
                    {
                        MaHd = hoaDon.MaHd,
                        MaSc = data.MaSuatChieu,
                        MaGhe = maGhe,
                        Gia = 80000,
                        TrangThai = 1
                    });
                }

                if (data.MaKhachHang != null)
                {
                    var kh = await _db.NguoiDungs.FindAsync(data.MaKhachHang);
                    if (kh != null)
                    {
                        int diemTru = 0;
                        if (data.MaKhuyenMai != null)
                        {
                            var km = await _db.KhuyenMais.FindAsync(data.MaKhuyenMai);
                            if (km != null) diemTru = km.SoDiem ?? 0;
                        }
                        if (data.TongTienSauGiam > 0)
                        {
                            diemThuong = (int)Math.Floor((double)(data.TongTienSauGiam / 1000));
                        }
                        kh.DiemHoiVien = (kh.DiemHoiVien ?? 0) + diemThuong - diemTru;
                        if (kh.DiemHoiVien < 0) kh.DiemHoiVien = 0;

                        _db.NguoiDungs.Update(kh);
                    }
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, maHD = hoaDon.MaHd, diemThuong = diemThuong });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Lỗi hệ thống: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }

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
        public List<DichVuRequest> DanhSachDichVu { get; set; }
        public int? MaKhachHang { get; set; }
        public int? MaKhuyenMai { get; set; }
        public decimal TongTienSauGiam { get; set; }
    }

    public class DichVuRequest
    {
        public int MaDV { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
    }
}