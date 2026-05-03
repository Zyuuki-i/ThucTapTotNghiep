using CinemaVN.DatModels;
using CinemaVN.Models;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Text.RegularExpressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CinemaVN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class GiaController : Controller
    {
        private readonly CinemaVNContext db = new();
        public IActionResult Index()
        {
            return View();
        }

        //LOẠI GHẾ
        public IActionResult loaiGhe()
        {
            var loaiGhes = db.LoaiGhes.ToList();
            return View(loaiGhes);
        }

        public IActionResult themLoaiGhe(LoaiGhe? loaiGhe)
        {
            loaiGhe ??= new LoaiGhe();
            return View(loaiGhe);
        }

        [HttpPost]
        public IActionResult ThemLoaiGhe(LoaiGhe x)
        {
            if(x == null || x.MaLg == null || String.IsNullOrEmpty(x.TenLg) || x.PhuThu == null)
            {
                TempData["MessageError_ThemLoaiGhe"] = "Vui lòng nhập thông tin loại ghế!";
                return RedirectToAction("themLoaiGhe", x);
            }
            try
            {
                LoaiGhe lg = new LoaiGhe
                {
                    MaLg = x.MaLg,
                    TenLg = x.TenLg,
                    PhuThu = x.PhuThu < 0 ? 0 : x.PhuThu
                };
                db.LoaiGhes.Add(lg);
                db.SaveChanges();
            }catch(Exception)
            {
                TempData["MessageError_LoaiGhe"] = "Lỗi, thêm loại ghế thất bại!";
                return RedirectToAction("loaiGhe");
            }
            TempData["MessageSuccess_LoaiGhe"] = "Thêm loại ghế thành công!";
            return RedirectToAction("loaiGhe");
        }

        public IActionResult xoaLoaiGhe(string id)
        {
            var loaiGhe = db.LoaiGhes.Find(id);
            if(loaiGhe == null)
            {
                TempData["MessageError_LoaiGhe"] = "Lỗi, không tìm thấy loại ghế!";
                return RedirectToAction("loaiGhe");
            }
            try
            {
                db.LoaiGhes.Remove(loaiGhe);
                db.SaveChanges();
                TempData["MessageSuccess_LoaiGhe"] = "Xóa loại ghế thành công!";
                return RedirectToAction("loaiGhe");
            }
            catch (Exception)
            {
                TempData["MessageError_LoaiGhe"] = "Lỗi, xóa loại ghế thất bại!";
                return RedirectToAction("loaiGhe");
            }
        }

        [HttpPost]
        public IActionResult capNhatLoaiGhe(string maLg, string? tenLg, decimal? phuThu)
        {
            var loaiGhe = db.LoaiGhes.Find(maLg);
            if (loaiGhe == null || String.IsNullOrEmpty(tenLg) || phuThu == null)
            {
                return Json(new { success = false });
            }
            try
            {
                loaiGhe.TenLg = tenLg;
                loaiGhe.PhuThu = phuThu;
                db.LoaiGhes.Update(loaiGhe);
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false });
            }
        }
        // END LOẠI GHẾ

        //ĐỊNH DẠNG
        public IActionResult dinhDang()
        {
            var dinhDangs = db.DinhDangs.ToList();
            return View(dinhDangs);
        }

        public IActionResult themDinhDang(DinhDang? dinhDang)
        {
            dinhDang ??= new DinhDang();
            return View(dinhDang);
        }

        [HttpPost]
        public IActionResult ThemDinhDang(DinhDang x)
        {
            if (x == null || x.MaDd == null || String.IsNullOrEmpty(x.TenDd) || x.PhuThu == null)
            {
                TempData["MessageError_ThemDinhDang"] = "Vui lòng nhập thông tin định dạng!";
                return RedirectToAction("themDinhDang", x);
            }
            try
            {
                DinhDang dd = new DinhDang
                {
                    MaDd = x.MaDd,
                    TenDd = x.TenDd,
                    PhuThu = x.PhuThu < 0 ? 0 : x.PhuThu
                };
                db.DinhDangs.Add(dd);
                db.SaveChanges();
            }
            catch (Exception)
            {
                TempData["MessageError_DinhDang"] = "Lỗi, thêm định dạng thất bại!";
                return RedirectToAction("dinhDang");
            }
            TempData["MessageSuccess_DinhDang"] = "Thêm định dạng thành công!";
            return RedirectToAction("dinhDang");
        }

        public IActionResult xoaDinhDang(string id)
        {
            var dinhDang = db.DinhDangs.Find(id);
            if (dinhDang == null)
            {
                TempData["MessageError_DinhDang"] = "Lỗi, không tìm thấy định dạng!";
                return RedirectToAction("dinhDang");
            }
            try
            {
                db.DinhDangs.Remove(dinhDang);
                db.SaveChanges();
                TempData["MessageSuccess_DinhDang"] = "Xóa định dạng thành công!";
                return RedirectToAction("dinhDang");
            }
            catch (Exception)
            {
                TempData["MessageError_DinhDang"] = "Lỗi, xóa định dạng thất bại!";
                return RedirectToAction("dinhDang");
            }
        }

        [HttpPost]
        public IActionResult capNhatDinhDang(string maDd, string? tenDd, decimal? phuThu)
        {
            var dinhDang = db.DinhDangs.Find(maDd);
            if (dinhDang == null || String.IsNullOrEmpty(tenDd) || phuThu == null)
            {
                return Json(new { success = false });
            }
            try
            {
                dinhDang.TenDd = tenDd;
                dinhDang.PhuThu = phuThu;
                db.DinhDangs.Update(dinhDang);
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false });
            }
        }
        //END ĐỊNH DẠNG

        //DỊCH VỤ
        public IActionResult dichVu(string? keyword, int trang = 1, bool sapxep = true)
        {
            keyword = keyword?.Trim();

            var query = db.DichVus.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(t =>
                    (t.TenDv != null && t.TenDv.Contains(keyword)) ||
                    t.MaDv.ToString() == keyword
                );
            }
            int tongDS = query.Count();
            int kichThuoc = 5;
            List<DichVu> dichVus = new List<DichVu>();
            if(sapxep)
            {
                 dichVus = query
                .OrderByDescending(t => t.Gia)
                .Skip((trang - 1) * kichThuoc)
                .Take(kichThuoc)
                .ToList();
            }
            else
            {
                dichVus = query
                .OrderBy(t => t.MaDv)
                .Skip((trang - 1) * kichThuoc)
                .Take(kichThuoc)
                .ToList();
            }


            ViewBag.trangHT = trang;
            ViewBag.tongTrang = (int)Math.Ceiling((double)tongDS / kichThuoc);
            ViewBag.Keyword = keyword;
            ViewBag.SapXep = sapxep;

            List<CDichVu> cDichVus = new();
            foreach (var item in dichVus)
            {
                cDichVus.Add(CDichVu.toCDichVu(item));
            }

            return View(cDichVus);
        }

        public IActionResult xoaDichVu(int id)
        {
            DichVu? dv = db.DichVus.Find(id);
            if (dv != null)
            {
                try
                {
                    string anh = dv.HinhAnh??"";
                    db.DichVus.Remove(dv);
                    db.SaveChanges();
                    string anhPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "snacks", anh);
                    if (System.IO.File.Exists(anhPath))
                    {
                        System.IO.File.Delete(anhPath);
                    }
                    TempData["MessageSuccess_DichVu"] = "Xóa dịch vụ '" + id + "' thành công.";
                    return RedirectToAction("dichVu");
                }catch(Exception)
                {
                    TempData["MessageError_DichVu"] = "Lỗi, không thể xóa!";
                    return RedirectToAction("dichVu");
                }
            }
            TempData["MessageError_DichVu"] = "Lỗi, không tìm thấy dịch vụ!";
            return RedirectToAction("dichVu");
        }

        public IActionResult themDichVu(CDichVu? x)
        {
            x ??= new CDichVu();
            return View(x);
        }

        [HttpPost]
        public IActionResult themDichVu(CDichVu? x, IFormFile file)
        {
            if(x == null)
            {
                TempData["MessageError_ThemDichVu"] = "Lỗi, dữ liệu không hợp lệ!";
                return RedirectToAction("themDichVu",x);
            }

            try
            {
                DichVu dv = new DichVu() { 
                    TenDv = x.TenDv,
                    Gia = x.Gia,
                    LoaiDv = x.LoaiDv,
                };

                if (file != null && file.Length > 0)
                {
                    if (file.Length > 10 * 1024 * 1024)
                    {
                        TempData["MessageError_ThemDichVu"] = "Lỗi, kích thước ảnh không được vượt quá 10MB!";
                        return RedirectToAction("themDichVu", x);
                    }
                    if (!new[] { ".jpg", ".jpeg", ".png" }.Contains(Path.GetExtension(file.FileName).ToLower()))
                    {
                        TempData["MessageError_ThemDichVu"] = "Lỗi, định dạng ảnh không hợp lệ! Chỉ chấp nhận file .jpg, .jpeg, .png.";
                        return RedirectToAction("themDichVu", x);
                    }
                    string snackName = "cinemavn-snacks-" + DateTime.Now.Ticks + Path.GetExtension(file.FileName);
                    string snackPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "snacks", snackName);
                    using (var stream = new FileStream(snackPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                    dv.HinhAnh = snackName;
                }
                db.DichVus.Add(dv);
                db.SaveChanges();

                TempData["MessageSuccess_DichVu"] = "Thêm mới thành công!";
                return RedirectToAction("dichVu");
            }
            catch (Exception)
            {
                TempData["MessageError_ThemDichVu"] = "Lỗi, không thể thêm!";
                return RedirectToAction("themDichVu", x);
            }

        }

        public IActionResult suaDichVu(int id)
        {
            DichVu? dv = db.DichVus.Find(id);
            if (dv != null)
            {
               return View(CDichVu.toCDichVu(dv));
            }
            TempData["MessageError_DichVu"] = "Lỗi, không tìm thấy dịch vụ!";
            return RedirectToAction("dichVu");
        }

        [HttpPost]
        public IActionResult suaDichVu(CDichVu? x, IFormFile file)
        {
            if (x == null)
            {
                TempData["MessageError_SuaDichVu"] = "Lỗi, dữ liệu không hợp lệ!";
                return RedirectToAction("suaDichVu", x);
            }
            DichVu? dv = db.DichVus.Find(x.MaDv);
            if (dv == null)
            {
                TempData["MessageError_DichVu"] = "Lỗi, không tìm thấy dịch vụ!";
                return RedirectToAction("dichVu");
            }
            try
            {
                dv.TenDv = x.TenDv;
                dv.Gia = x.Gia;
                dv.LoaiDv = x.LoaiDv;

                if (file != null && file.Length > 0)
                {
                    if (file.Length > 10 * 1024 * 1024)
                    {
                        TempData["MessageError_SuaDichVu"] = "Lỗi, kích thước ảnh không được vượt quá 10MB!";
                        return RedirectToAction("suaDichVu", x);
                    }
                    if (!new[] { ".jpg", ".jpeg", ".png" }.Contains(Path.GetExtension(file.FileName).ToLower()))
                    {
                        TempData["MessageError_SuaDichVu"] = "Lỗi, định dạng ảnh không hợp lệ! Chỉ chấp nhận file .jpg, .jpeg, .png.";
                        return RedirectToAction("suaDichVu", x);
                    }
                    string snackName = "cinemavn-snacks-" + DateTime.Now.Ticks + Path.GetExtension(file.FileName);
                    string snackPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "snacks", snackName);
                    using (var stream = new FileStream(snackPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    string anhXoa = dv.HinhAnh??"";
                    dv.HinhAnh = snackName;

                    if(anhXoa != "")
                    {
                        string anhPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "snacks", anhXoa);
                        if (System.IO.File.Exists(anhPath))
                        {
                            System.IO.File.Delete(anhPath);
                        }
                    }
                }
                db.DichVus.Update(dv);
                db.SaveChanges();

                TempData["MessageSuccess_DichVu"] = "Cập nhật thành công!";
                return RedirectToAction("dichVu");
            }
            catch (Exception)
            {
                TempData["MessageError_ThemDichVu"] = "Lỗi, không thể cập nhật!";
                return RedirectToAction("suaDichVu", x);
            }

        }

        //END DỊCH VỤ
    }
}
