using CinemaVN.Models;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.Text.RegularExpressions;

namespace CinemaVN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BannerController : Controller
    {
        private readonly CinemaVNContext db = new();
        public IActionResult Index(string? keyword, bool trangthai = true, int trang = 1)
        {
            keyword = keyword?.Trim();

            var query = db.Banners.AsQueryable();

            query = query.Where(t => t.TrangThai == trangthai);

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(t =>
                    (t.TieuDe != null && t.TieuDe.Contains(keyword)) ||
                    t.MaBn.ToString() == keyword
                );
            }
            int tongDS = query.Count();
            int kichThuoc = 5;
            var banners = query
                .OrderByDescending(t => t.MaBn)
                .Skip((trang - 1) * kichThuoc)
                .Take(kichThuoc)
                .ToList();

            ViewBag.trangHT = trang;
            ViewBag.tongTrang = (int)Math.Ceiling((double)tongDS / kichThuoc);
            ViewBag.Keyword = keyword;
            ViewBag.TrangThai = trangthai;

            return View(banners);
        }

        public IActionResult voHieu(int id)
        {
            Banner? banner = db.Banners.Find(id);
            if (banner != null)
            {
                banner.TrangThai = false;
                db.SaveChanges();
            }
            return RedirectToAction("Index", new { trangthai = true });
        }
        public IActionResult kichHoat(int id)
        {
            Banner? banner = db.Banners.Find(id);
            if (banner != null)
            {
                banner.TrangThai = true;
                db.SaveChanges();
            }
            return RedirectToAction("Index", new { trangthai = false });
        }

        public IActionResult them()
        {
            return View();
        }

        [HttpPost]
        public IActionResult them(Banner banner, IFormFile file)
        {
            if (file == null || file.Length == 0 ||
                !(file.ContentType == "image/jpeg" || file.ContentType == "image/png" || file.ContentType == "image/jpg"))
            {
                TempData["MessageError_ThemBanner"] = "Vui lòng chọn một tệp hình ảnh hợp lệ.";
                return View(banner);
            }

            using (var stream = file.OpenReadStream())
            using (var image = Image.FromStream(stream))
            {
                double ratio = (double)image.Width / image.Height;
                if (ratio < 1.6 || ratio > 1.9)
                {
                    TempData["MessageError_ThemBanner"] = "Ảnh phải có tỷ lệ gần 16:9 (ví dụ: 1920x1080)";
                    return View(banner);
                }
            }

            string thuMuc = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "banner");

            string? fileName = banner.HinhAnh?.Trim().ToLower();
            if (string.IsNullOrEmpty(fileName) || !Regex.IsMatch(fileName, @"^[a-zA-Z0-9_-]+$"))
            {
                TempData["MessageError_ThemBanner"] = "Tên file không hợp lệ.";
                return View(banner);
            }

            string extension = Path.GetExtension(file.FileName).ToLower();
            string anhMoi = fileName + extension;

            string duongDan = Path.Combine(thuMuc, anhMoi);

            if (db.Banners.Any(b => b.HinhAnh.ToLower() == anhMoi) || System.IO.File.Exists(duongDan))
            {
                TempData["MessageError_ThemBanner"] = "Tên file đã tồn tại!";
                return View(banner);
            }

            using (FileStream f = new FileStream(duongDan, FileMode.Create))
            {
                file.CopyTo(f);
            }

            banner.HinhAnh = anhMoi;
            banner.TrangThai = true;

            db.Banners.Add(banner);
            db.SaveChanges();

            TempData["MessageSuccess_Banner"] = "Thêm banner thành công";
            return RedirectToAction("Index", new { trangthai = true });
        }

        public IActionResult sua(int id)
        {
            Banner? banner = db.Banners.Find(id);
            if (banner != null)
            {
                ViewBag.PhanMoRong = Path.GetExtension(banner.HinhAnh);
                banner.HinhAnh = Path.GetFileNameWithoutExtension(banner.HinhAnh);
                return View(banner);
            }
            TempData["MessageError_Banner"] = "Không tìm thấy banner.";
            return RedirectToAction("Index", new { trangthai = true });
        }

        [HttpPost]
        public IActionResult sua(Banner banner, IFormFile? file)
        {
            Banner? bn = db.Banners.Find(banner.MaBn);
            if (bn == null)
            {
                TempData["MessageError_Banner"] = "Không tìm thấy banner.";
                return RedirectToAction("Index", new { trangthai = true });
            }

            bn.TieuDe = banner.TieuDe;
            bn.DuongDan = banner.DuongDan;

            if (file != null && file.Length > 0)
            {
                if (!(file.ContentType == "image/jpeg" || file.ContentType == "image/png" || file.ContentType == "image/jpg"))
                {
                    ViewBag.PhanMoRong = Path.GetExtension(bn.HinhAnh);
                    banner.HinhAnh = Path.GetFileNameWithoutExtension(bn.HinhAnh);
                    TempData["MessageError_SuaBanner"] = "Vui lòng chọn một tệp hình ảnh hợp lệ.";
                    return View(banner);
                }

                using (var stream = file.OpenReadStream())
                using (var image = Image.FromStream(stream))
                {
                    double ratio = (double)image.Width / image.Height;
                    if (ratio < 1.6 || ratio > 1.9)
                    {
                        ViewBag.PhanMoRong = Path.GetExtension(bn.HinhAnh);
                        banner.HinhAnh = Path.GetFileNameWithoutExtension(bn.HinhAnh);
                        TempData["MessageError_SuaBanner"] = "Ảnh phải có tỷ lệ gần 16:9 (ví dụ: 1920x1080)";
                        return View(banner);
                    }
                }

                string thuMuc = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "banner");

                string? fileName = banner.HinhAnh?.Trim().ToLower();
                if (string.IsNullOrEmpty(fileName) || !Regex.IsMatch(fileName, @"^[a-zA-Z0-9_-]+$"))
                {
                    ViewBag.PhanMoRong = Path.GetExtension(bn.HinhAnh);
                    banner.HinhAnh = Path.GetFileNameWithoutExtension(bn.HinhAnh);
                    TempData["MessageError_SuaBanner"] = "Tên file không hợp lệ.";
                    return View(banner);
                }

                string extension = Path.GetExtension(file.FileName).ToLower();
                string anhMoi = fileName + extension;

                string duongDan = Path.Combine(thuMuc, anhMoi);

                string anhXoa = bn.HinhAnh;

                bool trungDB = db.Banners.Any(b =>
                    b.HinhAnh.ToLower() == anhMoi
                    && b.MaBn != banner.MaBn);

                bool trungFile = System.IO.File.Exists(duongDan);

                // Kiểm tra nếu file tồn tại nhưng KHÔNG phải ảnh cũ của banner đang sửa thì mới báo lỗi
                bool fileKhac = trungFile && anhXoa.ToLower() != anhMoi;

                if (trungDB || fileKhac)
                {
                    ViewBag.PhanMoRong = Path.GetExtension(bn.HinhAnh);
                    banner.HinhAnh = Path.GetFileNameWithoutExtension(bn.HinhAnh);
                    TempData["MessageError_SuaBanner"] = "Tên file đã tồn tại!";
                    return View(banner);
                }

                using (FileStream f = new FileStream(duongDan, FileMode.Create))
                {
                    file.CopyTo(f);
                }

                bn.HinhAnh = anhMoi;

                if (!string.IsNullOrEmpty(anhXoa))
                {
                    string oldPath = Path.Combine(thuMuc, anhXoa);
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }
            }

            db.SaveChanges();
            TempData["MessageSuccess_Banner"] = "Cập nhật banner thành công";
            return RedirectToAction("Index", new { trangthai = true });
        }
    }
}
