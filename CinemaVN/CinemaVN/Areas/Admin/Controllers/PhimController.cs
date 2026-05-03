using CinemaVN.Models;
using CinemaVN.DatModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaVN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PhimController : Controller
    {
        private readonly CinemaVNContext db = new();
        public IActionResult Index(string? keyword, int trangthai = 1, int trang = 1)
        {
            keyword = keyword?.Trim();
            var query = db.Phims.AsQueryable();
            if (trangthai == 0 || trangthai == 1 || trangthai == 2)
            {
                query = query.Where(t => t.TrangThai == trangthai);
            }
            if (!String.IsNullOrEmpty(keyword))
            {
                query = query.Where(t =>
                    (t.TenPhim != null && t.TenPhim.Contains(keyword)) ||
                    t.MaPhim.ToString() == keyword
                );
            }
            int tongDS = query.Count();
            int kichThuoc = 5;
            var phims = query
                .OrderByDescending(t => t.MaPhim)
                .Skip((trang - 1) * kichThuoc)
                .Take(kichThuoc)
                .ToList();

            ViewBag.trangHT = trang;
            ViewBag.tongTrang = (int)Math.Ceiling((double)tongDS / kichThuoc);
            ViewBag.Keyword = keyword;
            ViewBag.TrangThai = trangthai;

            List<CPhim> cphims = new();
            foreach (var item in phims)
            {
                cphims.Add(CPhim.ToCPhim(item));
            }

            return View(cphims);
        }

        private int timTrang(int id)
        {
            Phim? phim = db.Phims.Find(id);
            if (phim == null) return 1;

            int kichThuoc = 5;

            int soLuongTruoc = db.Phims
                .Where(t => t.TrangThai == phim.TrangThai && t.MaPhim > phim.MaPhim)
                .Count();

            return (soLuongTruoc / kichThuoc) + 1;
        }

        public IActionResult doiTrangThai(int id, int trangthai)
        {
            Phim? phim = db.Phims.Find(id);
            if (phim == null)
            {
                TempData["MessageError_Phim"] = "Lỗi, không tìm thấy phim!";
                return RedirectToAction("Index");
            }
            try
            {
                phim.TrangThai = trangthai;
                db.Phims.Update(phim);
                db.SaveChanges();
                TempData["MessageSuccess_Phim"] = "Cập nhật trạng thái thành công!";
                int trang = timTrang(id);
                return RedirectToAction("Index", new { trangthai = trangthai, trang = trang });
            }
            catch (Exception)
            {
                TempData["MessageError_Phim"] = "Lỗi, không thể cập nhật trạng thái!";
                return RedirectToAction("Index");
            }
        }

        public IActionResult xoa(int id)
        {
            Phim? phim = db.Phims.Include(p => p.MaTls).FirstOrDefault(p => p.MaPhim == id);
            if (phim == null)
            {
                TempData["MessageError_Phim"] = "Lỗi, không tìm thấy phim!";
                return RedirectToAction("Index");
            }
            try
            {
                string anh = phim.Poster??"";
                phim.MaTls.Clear();
                db.Phims.Remove(phim);
                db.SaveChanges();
                string posterPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "posters", anh);
                if (System.IO.File.Exists(posterPath))
                {
                    System.IO.File.Delete(posterPath);
                }
                TempData["MessageSuccess_Phim"] = "Xóa phim thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["MessageError_Phim"] = "Lỗi, không thể xóa phim này!";
                return RedirectToAction("Index");
            }
        }

        public IActionResult chiTiet(int id)
        {
            Phim? phim = db.Phims.Find(id);
            if (phim == null)
            {
                TempData["MessageError_Phim"] = "Lỗi, không tìm thấy phim!";
                return RedirectToAction("Index");
            }
            var theLoais = db.TheLoais.Where(t => t.MaPhims.Any(p => p.MaPhim == id)).ToList();
            ViewBag.TheLoais = string.Join(", ", theLoais.Select(t => t.TenTl));
            return View(CPhim.ToCPhim(phim));
        }

        public IActionResult xoaAnh(int id)
        {
            Phim? phim = db.Phims.Find(id);
            if (phim == null)
            {
                TempData["MessageError_Phim"] = "Lỗi, không tìm thấy phim!";
                return RedirectToAction("Index");
            }
            try
            {
                string posterPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "posters", phim.Poster ?? "");
                if (System.IO.File.Exists(posterPath))
                {
                    System.IO.File.Delete(posterPath);
                }
                phim.Poster = null;
                db.Phims.Update(phim);
                db.SaveChanges();
                return RedirectToAction("chiTiet", id);
            }
            catch (Exception)
            {
                TempData["MessageError_Phim"] = "Lỗi, không thể xóa poster này!";
                return RedirectToAction("Index");
            }
        }

        public IActionResult them(CPhim x)
        {
            ViewBag.TheLoais = db.TheLoais.OrderByDescending(t => t.TenTl).ToList();
            x ??= new CPhim();
            return View(x);
        }

        [HttpPost]
        public IActionResult them(CPhim x, IFormFile poster, List<int> theLoaiChon)
        {
            try
            {
                if(theLoaiChon == null || theLoaiChon.Count == 0)
                {
                    TempData["MessageError_ThemPhim"] = "Lỗi, vui lòng chọn ít nhất một thể loại!";
                    return RedirectToAction("them", x);
                }
                Phim phim = new Phim
                {
                    TenPhim = x.TenPhim,
                    MoTa = x.MoTa,
                    ThoiLuong = x.ThoiLuong,
                    NgayChieu = x.NgayChieu,
                    DaoDien = x.DaoDien,
                    Trailer = x.Trailer,
                    DoTuoi = x.DoTuoi,
                    TrangThai = x.NgayChieu > DateTime.Now ? 0 : 1
                };
                if (poster != null && poster.Length > 0)
                {
                    if (poster.Length > 10 * 1024 * 1024)
                    {
                        TempData["MessageError_ThemPhim"] = "Lỗi, kích thước poster không được vượt quá 10MB!";
                        return RedirectToAction("them", x);
                    }
                    if (!new[] { ".jpg", ".jpeg", ".png" }.Contains(Path.GetExtension(poster.FileName).ToLower()))
                    {
                        TempData["MessageError_ThemPhim"] = "Lỗi, định dạng poster không hợp lệ! Chỉ chấp nhận file .jpg, .jpeg, .png.";
                        return RedirectToAction("them", x);
                    }
                    string posterName = "cinemavn-poster-" + DateTime.Now.Ticks + Path.GetExtension(poster.FileName);
                    string posterPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "posters", posterName);
                    using (var stream = new FileStream(posterPath, FileMode.Create))
                    {
                        poster.CopyTo(stream);
                    }
                    phim.Poster = posterName;
                }
                phim.MaTls = db.TheLoais.Where(t => theLoaiChon.Contains(t.MaTl)).ToList();
                db.Phims.Add(phim);
                db.SaveChanges();
                TempData["MessageSuccess_Phim"] = "Thêm phim thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["MessageError_ThemPhim"] = "Lỗi, không thể thêm phim!";
                return RedirectToAction("them", x);
            }
        }

        public IActionResult sua(int id)
        {
            Phim? phim = db.Phims.Include(p => p.MaTls).FirstOrDefault(p => p.MaPhim == id);
            if (phim == null)
            {
                TempData["MessageError_Phim"] = "Lỗi, không tìm thấy phim!";
                return RedirectToAction("Index");
            }
            ViewBag.TheLoais = db.TheLoais.OrderByDescending(t => t.TenTl).ToList();
            ViewBag.theLoaiChon = phim.MaTls.Select(t => t.MaTl).ToList();
            return View(CPhim.ToCPhim(phim));
        }

        [HttpPost]
        public IActionResult sua(CPhim x, IFormFile poster, List<int> theLoaiChon)
        {
            try
            {
                Phim? phim = db.Phims.Include(p => p.MaTls).FirstOrDefault(p => p.MaPhim == x.MaPhim);
                if (phim == null)
                {
                    TempData["MessageError_Phim"] = "Lỗi, không tìm thấy phim!";
                    return RedirectToAction("Index");
                }
                if (theLoaiChon == null || theLoaiChon.Count == 0)
                {
                    TempData["MessageError_SuaPhim"] = "Lỗi, vui lòng chọn ít nhất một thể loại!";
                    return RedirectToAction("sua", new { id = x.MaPhim });
                }
                phim.TenPhim = x.TenPhim;
                phim.MoTa = x.MoTa;
                phim.ThoiLuong = x.ThoiLuong;
                phim.NgayChieu = x.NgayChieu;
                phim.DaoDien = x.DaoDien;
                phim.Trailer = x.Trailer;
                phim.DoTuoi = x.DoTuoi;
                if (poster != null && poster.Length > 0 && phim.Poster != poster.FileName)
                {
                    if (poster.Length > 10 * 1024 * 1024)
                    {
                        TempData["MessageError_SuaPhim"] = "Lỗi, kích thước poster không được vượt quá 10MB!";
                        return RedirectToAction("sua", new { id = x.MaPhim });
                    }
                    if (!new[] { ".jpg", ".jpeg", ".png" }.Contains(Path.GetExtension(poster.FileName).ToLower()))
                    {
                        TempData["MessageError_SuaPhim"] = "Lỗi, định dạng poster không hợp lệ! Chỉ chấp nhận file .jpg, .jpeg, .png.";
                        return RedirectToAction("sua", new { id = x.MaPhim });
                    }
                    string posterName = "cinemavn-poster-" + DateTime.Now.Ticks + Path.GetExtension(poster.FileName);
                    string posterPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "posters", posterName);
                    using (var stream = new FileStream(posterPath, FileMode.Create))
                    {
                        poster.CopyTo(stream);
                    }
                    if (!string.IsNullOrEmpty(posterPath))
                    {
                        string oldPosterPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "poster", phim.Poster ?? "");
                        if (System.IO.File.Exists(oldPosterPath))
                        {
                            System.IO.File.Delete(oldPosterPath);
                        }
                    }
                    phim.Poster = posterName;
                }
                phim.MaTls = db.TheLoais.Where(t => theLoaiChon.Contains(t.MaTl)).ToList();
                db.Phims.Update(phim);
                db.SaveChanges();
                TempData["MessageSuccess_Phim"] = "Cập nhật phim thành công!";
                int trang = timTrang(x.MaPhim);
                return RedirectToAction("Index", new { trang = trang });
            }
            catch (Exception)
            {
                TempData["MessageError_SuaPhim"] = "Lỗi, không thể cập nhật phim!";
                return RedirectToAction("sua", new { id = x.MaPhim });
            }
        }
    }
}
