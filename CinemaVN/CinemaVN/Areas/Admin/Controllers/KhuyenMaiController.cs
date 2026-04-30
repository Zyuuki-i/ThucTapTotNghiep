using CinemaVN.DatModels;
using CinemaVN.Models;
using Microsoft.AspNetCore.Mvc;

namespace CinemaVN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class KhuyenMaiController : Controller
    {
        private readonly CinemaVNContext db = new();
        public IActionResult Index(string? keyword, int trang = 1, int trangthai = 1)
        {
            var khuyenMais = db.KhuyenMais.AsQueryable();
            ViewBag.demSapDienRa = khuyenMais.Count(k => k.TrangThai == 0);
            ViewBag.demHieuLuc = khuyenMais.Count(k => k.TrangThai == 1);
            ViewBag.demHetHan = khuyenMais.Count(k => k.TrangThai == 2);

            if(trangthai == 0 || trangthai == 1 || trangthai == 2)
                khuyenMais = khuyenMais.Where(k => k.TrangThai == trangthai);

            if (!string.IsNullOrEmpty(keyword))
            {
                khuyenMais = khuyenMais.Where(k => (k.Code != null && k.Code.Contains(keyword)) || k.MaKm.ToString().Contains(keyword) || (k.MoTa != null && k.MoTa.Contains(keyword)));
            }

            int kichThuoc = 5;
            int tongTrang = (int)Math.Ceiling((double)khuyenMais.Count() / kichThuoc);
            int skip = (trang - 1) * kichThuoc;
            khuyenMais = khuyenMais.Skip(skip).Take(kichThuoc);

            ViewBag.trangHT = trang;
            ViewBag.tongTrang = tongTrang;
            ViewBag.Keyword = keyword;
            ViewBag.TrangThai = trangthai;

            List<CKhuyenMai> cKhuyenMais = new List<CKhuyenMai>();
            foreach (var item in khuyenMais)
            {
                cKhuyenMais.Add(CKhuyenMai.ToCKhuyenMai(item));
            }

            return View(cKhuyenMais);
        }

        private int timTrang(int id)
        {
            KhuyenMai? km = db.KhuyenMais.Find(id);
            if (km == null)
            {
                return 1;
            }
            var khuyenMais = db.KhuyenMais.Where(k => k.TrangThai == km.TrangThai).ToList();
            int trang = 1;
            foreach (var item in khuyenMais)
            {
                if (item.MaKm == id)
                {
                    break;
                }
                trang++;
            }
            return (int)Math.Ceiling((double)trang / 5);
        }

        public IActionResult doiTrangThai(int id, int trangthai)
        {
            KhuyenMai? km = db.KhuyenMais.Find(id);
            if (km == null)
            {
                TempData["MessageError_KhuyenMai"] = "Lỗi, không tìm thấy khuyến mãi";
                return RedirectToAction("Index", new {trang=timTrang(id), trangthai = trangthai});
            }
            try
            {
                km.TrangThai = trangthai;
                db.SaveChanges();
            }catch (Exception)
            {
                TempData["MessageError_KhuyenMai"] = "Lỗi, không thể đổi trạng thái khuyến mãi";
                return RedirectToAction("Index", new { trang = timTrang(id), trangthai = trangthai });
            }
            return RedirectToAction("Index", new { trang = timTrang(id), trangthai = trangthai });
        }




        
    }
}
