using CinemaVN.Models;
using CinemaVN.DatModels;
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

            List<CBanner> cbanners = new();
            foreach (var item in banners)
            {
                cbanners.Add(CBanner.ToCBanner(item));
            }

            return View(cbanners);
        }

        public IActionResult doiTrangThai(int id, bool trangthai)
        {
            Banner? banner = db.Banners.Find(id);
            if (banner == null) {
                TempData["MessageError_Banner"] = "Lỗi, không tìm thấy banner!";
                return RedirectToAction("Index", new { trangthai = trangthai });
            }
            try { 
                banner.TrangThai = trangthai;
                db.SaveChanges();
                
            }catch (Exception)
            {
                TempData["MessageError_Banner"] = "Lỗi, không thể cập nhật trạng thái!";
                return RedirectToAction("Index", new { trangthai = trangthai });
            }
            int trang = timTrang(id);
            return RedirectToAction("Index", new { trangthai = trangthai, trang = trang });
        }

        private int timTrang(int id)
        {
            Banner? banner = db.Banners.Find(id);
            if (banner == null) return 1;

            int kichThuoc = 5;

            int soLuongTruoc = db.Banners
                .Where(t => t.TrangThai == banner.TrangThai && t.MaBn > banner.MaBn)
                .Count();

            return (soLuongTruoc / kichThuoc) + 1;
        }

        public IActionResult xoa(int id)
        {
            Banner? banner = db.Banners.Find(id);
            if (banner != null)
            {
                try
                {
                    db.Banners.Remove(banner);
                    db.SaveChanges();
                }
                catch (Exception)
                {
                    TempData["MessageError_Banner"] = "Lỗi, không thể xóa banner!";
                    return RedirectToAction("Index", new { trangthai = banner.TrangThai });
                }
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

            string thuMuc = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "banners");

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
            TempData["MessageError_Banner"] = "Lỗi, không tìm thấy banner!";
            return RedirectToAction("Index", new { trangthai = true });
        }

        [HttpPost]
        public IActionResult sua(Banner banner, IFormFile? file)
        {
            Banner? bn = db.Banners.Find(banner.MaBn);
            if (bn == null)
            {
                TempData["MessageError_Banner"] = "Lỗi, không tìm thấy banner!";
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

                string thuMuc = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "banners");

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
                try
                {
                    bn.HinhAnh = anhMoi;
                    db.SaveChanges();
                }
                catch (Exception)
                {
                    ViewBag.PhanMoRong = Path.GetExtension(bn.HinhAnh);
                    banner.HinhAnh = Path.GetFileNameWithoutExtension(bn.HinhAnh);
                    TempData["MessageError_SuaBanner"] = "Lỗi, không thể cập nhật banner!";
                    return View(banner);
                }

                using (FileStream f = new FileStream(duongDan, FileMode.Create))
                {
                    file.CopyTo(f);
                }

                if (!string.IsNullOrEmpty(anhXoa))
                {
                    string oldPath = Path.Combine(thuMuc, anhXoa);
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }
            }
            TempData["MessageSuccess_Banner"] = "Cập nhật banner thành công";
            return RedirectToAction("Index", new { trangthai = true });
        }
    }
}
