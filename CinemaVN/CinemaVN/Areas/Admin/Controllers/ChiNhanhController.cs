using CinemaVN.DatModels;
using CinemaVN.Models;
using Microsoft.AspNetCore.Mvc;

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
    }
}
