using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using CinemaVN.Models;
using System.Security.Cryptography;
using BCrypt.Net;
using CinemaVN.DatModels;
using Microsoft.EntityFrameworkCore;

namespace CinemaVN.Controllers
{
    public class NguoiDungController : Controller
    {
        private readonly EmailService _emailService;
        public NguoiDungController(EmailService emailService)
        {
            _emailService = emailService;
        }

        private CinemaVNContext db = new CinemaVNContext();

        public IActionResult Index()
        {
            NguoiDung? nd = db.NguoiDungs
                            .Include(n => n.MaVtNavigation)
                            .Include(n => n.MaCnNavigation)
                            .FirstOrDefault(n => n.MaNd == HttpContext.Session.GetInt32("UserId"));
            if (nd == null)
            {
                return RedirectToAction("DangNhap");
            }
            return View(CNguoiDung.ToCNguoiDung(nd));
        }

        public IActionResult dangNhap()
        {
            return View();
        }

        [HttpPost]
        public IActionResult dangNhap(string email, string matkhau)
        {
            ViewBag.Email = email;
            ViewBag.Matkhau = matkhau;

            var tk = db.NguoiDungs.FirstOrDefault(t => t.Email == email);
            var tt = db.NguoiDungs;
            if (tk == null)
            {
                TempData["MessageError_DangNhap"] = "Email không tồn tại!";
            }
            else
            {
                bool ktraMatKhau = BCrypt.Net.BCrypt.Verify(matkhau, tk.MatKhau);

                if (ktraMatKhau)
                {
                    if (tk.TrangThai == true)
                    {
                        HttpContext.Session.SetString("UserRole", tk.MaVt?.Trim()??"customer");
                        HttpContext.Session.SetString("UserEmail", email);
                        HttpContext.Session.SetInt32("UserId", tk.MaNd);
                        HttpContext.Session.SetString("UserName", tk.HoTen??"Unknown");
                        return RedirectToAction("Index", "Home", new { area = (tk.MaVt=="customer"?"": tk.MaVt)});
                    }
                    TempData["MessageError_DangNhap"] = "Tài khoản người dùng đã bị khóa!";
                }
                else
                {
                    TempData["MessageError_DangNhap"] = "Mật khẩu không đúng! Vui lòng thử lại.";
                }
            }
            return View();
        }

        public IActionResult dangXuat()
        {
            HttpContext.Session.Remove("UserRole");
            HttpContext.Session.Remove("UserEmail");
            HttpContext.Session.Remove("UserId");
            HttpContext.Session.Remove("UserName");
            return RedirectToAction("DangNhap");
        }

        public IActionResult doiMatKhau()
        {
            string? email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
            {
                TempData["MessageError_NguoiDung"] = "Phát sinh sự cố! Tạm thời không thể truy cập.";
                return RedirectToAction("Index");
            }
            Register register = new Register
            {
                Email = email
            };
            return View(register);
        }

        [HttpPost]
        public IActionResult doiMatKhau(Register reg, string passOld)
        {
            if (reg == null || string.IsNullOrEmpty(reg.Email) || string.IsNullOrEmpty(reg.Password) || string.IsNullOrEmpty(passOld))
            {
                TempData["MessageError_DoiMatKhau"] = "Thông tin không hợp lệ!";
                return RedirectToAction("doiMatKhau", reg);
            }
            var user = db.NguoiDungs.FirstOrDefault(u => u.Email == reg.Email);
            if (user == null)
            {
                TempData["MessageError_DoiMatKhau"] = "Người dùng không tồn tại!";
                return RedirectToAction("doiMatKhau", reg);
            }
            if (!BCrypt.Net.BCrypt.Verify(passOld,user.MatKhau))
            {
                TempData["MessageError_DoiMatKhau"] = "Mật khẩu cũ không đúng!";
                return RedirectToAction("doiMatKhau", reg);
            }
            if (BCrypt.Net.BCrypt.Verify(reg.Password, user.MatKhau))
            {
                TempData["MessageError_DoiMatKhau"] = "Mật khẩu mới phải khác mật khẩu cũ!";
                return RedirectToAction("doiMatKhau", reg);
            }
            user.MatKhau = BCrypt.Net.BCrypt.HashPassword(reg.Password);
            try
            {
                db.SaveChanges();
                TempData["MessageSuccess_NguoiDung"] = "Đổi mật khẩu thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["MessageError_DoiMatKhau"] = "Đổi mật khẩu thất bại! Xảy ra lỗi trong quá trình xử lý.";
                return RedirectToAction("doiMatKhau", reg);
            }
        }

