using CinemaVN.Models;
using Microsoft.AspNetCore.Mvc;

namespace CinemaVN.Controllers
{
    public class KhuyenMaiController : Controller
    {
        private CinemaVNContext db = new CinemaVNContext();
        public IActionResult Index()
        {
            List<KhuyenMai> dsKm = db.KhuyenMais
                                     .Where(t => t.TrangThai != 2)
                                     .OrderByDescending(t => t.TrangThai)
                                     .ThenByDescending(t => t.NgayBd)
                                     .ToList();

            return View(dsKm);
        }
    }
}
