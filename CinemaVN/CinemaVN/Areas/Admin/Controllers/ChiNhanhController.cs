using CinemaVN.DatModels;
using CinemaVN.Models;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace CinemaVN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ChiNhanhController : Controller
    {
        private readonly CinemaVNContext db = new();
        public IActionResult Index(string? keyword, int trang = 1)
        {
            var chiNhanhs = db.ChiNhanhs.AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
            {
                chiNhanhs = chiNhanhs.Where(c => (c.TenCn != null && c.TenCn.Contains(keyword)) || c.MaCn.Contains(keyword) || (c.DiaChi != null && c.DiaChi.Contains(keyword)));
            }
            int kichThuoc = 5;
            int soTrang = (int)Math.Ceiling((double)chiNhanhs.Count()/ kichThuoc);
            ViewBag.tongTrang = soTrang;
            ViewBag.trangHT = trang;
            ViewBag.Keyword = keyword;

            chiNhanhs = chiNhanhs.Skip((trang - 1) * kichThuoc).Take(kichThuoc);
            List<CChiNhanh> cChiNhanhs = new();
            foreach (var item in chiNhanhs)
            {
                cChiNhanhs.Add(CChiNhanh.ToCChiNhanh(item));
            }
            return View(cChiNhanhs);
        }

        public IActionResult xoa(string id)
        {
            ChiNhanh? cn = db.ChiNhanhs.Find(id);
            if (cn == null)
            {
                TempData["MessageError_ChiNhanh"] = "Lỗi, không tìm thấy chi nhánh!";
                return RedirectToAction("Index");
            }
            try
            {
                db.ChiNhanhs.Remove(cn);
                db.SaveChanges();
                TempData["MessageSuccess_ChiNhanh"] = "Xóa chi nhánh '"+ id +"' thành công!";
                return RedirectToAction("Index");
            }catch(Exception)
            {
                TempData["MessageError_ChiNhanh"] = "Lỗi, không thể xóa chi nhánh này!";
                return RedirectToAction("Index");
            }
        }

        public IActionResult them(CChiNhanh? x)
        {
            x ??= new CChiNhanh();
            return View(x);
        }

        [HttpPost]
        public IActionResult themCN(CChiNhanh? x)
        {
            if(x == null)
            {
                TempData["MessageError_ThemChiNhanh"] = "Lỗi, dữ liệu không hợp lệ!";
                return RedirectToAction("them",x);
            }
            try
            {
                ChiNhanh cn = new ChiNhanh()
                {
                    MaCn = x.MaCn,
                    TenCn = x.TenCn,
                    DiaChi = x.DiaChi,
                    Sdt = x.Sdt
                };
                db.ChiNhanhs.Add(cn);
                db.SaveChanges();
                TempData["MessageSuccess_ChiNhanh"] = "Thêm mới thành công!";
                return RedirectToAction("Index");
            }
            catch(Exception)
            {
                TempData["MessageError_ThemChiNhanh"] = "Lỗi, không thể thêm mới!";
                return RedirectToAction("them", x);
            }
        }

        public IActionResult sua(string id)
        {
            ChiNhanh? cn = db.ChiNhanhs.Find(id);
            if(cn == null)
            {
                TempData["MessageError_ChiNhanh"] = "Lỗi, không tìm thấy chi nhánh!";
                return RedirectToAction("Index");
            }
            return View(CChiNhanh.ToCChiNhanh(cn));
        }

        [HttpPost]
        public IActionResult suaCN(CChiNhanh? x)
        {
            ChiNhanh? cn;
            if (x == null || x.MaCn == null)
            {
                TempData["MessageError_SuaChiNhanh"] = "Lỗi, dữ liệu không hợp lệ!";
                return RedirectToAction("sua", x?.MaCn);
            }
            else
                cn = db.ChiNhanhs.Find(x.MaCn);
            if (cn != null)
            {
                try
                {
                    cn.TenCn = x.TenCn;
                    cn.DiaChi = x.DiaChi;
                    cn.Sdt = x.Sdt;
                    db.ChiNhanhs.Update(cn);
                    db.SaveChanges();
                    TempData["MessageSuccess_ChiNhanh"] = "Cập nhật thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    TempData["MessageError_SuaChiNhanh"] = "Lỗi, không thể sửa!";
                    return RedirectToAction("sua", cn.MaCn);
                }
            }
            TempData["MessageError_ChiNhanh"] = "Lỗi, không tìm thấy chi nhánh!";
            return RedirectToAction("Index");
        }







            //end
    }
}
