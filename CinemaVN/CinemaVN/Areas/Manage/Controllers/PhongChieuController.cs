using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaVN.Models; 

namespace CinemaVN.Areas.Manage.Controllers
{
    [Area("Manage")]
    public class PhongChieuController : Controller
    {
        private readonly CinemaVNContext _db;

        public PhongChieuController(CinemaVNContext db)
        {
            _db = db;
        }
        private NguoiDung GetCurrentUser()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            return _db.NguoiDungs.FirstOrDefault(u => u.MaNd == userId);
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var user = _db.NguoiDungs
                          .Include(u => u.MaCnNavigation)
                          .FirstOrDefault(u => u.MaNd == userId);

            if (user == null) return RedirectToAction("dangNhap", "Home", new { area = "" });

            ViewBag.TenChiNhanh = user.MaCnNavigation?.TenCn ?? "Không xác định";

            var dsPhong = _db.PhongChieus
                             .Where(p => p.MaCn == user.MaCn)
                             .ToList();

            return View(dsPhong);
        }

        public IActionResult Them()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Them(PhongChieu model)
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("dangNhap", "Home", new { area = "" });

        
            bool isExist = _db.PhongChieus.Any(p => p.TenPc == model.TenPc && p.MaCn == user.MaCn);

            if (isExist)
            {
                ModelState.AddModelError("TenPc", "Tên phòng chiếu này đã tồn tại trong chi nhánh của bạn.");
            }

            if (ModelState.IsValid)
            {
                model.MaCn = user.MaCn;
                _db.PhongChieus.Add(model);
                _db.SaveChanges();
                TempData["MessageSuccess_PhongChieu"] = "Thêm phòng chiếu thành công!";
                return RedirectToAction("Index");
            }

            return View(model);
        }

        public IActionResult Sua(int id)
        {
            var user = GetCurrentUser();
            var phong = _db.PhongChieus.FirstOrDefault(p => p.MaPc == id && p.MaCn == user.MaCn);

            if (phong == null) return NotFound();
            return View(phong);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Sua(PhongChieu model)
        {
            var user = GetCurrentUser();
            if (ModelState.IsValid && user != null)
            {
                var existing = _db.PhongChieus.AsNoTracking().FirstOrDefault(p => p.MaPc == model.MaPc && p.MaCn == user.MaCn);
                if (existing != null)
                {
                    model.MaCn = user.MaCn;
                    _db.Update(model);
                    _db.SaveChanges();
                    TempData["MessageSuccess_PhongChieu"] = "Cập nhật thành công!";
                    return RedirectToAction("Index");
                }
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult Xoa(int id)
        {
            var user = GetCurrentUser();
            var phong = _db.PhongChieus.FirstOrDefault(p => p.MaPc == id && p.MaCn == user.MaCn);

            if (phong != null)
            {
                try
                {
                    _db.PhongChieus.Remove(phong);
                    _db.SaveChanges();
                    TempData["MessageSuccess_PhongChieu"] = "Đã xóa phòng chiếu.";
                }
                catch
                {
                    TempData["MessageError_PhongChieu"] = "Không thể xóa phòng này (có thể đã có suất chiếu).";
                }
            }
            return RedirectToAction("Index");
        }
    }
}