        public IActionResult quenMatKhau(Register reg)
        {
            reg ??= new Register();
            return View(reg);
        }

        [HttpPost]
        public IActionResult quenMatKhauOTP(Register reg)
        {
            if (reg == null || string.IsNullOrEmpty(reg.Email))
            {
                TempData["MessageError_QuenMatKhau"] = "Thông tin không hợp lệ!";
                return RedirectToAction("quenMatKhau", reg);
            }
            var user = db.NguoiDungs.FirstOrDefault(u => u.Email == reg.Email);
            if (user == null)
            {
                TempData["MessageError_QuenMatKhau"] = "Email không tồn tại!";
                return RedirectToAction("quenMatKhau", reg);
            }
            string otp = new Random().Next(111111, 999999).ToString();
            HttpContext.Session.SetString("OTP_QuenMatKhau", otp);
            HttpContext.Session.SetString("OTP_Email_QuenMatKhau", reg.Email);
            HttpContext.Session.SetString("OTP_Time_QuenMatKhau", DateTime.Now.ToString());
            _emailService.SendOTP(reg.Email, otp);
            TempData["MessageSuccess_OTP"] = "Đã gửi OTP! Vui lòng kiểm tra email.";
            return View(reg);
        }

        [HttpPost]
        public IActionResult quenMatKhauVerifyOTP(Register reg, string OTP)
        {
            if (reg == null || string.IsNullOrEmpty(reg.Email))
            {
                TempData["MessageError_QuenMatKhau"] = "Thông tin không hợp lệ!";
                return RedirectToAction("quenMatKhau", reg);
            }
            string? otp = HttpContext.Session.GetString("OTP_QuenMatKhau");
            string? timeStr = HttpContext.Session.GetString("OTP_Time_QuenMatKhau");
            if (otp == null || timeStr == null)
            {
                TempData["MessageError_OTP"] = "OTP không tồn tại!";
                return View("quenMatKhauOTP", reg);
            }
            DateTime time = DateTime.Parse(timeStr);
            if ((DateTime.Now - time).TotalMinutes > 5)
            {
                TempData["MessageError_OTP"] = "OTP đã hết hạn!";
                return View("quenMatKhauOTP", reg);
            }
            string? email = HttpContext.Session.GetString("OTP_Email_QuenMatKhau");
            if (OTP != otp || reg.Email != email)
            {
                TempData["MessageError_OTP"] = "OTP không hợp lệ!";
                return View("quenMatKhauOTP", reg);
            }
            try
            {
                NguoiDung? user = db.NguoiDungs.FirstOrDefault(u => u.Email == reg.Email);
                if (user == null)
                {
                    TempData["MessageError_QuenMatKhau"] = "Email không tồn tại!";
                    return View("quenMatKhauOTP", reg);
                }
                user.MatKhau = BCrypt.Net.BCrypt.HashPassword(reg.Password);
                db.SaveChanges();
                HttpContext.Session.Remove("OTP_QuenMatKhau");
                HttpContext.Session.Remove("OTP_Email_QuenMatKhau");
                HttpContext.Session.Remove("OTP_Time_QuenMatKhau");
            }
            catch (Exception)
            {
                TempData["MessageError_QuenMatKhau"] = "Xác thực OTP thất bại! Xảy ra lỗi trong quá trình xử lý.";
                return View("quenMatKhauOTP", reg);
            }
            TempData["MessageSuccess_DangNhap"] = "Lấy lại mật khẩu thành công! Vui lòng đăng nhập với mật khẩu mới.";
            return RedirectToAction("dangNhap");
        }

        public IActionResult dangKy(Register reg)
        {
            reg ??= new Register();
            return View(reg);
        }

