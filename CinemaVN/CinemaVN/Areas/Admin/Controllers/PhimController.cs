using CinemaVN.Models;
using Microsoft.AspNetCore.Mvc;

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
            if(trangthai != -1)
            {
                query = query.Where(t => t.TrangThai == trangthai);
            }
            if(!String.IsNullOrEmpty(keyword))
            {
                query = query.Where(t =>
                    (t.TenPhim != null && t.TenPhim.Contains(keyword)) ||
                    t.MaPhim.ToString() == keyword
                );
            }
            int tongDS = query.Count();
            int kichThuoc = 4;
            var phims = query
                .OrderByDescending(t => t.MaPhim)
                .Skip((trang - 1) * kichThuoc)
                .Take(kichThuoc)
                .ToList();

            ViewBag.trangHT = trang;
            ViewBag.tongTrang = (int)Math.Ceiling((double)tongDS / kichThuoc);
            ViewBag.Keyword = keyword;
            ViewBag.TrangThai = trangthai;

            return View(phims);
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




    }
}
