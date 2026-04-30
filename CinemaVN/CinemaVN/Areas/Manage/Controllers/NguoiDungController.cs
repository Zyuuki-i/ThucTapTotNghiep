using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CinemaVN.Models;
using BCrypt.Net;
using System.IO;

namespace CinemaVN.Areas.Manage.Controllers
{
    [Area("Manage")]
    public class NguoiDungController : Controller
    {
        private readonly CinemaVNContext _db;
        public NguoiDungController(CinemaVNContext db) => _db = db;

        private NguoiDung GetCurrentUser()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            return _db.NguoiDungs.Include(u => u.MaCnNavigation).FirstOrDefault(u => u.MaNd == userId);
        }

        public IActionResult Index()
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null) return RedirectToAction("Login", "Home", new { area = "" });


            var dsNhanVien = _db.NguoiDungs
                .Include(n => n.MaVtNavigation)
                .Where(n => n.MaCn == currentUser.MaCn && n.MaVt != null)
                .OrderByDescending(n => n.MaNd)
                .ToList();

            ViewBag.TenChiNhanh = currentUser.MaCnNavigation?.TenCn;
            ViewBag.CurrentUserId = currentUser.MaNd;

            return View(dsNhanVien);
        }

        public IActionResult Them()
        {
            var currentUser = GetCurrentUser(); 
            if (currentUser == null) return RedirectToAction("Login", "Home", new { area = "" });

            ViewBag.TenChiNhanh = currentUser.MaCnNavigation?.TenCn;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Them(NguoiDung nv, IFormFile? HinhAnh)
        {
            var currentUser = GetCurrentUser();
            bool isDuplicate = _db.NguoiDungs.Any(n => n.Email == nv.Email || n.Cccd == n.Cccd);
            if (!isDuplicate)
            {
                TempData["MessageError_NV"] = "Tài khoản Email hoặc CCCD này đã tồn tại trong hệ thống!";
                ViewBag.TenChiNhanh = currentUser.MaCnNavigation?.TenCn;
                return View(nv);
            }
          
            if (!string.IsNullOrEmpty(nv.MatKhau))
            {
                nv.MatKhau = BCrypt.Net.BCrypt.HashPassword(nv.MatKhau);
            }
            if (HinhAnh != null && HinhAnh.Length > 0)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(HinhAnh.FileName);
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatars");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                string filePath = Path.Combine(uploadPath, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await HinhAnh.CopyToAsync(fileStream);
                }
                nv.Hinh = fileName;
            }
            else
            {
                nv.Hinh = "default-avatar.png";
            }

            nv.MaCn = currentUser.MaCn;
            nv.TrangThai = true;


            var staffRole = _db.VaiTros.FirstOrDefault(v => v.TenVt.ToLower().Contains("staff") || v.TenVt.ToLower().Contains("nhân viên"));

            if (staffRole != null)
            {
                nv.MaVt = staffRole.MaVt;
            }
            else
            {
                nv.MaVt = "staff"; 
            }

            try
            {
                _db.NguoiDungs.Add(nv);
                _db.SaveChanges(); 
                TempData["MessageSuccess_NV"] = "Đã cấp tài khoản nhân viên mới thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["MessageError_NV"] = "Lỗi hệ thống khi tạo tài khoản. Vui lòng kiểm tra lại dữ liệu nhập (VD: Ngày sinh không hợp lệ).";
                ViewBag.TenChiNhanh = currentUser.MaCnNavigation?.TenCn;
                return View(nv);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Xoa(int id)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null) return RedirectToAction("Login", "Home", new { area = "" });

            var nv = _db.NguoiDungs.FirstOrDefault(n => n.MaNd == id && n.MaCn == currentUser.MaCn);
            if (nv == null)
            {
                TempData["MessageError_NV"] = "Không tìm thấy hồ sơ hoặc bạn không có quyền xóa!";
                return RedirectToAction("Index");
            }

            if (nv.MaNd == currentUser.MaNd)
            {
                TempData["MessageError_NV"] = "Bạn không thể tự xóa tài khoản của chính mình!";
                return RedirectToAction("Index");
            }

            try
            {
                if (!string.IsNullOrEmpty(nv.Hinh) && nv.Hinh != "default-avatar.png")
                {
                    string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatars", nv.Hinh);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }
                _db.NguoiDungs.Remove(nv);
                await _db.SaveChangesAsync();

                TempData["MessageSuccess_NV"] = $"Đã xóa vĩnh viễn tài khoản của {nv.HoTen}!";
            }
            catch (Exception)
            {
                TempData["MessageError_NV"] = "Không thể xóa! Nhân viên này đã có dữ liệu hoạt động trong hệ thống (Hóa đơn, Bán vé...). Vui lòng sử dụng tính năng KHÓA tài khoản thay thế.";
            }

            return RedirectToAction("Index");
        }
        

        public IActionResult Sua(int id)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null) return RedirectToAction("Login", "Home", new { area = "" });

            var nv = _db.NguoiDungs.FirstOrDefault(n => n.MaNd == id && n.MaCn == currentUser.MaCn);
            if (nv == null)
            {
                TempData["MessageError_NV"] = "Không tìm thấy hồ sơ hoặc bạn không có quyền thao tác!";
                return RedirectToAction("Index");
            }

            ViewBag.TenChiNhanh = currentUser.MaCnNavigation?.TenCn;
            return View(nv);
        }

        [HttpPost]
        public async Task<IActionResult> Sua(NguoiDung nvUpdated, IFormFile? HinhAnhMoi, string? MatKhauMoi)
        {
            var currentUser = GetCurrentUser();
            var nvOld = _db.NguoiDungs.FirstOrDefault(n => n.MaNd == nvUpdated.MaNd && n.MaCn == currentUser.MaCn);
            if (nvOld == null)
            {
                TempData["MessageError_NV"] = "Dữ liệu không hợp lệ!";
                return RedirectToAction("Index");
            }

            if (nvOld.Email != nvUpdated.Email)
            {
                bool isDuplicate = _db.NguoiDungs.Any(n => n.Email == nvUpdated.Email);
                if (isDuplicate)
                {
                    TempData["MessageError_NV"] = "Email này đã được sử dụng bởi người khác!";
                    ViewBag.TenChiNhanh = currentUser.MaCnNavigation?.TenCn;
                    nvUpdated.Hinh = nvOld.Hinh;
                    return View(nvUpdated);
                }
                nvOld.Email = nvUpdated.Email;
            }
            if (HinhAnhMoi != null && HinhAnhMoi.Length > 0)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(HinhAnhMoi.FileName);
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatars");
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                using (var fileStream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create))
                {
                    await HinhAnhMoi.CopyToAsync(fileStream);
                }
                nvOld.Hinh = fileName;
            }

            if (!string.IsNullOrWhiteSpace(MatKhauMoi))
            {
                nvOld.MatKhau = BCrypt.Net.BCrypt.HashPassword(MatKhauMoi);
            }

            nvOld.HoTen = nvUpdated.HoTen;
            nvOld.NgaySinh = nvUpdated.NgaySinh;
            nvOld.Phai = nvUpdated.Phai;
            nvOld.Cccd = nvUpdated.Cccd;
            nvOld.Sdt = nvUpdated.Sdt;
            nvOld.DiaChi = nvUpdated.DiaChi;
            nvOld.ChucVu = nvUpdated.ChucVu;
            nvOld.TrangThai = nvUpdated.TrangThai;

            try
            {
                _db.NguoiDungs.Update(nvOld);
                await _db.SaveChangesAsync();
                TempData["MessageSuccess_NV"] = "Cập nhật hồ sơ thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["MessageError_NV"] = "Lỗi hệ thống khi lưu thay đổi.";
                ViewBag.TenChiNhanh = currentUser.MaCnNavigation?.TenCn;
                nvUpdated.Hinh = nvOld.Hinh;
                return View(nvUpdated);
            }
        }
        public IActionResult ChiTiet(int id)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null) return RedirectToAction("Login", "Home", new { area = "" });
            var nhanVien = _db.NguoiDungs
                .Include(n => n.MaVtNavigation)
                .Include(n => n.MaCnNavigation) // Lấy thêm tên chi nhánh nếu cần
                .FirstOrDefault(n => n.MaNd == id && n.MaCn == currentUser.MaCn);

            if (nhanVien == null)
            {
                TempData["MessageError_NV"] = "Không tìm thấy hồ sơ hoặc nhân viên không thuộc chi nhánh của bạn!";
                return RedirectToAction("Index");
            }

            return View(nhanVien);
        }

        [HttpPost]
        public async Task<IActionResult> Khoa(int id)
        {
            var currentUser = GetCurrentUser();
            var nv = _db.NguoiDungs.FirstOrDefault(n => n.MaNd == id && n.MaCn == currentUser.MaCn);

            if (nv != null && nv.MaNd != currentUser.MaNd)
            {
                nv.TrangThai = false;
                _db.NguoiDungs.Update(nv);
                await _db.SaveChangesAsync();
                TempData["MessageSuccess_NV"] = $"Đã khóa tài khoản của {nv.HoTen}!";
            }
            else
            {
                TempData["MessageError_NV"] = "Thao tác không hợp lệ!";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> MoKhoa(int id)
        {
            var currentUser = GetCurrentUser();
            var nv = _db.NguoiDungs.FirstOrDefault(n => n.MaNd == id && n.MaCn == currentUser.MaCn);

            if (nv != null)
            {
                nv.TrangThai = true; 
                _db.NguoiDungs.Update(nv);
                await _db.SaveChangesAsync();
                TempData["MessageSuccess_NV"] = $"Đã mở khóa tài khoản cho {nv.HoTen}!";
            }
            else
            {
                TempData["MessageError_NV"] = "Thao tác không hợp lệ!";
            }

            return RedirectToAction("Index");
        }
    }
}