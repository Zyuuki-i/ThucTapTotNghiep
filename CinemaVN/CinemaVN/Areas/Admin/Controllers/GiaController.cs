using CinemaVN.Models;
using Microsoft.AspNetCore.Mvc;

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

        //SUẤT CHIẾU
        public IActionResult suatChieu()
        {
            return View();
        }
        //END SUẤT CHIẾU

        //DỊCH VỤ
        public IActionResult dichVu()
        {
            return View();
        }
        //END DỊCH VỤ
    }
}
