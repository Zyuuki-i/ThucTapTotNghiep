using CinemaVN.Models;
using Microsoft.AspNetCore.Mvc;

namespace CinemaVN.Controllers
{
    public class ChiNhanhController : Controller
    {
        private CinemaVNContext db = new CinemaVNContext();
        public IActionResult Index()
        {
            List<ChiNhanh> dsCN = db.ChiNhanhs.ToList();
            return View(dsCN);
        }
    }
}
