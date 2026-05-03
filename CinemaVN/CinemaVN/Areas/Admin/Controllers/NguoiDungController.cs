using CinemaVN.DatModels;
using CinemaVN.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace CinemaVN.Areas.Admin.Controllers
{ 

    [Area("Admin")]
    public class NguoiDungController : Controller
    {
        private readonly CinemaVNContext db = new();
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult quanLy(string? macn, string? keyword, int trang = 1, bool trangthai = true)
        {
            var query = db.NguoiDungs.Where(t => t.MaVt != null && t.MaVt.ToLower() == "manage").AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(t =>
                    (t.HoTen != null && t.HoTen.ToLower().Contains(keyword.ToLower())) ||
                    (t.Cccd != null && t.Cccd.ToLower().Contains(keyword.ToLower())) ||
                    (t.Sdt != null && t.Sdt.ToLower().Contains(keyword.ToLower())) ||
                    (t.Email != null && t.Email.ToLower().Contains(keyword.ToLower())) ||
                    (t.MaNd.ToString().Contains(keyword.ToLower()))
                );
            }

            if (!string.IsNullOrEmpty(macn))
            {
                query = query.Where(t => t.MaCn != null && t.MaCn.ToLower() == macn.ToLower());
            }
            if (trangthai) {
                query = query.Where(t => t.TrangThai == true);
            }
            else
            {
                query = query.Where(t => t.TrangThai == false);
            }

            int kichThuoc = 5;
            int tongTrang = (int)Math.Ceiling((double)query.Count() / kichThuoc);
            query = query.Skip((trang - 1) * kichThuoc).Take(kichThuoc);

            ViewBag.trangHT = trang;
            ViewBag.tongTrang = tongTrang;

            ViewBag.Macn = macn;
            ViewBag.Keyword = keyword;
            ViewBag.Trangthai = trangthai;

            ViewBag.ChiNhanhs = db.ChiNhanhs.ToList();

            List<CNguoiDung> cNguoiDungs = new List<CNguoiDung>();
            foreach(var item in query.ToList())
            {
                cNguoiDungs.Add(CNguoiDung.ToCNguoiDung(item));
            }
            return View(cNguoiDungs);
        }

        public IActionResult nhanVien(string? macn, string? keyword, int trang = 1, bool trangthai = true)
        {
            var query = db.NguoiDungs.Where(t => t.MaVt != null && t.MaVt.ToLower() == "staff").AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(t =>
                    (t.HoTen != null && t.HoTen.ToLower().Contains(keyword.ToLower())) ||
                    (t.Cccd != null && t.Cccd.ToLower().Contains(keyword.ToLower())) ||
                    (t.Sdt != null && t.Sdt.ToLower().Contains(keyword.ToLower())) ||
                    (t.Email != null && t.Email.ToLower().Contains(keyword.ToLower())) ||
                    (t.MaNd.ToString().Contains(keyword.ToLower()))
                );
            }

            if (!string.IsNullOrEmpty(macn))
            {
                query = query.Where(t => t.MaCn != null && t.MaCn.ToLower() == macn.ToLower());
            }
            if (trangthai)
            {
                query = query.Where(t => t.TrangThai == true);
            }
            else
            {
                query = query.Where(t => t.TrangThai == false);
            }

            int kichThuoc = 5;
            int tongTrang = (int)Math.Ceiling((double)query.Count() / kichThuoc);
            query = query.Skip((trang - 1) * kichThuoc).Take(kichThuoc);

            ViewBag.trangHT = trang;
            ViewBag.tongTrang = tongTrang;

            ViewBag.Macn = macn;
            ViewBag.Keyword = keyword;
            ViewBag.Trangthai = trangthai;

            ViewBag.ChiNhanhs = db.ChiNhanhs.ToList();

            List<CNguoiDung> cNguoiDungs = new List<CNguoiDung>();
            foreach (var item in query.ToList())
            {
                cNguoiDungs.Add(CNguoiDung.ToCNguoiDung(item));
            }
            return View(cNguoiDungs);
        }

        public IActionResult khachHang(string? keyword, int trang = 1, bool trangthai = true)
        {
            var query = db.NguoiDungs.Where(t => t.MaVt != null && t.MaVt.ToLower() == "customer").AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(t =>
                    (t.HoTen != null && t.HoTen.ToLower().Contains(keyword.ToLower())) ||
                    (t.Sdt != null && t.Sdt.ToLower().Contains(keyword.ToLower())) ||
                    (t.Email != null && t.Email.ToLower().Contains(keyword.ToLower())) ||
                    (t.MaNd.ToString().Contains(keyword.ToLower()))
                );
            }

            if (trangthai)
            {
                query = query.Where(t => t.TrangThai == true);
            }
            else
            {
                query = query.Where(t => t.TrangThai == false);
            }

            int kichThuoc = 5;
            int tongTrang = (int)Math.Ceiling((double)query.Count() / kichThuoc);
            query = query.Skip((trang - 1) * kichThuoc).Take(kichThuoc);

            ViewBag.trangHT = trang;
            ViewBag.tongTrang = tongTrang;

            ViewBag.Keyword = keyword;
            ViewBag.Trangthai = trangthai;

            List<CNguoiDung> cNguoiDungs = new List<CNguoiDung>();
            foreach (var item in query.ToList())
            {
                cNguoiDungs.Add(CNguoiDung.ToCNguoiDung(item));
            }
            return View(cNguoiDungs);
        }

        public IActionResult chuyenTrang(string role = "customer", int trang = 1, bool trangthai = true)
        {
            switch (role?.ToLower())
            {
                case "customer":
                    return RedirectToAction("khachHang", new { trang, trangthai });

                case "staff":
                    return RedirectToAction("nhanVien", new { trang, trangthai });

                case "manage":
                    return RedirectToAction("quanLy", new { trang, trangthai });

                default:
                    return RedirectToAction("Index");
            }
        }

        private int timTrang(int id)
        {
            NguoiDung? x = db.NguoiDungs.Find(id);
            if (x == null) return 1;

            int kichThuoc = 5;

            int soLuongTruoc = db.NguoiDungs
                .Where(t => t.MaVt == x.MaVt && t.TrangThai == x.TrangThai && t.MaNd < x.MaNd)
                .Count();

            return (soLuongTruoc / kichThuoc) + 1;
        }

        public IActionResult voHieu(int id, string role)
        {
            NguoiDung? nd = db.NguoiDungs.Find(id);
            if (nd == null)
            {
                TempData["MessageError_NguoiDung"] = "Lỗi, không tìm thấy người dùng!";
                return RedirectToAction("chuyenTrang", new { role = role });
            }
            int trang = timTrang(id);
            try
            {
                nd.TrangThai = false;
                db.NguoiDungs.Update(nd);
                db.SaveChanges();
                TempData["MessageSuccess_NguoiDung"] = "Vô hiệu người dùng '" + id + "' thành công!";
                return RedirectToAction("chuyenTrang", new { role = role, trang = trang, trangthai = nd.TrangThai });
            }
            catch (Exception)
            {
                TempData["MessageError_NguoiDung"] = "Lỗi, vô hiệu thất bại!";
                return RedirectToAction("chuyenTrang", new { role = role, trang = trang, trangthai = nd.TrangThai });
            }
        }

        public IActionResult kichHoat(int id, string role)
        {
            NguoiDung? nd = db.NguoiDungs.Find(id);
            if (nd == null)
            {
                TempData["MessageError_NguoiDung"] = "Lỗi, không tìm thấy người dùng!";
                return RedirectToAction("chuyenTrang", new { role = role });
            }
            int trang = timTrang(id);
            try
            {
                nd.TrangThai = true;
                db.NguoiDungs.Update(nd);
                db.SaveChanges();
                TempData["MessageSuccess_NguoiDung"] = "Kích hoạt người dùng '" + id + "' thành công!";
                return RedirectToAction("chuyenTrang", new { role = role, trang = trang, trangthai = nd.TrangThai });
            }
            catch (Exception)
            {
                TempData["MessageError_NguoiDung"] = "Lỗi, kích hoạt thất bại!";
                return RedirectToAction("chuyenTrang", new { role = role, trang = trang, trangthai = nd.TrangThai });
            }
        }

        public IActionResult chiTiet(int id, string role)
        {
            NguoiDung? nd = db.NguoiDungs
                            .Include(n => n.MaVtNavigation)
                            .Include(n => n.MaCnNavigation)
                            .FirstOrDefault(n => n.MaNd == id);
            if (nd == null)
            {
                TempData["MessageError_NguoiDung"] = "Lỗi, không tìm thấy người dùng!";
                return RedirectToAction("chuyenTrang", new { role = role });
            }
            return View(CNguoiDung.ToCNguoiDung(nd));
        }

        public IActionResult xoaAnh(int id, string role)
        {
            NguoiDung? nd = db.NguoiDungs.Find(id);
            if (nd == null)
            {
                TempData["MessageError_NguoiDung"] = "Lỗi, không tìm thấy người dùng!";
                return RedirectToAction("chuyenTrang", new { role = role });
            }
            int trang = timTrang(id);
            try
            {
                string avatarPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatars", nd.Hinh ?? "");
                if (System.IO.File.Exists(avatarPath))
                {
                    System.IO.File.Delete(avatarPath);
                }
                nd.Hinh = null;
                db.NguoiDungs.Update(nd);
                db.SaveChanges();
                TempData["MessageSuccess_ChiTietNguoiDung"] = "Xóa hình người dùng thành công!";
                return RedirectToAction("chiTiet", new {id = id, role = role});
            }
            catch (Exception)
            {
                TempData["MessageError_ChiTietNguoiDung"] = "Lỗi, không thể xóa hình!";
                return RedirectToAction("chiTiet", new { id = id, role = role });
            }
        }

        public IActionResult resetPass(int id, string role)
        {
            NguoiDung? nd = db.NguoiDungs.Find(id);
            if (nd == null)
            {
                TempData["MessageError_NguoiDung"] = "Lỗi, không tìm thấy người dùng!";
                return RedirectToAction("chuyenTrang", new { role = role });
            }
            int trang = timTrang(id);
            try
            {
                nd.MatKhau = BCrypt.Net.BCrypt.HashPassword("123456");
                db.NguoiDungs.Update(nd);
                db.SaveChanges();
                TempData["MessageSuccess_ChiTietNguoiDung"] = "Đã cập nhật lại mật khẩu! Mật khẩu mới: 123456";
                return RedirectToAction("chiTiet", new { id = id, role = role });
            }
            catch (Exception)
            {
                TempData["MessageError_ChiTietNguoiDung"] = "Lỗi, không thể reset mật khẩu!";
                return RedirectToAction("chiTiet", new { id = id, role = role });
            }
        }

        public IActionResult them(string role)
        {
            CNguoiDung nd = new CNguoiDung() { MaVt = role};
            ViewBag.ChiNhanhs = new SelectList(db.ChiNhanhs, "MaCn", "TenCn");
            return View(nd);
        }

        [HttpPost]
        public IActionResult them(CNguoiDung x)
        {
            NguoiDung nd = new NguoiDung()
            {
                MaVt = x.MaVt,
                MaCn = x.MaCn,
                HoTen = x.HoTen,
                Email = x.Email,
                MatKhau = BCrypt.Net.BCrypt.HashPassword(x.MatKhau),
                Phai = x.Phai,
                NgaySinh = x.NgaySinh,
                Sdt = x.Sdt,
                Cccd = x.Cccd,
                ChucVu = x.ChucVu,
                DiaChi = x.DiaChi,
                DiemHoiVien = x.DiemHoiVien,
                TrangThai = true
            };
            try
            {
                db.NguoiDungs.Add(nd);
                db.SaveChanges();
                TempData["MessageSuccess_NguoiDung"] = "Tạo tài khoản mới thành công!";
                return RedirectToAction("chuyenTrang", new { role = nd.MaVt, trangthai = nd.TrangThai });
            }
            catch (Exception) 
            {
                TempData["MessageError_ThemNguoiDung"] = "Lỗi, không thể thêm mới!";
                ViewBag.ChiNhanhs = new SelectList(db.ChiNhanhs, "MaCn", "TenCn");
                return View(nd);
            }
        }

        public IActionResult sua(int id, string role)
        {
            NguoiDung? nd = db.NguoiDungs.Find(id);
            if (nd == null)
            {
                TempData["MessageError_NguoiDung"] = "Lỗi, không tìm thấy người dùng!";
                return RedirectToAction("chuyenTrang", new { role = role});
            }
            ViewBag.ChiNhanhs = new SelectList(db.ChiNhanhs, "MaCn", "TenCn");
            ViewBag.VaiTros = new SelectList(db.VaiTros, "MaVt", "TenVt");
            return View(CNguoiDung.ToCNguoiDung(nd));
        }

        [HttpPost]
        public IActionResult sua(CNguoiDung x)
        {
            NguoiDung? nd = db.NguoiDungs.Find(x.MaNd);
            ViewBag.ChiNhanhs = new SelectList(db.ChiNhanhs, "MaCn", "TenCn");
            ViewBag.VaiTros = new SelectList(db.VaiTros, "MaVt", "TenVt");
            if (nd == null)
            {
                TempData["MessageError_NguoiDung"] = "Lỗi, dữ liệu không hợp lệ!";
                return View(x);
            }
            nd.MaVt = x.MaVt;
            nd.MaCn = x.MaCn;
            nd.HoTen = x.HoTen;
            nd.Email = x.Email;
            nd.Phai = x.Phai;
            nd.NgaySinh = x.NgaySinh;
            nd.Sdt = x.Sdt;
            nd.Cccd = x.Cccd;
            nd.ChucVu = x.ChucVu;
            nd.DiaChi = x.DiaChi;
            nd.DiemHoiVien = x.DiemHoiVien;
            int trang = timTrang(nd.MaNd);
            try
            {
                db.NguoiDungs.Update(nd);
                db.SaveChanges();
                TempData["MessageSuccess_NguoiDung"] = "Cập nhật thành công!";
                return RedirectToAction("chuyenTrang", new { role = nd.MaVt, trang = trang, trangthai = nd.TrangThai });
            }
            catch (Exception)
            {
                TempData["MessageError_SuaNguoiDung"] = "Lỗi, không thể sửa đổi!";
                return View(x);
            }
        }



    }
}