        public IActionResult SendOTP(Register reg)
        {
            if (reg == null || string.IsNullOrEmpty(reg.Email) || string.IsNullOrEmpty(reg.Password))
            {
                TempData["MessageError_DangKy"] = "Thông tin đăng ký không hợp lệ!";
                return RedirectToAction("dangKy", reg);
            }

            if (db.NguoiDungs.Any(u => u.Email == reg.Email))
            {
                TempData["MessageError_DangKy"] = "Email đã được đăng ký.";
                return RedirectToAction("dangKy", reg);
            }

            if(db.NguoiDungs.Any(u => u.Sdt == reg.Phone))
            {
                TempData["MessageError_DangKy"] = "Số điện thoại đã được đăng ký.";
                return RedirectToAction("dangKy", reg);
            }

            string otp = new Random().Next(111111, 999999).ToString();
            ViewBag.LastSent = HttpContext.Session.GetString("OTP_LastSent") ?? DateTime.Now.ToString();

            HttpContext.Session.SetString("OTP", otp);
            HttpContext.Session.SetString("OTP_Email", reg.Email);
            HttpContext.Session.SetString("OTP_Time", DateTime.Now.ToString());
            HttpContext.Session.SetString("OTP_LastSent", DateTime.Now.ToString());
            HttpContext.Session.SetInt32("OTP_Count", 0);
            HttpContext.Session.SetString("OTP_Count_ResetTime", DateTime.Now.AddMinutes(30).ToString());

            _emailService.SendOTP(reg.Email, otp);

            TempData["MessageSuccess_OTP"] = "Đã gửi OTP!"; 
            return View(reg);
        }

        public IActionResult ResendOTP(Register reg)
        {
            if (reg == null || string.IsNullOrEmpty(reg.Email) || string.IsNullOrEmpty(reg.Password))
            {
                TempData["MessageError_DangKy"] = "Thông tin đăng ký không hợp lệ!";
                return RedirectToAction("dangKy", reg);
            }

            string? lastSent = HttpContext.Session.GetString("OTP_LastSent");
            string? resetCount = HttpContext.Session.GetString("OTP_Count_ResetTime");
            int count = HttpContext.Session.GetInt32("OTP_Count") ?? 0;

            var now = DateTime.Now;
            if (!string.IsNullOrEmpty(resetCount))
            {
                DateTime resetTime = DateTime.Parse(resetCount);
                if (now > resetTime)
                {
                    count = 0;
                    HttpContext.Session.SetInt32("OTP_Count", 0);
                }
            }

            if (count >= 5)
            {
                string? resetTimeStr = HttpContext.Session.GetString("OTP_Count_ResetTime");
                int minutes = 0;
                if (!string.IsNullOrEmpty(resetTimeStr))
                {
                    DateTime resetTime = DateTime.Parse(resetTimeStr);
                    minutes = (int)(resetTime - DateTime.Now).TotalMinutes;
                }
                TempData["MessageError_OTP"] = "Bạn đã gửi OTP quá nhiều lần! Vui lòng thử lại sau " + minutes + " phút.";
                return View("SendOTP", reg);
            }

            if (!string.IsNullOrEmpty(lastSent))
            {
                DateTime lastSentTime = DateTime.Parse(lastSent);

                if ((DateTime.Now - lastSentTime).TotalSeconds < 30)
                {
                    TempData["MessageError_OTP"] = "Vui lòng đợi 30s!";
                    return View("SendOTP", reg);
                }
            }

            string otp = new Random().Next(100000, 999999).ToString();

            HttpContext.Session.SetString("OTP", otp);
            HttpContext.Session.SetString("OTP_Email", reg.Email);
            HttpContext.Session.SetString("OTP_LastSent", DateTime.Now.ToString());
            HttpContext.Session.SetInt32("OTP_Count", count + 1);

            _emailService.SendOTP(reg.Email, otp);

            TempData["MessageSuccess_OTP"] = "Đã gửi lại OTP!";
            return View("SendOTP", reg);
        }

        [HttpPost]
        public IActionResult VerifyOTP(Register reg, string OTP)
        {
            if (reg == null || string.IsNullOrEmpty(reg.Email) || string.IsNullOrEmpty(reg.Password))
            {
                TempData["MessageError_DangKy"] = "Thông tin đăng ký không hợp lệ!";
                return RedirectToAction("dangKy", reg);
            }

            string? otp = HttpContext.Session.GetString("OTP");
            string? timeStr = HttpContext.Session.GetString("OTP_Time");

            if (otp == null || timeStr == null)
            {
                TempData["MessageError_OTP"] = "OTP không tồn tại!";
                return View("SendOTP", reg);
            }

            DateTime time = DateTime.Parse(timeStr);

            if ((DateTime.Now - time).TotalMinutes > 5)
            {
                TempData["MessageError_OTP"] = "OTP đã hết hạn!";
                return View("SendOTP", reg);
            }
            
            string? email = HttpContext.Session.GetString("OTP_Email");
            if (OTP != otp || reg.Email != email)
            {
                TempData["MessageError_OTP"] = "OTP không hợp lệ!";
                return View("SendOTP", reg);
            }
            TempData["MessageSuccess_OTP"] = "Xác thực OTP thành công!";
            HttpContext.Session.Remove("OTP");
            HttpContext.Session.Remove("OTP_Email");
            HttpContext.Session.Remove("OTP_Time");
            HttpContext.Session.Remove("OTP_LastSent");
            HttpContext.Session.Remove("OTP_Count");
            HttpContext.Session.Remove("OTP_Count_ResetTime");
            return RedirectToAction("hoanThienDangKy", reg);
        }

        public IActionResult hoanThienDangKy(Register reg)
        {
            if(reg == null || string.IsNullOrEmpty(reg.Email) || string.IsNullOrEmpty(reg.Password)) {
                TempData["MessageError_DangKy"] = "Thông tin đăng ký không hợp lệ!";
                return RedirectToAction("dangKy", reg);
            }
            NguoiDung nd = new NguoiDung
            {
                Email = reg.Email,
                MatKhau = BCrypt.Net.BCrypt.HashPassword(reg.Password),
                Phai = true,
                Sdt = reg.Phone,
            };
            return View(nd);
        }

        [HttpPost]
        public IActionResult hoanThienDangKy(NguoiDung nd)
        {
            try
            {
                NguoiDung newND = new NguoiDung
                {
                    Email = nd.Email,
                    MatKhau = nd.MatKhau,
                    MaVt = "customer",
                    HoTen = nd.HoTen,
                    NgaySinh = nd.NgaySinh,
                    Phai = nd.Phai,
                    Sdt = nd.Sdt,
                    DiaChi = "",
                    ChucVu = "Khách hàng",
                    DiemHoiVien = 0,
                    TrangThai = true
                };
                db.NguoiDungs.Add(newND);
                db.SaveChanges();
                TempData["MessageSuccess_DangNhap"] = "Đăng ký thành công! Đăng nhập ngay.";
                return RedirectToAction("dangNhap");
            }
            catch(Exception)
            {
                TempData["MessageError_DangKy"] = "Đăng ký thất bại! Xảy ra lỗi trong quá trình xử lý.";
                return RedirectToAction("dangKy");
            }
        }

        public IActionResult capNhat(int id)
        {
            NguoiDung? nd = db.NguoiDungs.Find(id);
            if(nd == null)
            {
                TempData["MessageError_NguoiDung"] = "Lỗi, người dùng không xác định!";
                return RedirectToAction("Index");
            }
            return View(CNguoiDung.ToCNguoiDung(nd));
        }

        [HttpPost]
        public IActionResult capNhat(CNguoiDung x, IFormFile avatarFile)
        {
            NguoiDung? nd = db.NguoiDungs.Find(x.MaNd);
            if (nd == null)
            {
                TempData["MessageError_CapNhatNguoiDung"] = "Lỗi, người dùng không xác định!";
                return View(x);
            }
            try
            {
                if(avatarFile != null && avatarFile.Length > 0)
                {
                    if (avatarFile.Length > 10 * 1024 * 1024)
                    {
                        TempData["MessageError_CapNhatNguoiDung"] = "Lỗi, kích thước ảnh không được vượt quá 10MB!";
                        return View(x);
                    }
                    if (!new[] { ".jpg", ".jpeg", ".png" }.Contains(Path.GetExtension(avatarFile.FileName).ToLower()))
                    {
                        TempData["MessageError_CapNhatNguoiDung"] = "Lỗi, định dạng ảnh không hợp lệ! Chỉ chấp nhận file .jpg, .jpeg, .png.";
                        return View(x);
                    }
                    string avatarName = "cinemavn-avatar-" + DateTime.Now.Ticks + Path.GetExtension(avatarFile.FileName);
                    string avatarPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatars", avatarName);
                    using (var stream = new FileStream(avatarPath, FileMode.Create))
                    {
                        avatarFile.CopyTo(stream);
                    }
                    nd.Hinh = avatarName;

                    string avatarOldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatars", x.Hinh??"");
                    if (System.IO.File.Exists(avatarOldPath))
                    {
                        System.IO.File.Delete(avatarOldPath);
                    }
                    x.Hinh = avatarName;
                }
                nd.HoTen = x.HoTen;
                nd.DiaChi = x.DiaChi;
                nd.NgaySinh = x.NgaySinh;
                nd.Phai = x.Phai;
                nd.Sdt = x.Sdt;
                db.NguoiDungs.Update(nd);
                db.SaveChanges();
                TempData["MessageSuccess_CapNhatNguoiDung"] = "Cập nhật thông tin thành công!";
                return View(x);
            }
            catch (Exception) {
                TempData["MessageError_CapNhatNguoiDung"] = "Lỗi, không xác định!";
                return View(x);
            }
        }

        public IActionResult xoaAvatar(int id)
        {
            NguoiDung? nd = db.NguoiDungs.Find(id);
            if (nd == null)
            {
                TempData["MessageError_CapNhatNguoiDung"] = "Lỗi, người dùng không xác định!";
                return RedirectToAction("capNhat", new { id = id });
            }
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
                TempData["MessageSuccess_CapNhatNguoiDung"] = "Xóa ảnh thành công!";
                return RedirectToAction("capNhat", new { id = id });
            }
            catch (Exception)
            {
                TempData["MessageError_CapNhatNguoiDung"] = "Lỗi, không xác định!";
                return RedirectToAction("capNhat", new { id = id });
            }
        }

        public IActionResult lichSuVe()
        {
            NguoiDung? nd = db.NguoiDungs.FirstOrDefault(n => n.MaNd == HttpContext.Session.GetInt32("UserId"));
            if (nd == null)
            {
                TempData["MessageError_NguoiDung"] = "Lỗi, người dùng không xác định!";
                return RedirectToAction("Index");
            }
            List<HoaDon> dsHd = db.HoaDons
                .Where(t => t.MaNd == nd.MaNd)
                .Include(t => t.MaCnNavigation)
                .Include(t => t.ChiTietDichVus)
                    .ThenInclude(ct => ct.MaDvNavigation)
                .Include(t => t.Ves)
                    .ThenInclude(v => v.MaScNavigation)
                        .ThenInclude(sc => sc.MaPhimNavigation)
                .Include(t => t.Ves)
                    .ThenInclude(v => v.MaScNavigation)
                        .ThenInclude(sc => sc.MaDdNavigation)
                .OrderByDescending(t => t.MaHd)
                .Take(10)
                .ToList();
            return View(dsHd);
        }

        public IActionResult chiTietVe(int id)
        {
            var Hd = db.HoaDons
                .Where(t => t.MaHd == id)
                .Include(t => t.MaKmNavigation)
                .Include(t => t.MaCnNavigation)
                .Include(t => t.ChiTietDichVus)
                    .ThenInclude(ct => ct.MaDvNavigation)
                .Include(t => t.Ves)
                    .ThenInclude(v => v.MaScNavigation)
                        .ThenInclude(sc => sc.MaPhimNavigation)
                .Include(t => t.Ves)
                    .ThenInclude(v => v.MaScNavigation)
                        .ThenInclude(sc => sc.MaDdNavigation)
                .Include (t => t.Ves)
                    .ThenInclude(v => v.MaGheNavigation)
                        .ThenInclude(mg => mg.MaLgNavigation)
                .FirstOrDefault();
            Hd ??= new HoaDon();
            return PartialView(Hd);
        }

    }
}
