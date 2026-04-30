using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaVN.Models;

namespace CinemaVN.Areas.Manage.Controllers
{
    [Area("Manage")]
    public class DichVuController : Controller
    {
        private readonly CinemaVNContext _db;

        public DichVuController(CinemaVNContext db)
        {
            _db = db;
        }
        private NguoiDung GetCurrentUser()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            return _db.NguoiDungs.Include(u => u.MaCnNavigation).FirstOrDefault(u => u.MaNd == userId);
        }

        public IActionResult Index()
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null) return RedirectToAction("Login", "Home", new { area = "" });
            var danhSachDichVu = _db.Khos
                .Include(k => k.MaDvNavigation) 
                .Where(k => k.MaCn == currentUser.MaCn)
                .OrderByDescending(k => k.SoLuongTon) 
                .ToList();

            ViewBag.TenChiNhanh = currentUser.MaCnNavigation?.TenCn;

            return View(danhSachDichVu);
        }
        public IActionResult Them(int? maDV) 
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null) return RedirectToAction("Login", "Home", new { area = "" });

            ViewBag.TenChiNhanh = currentUser.MaCnNavigation?.TenCn;
            ViewBag.DanhSachDichVu = _db.DichVus.OrderBy(d => d.TenDv).ToList();
            ViewBag.SelectedMaDv = maDV;

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Them(int MaDv, int SoLuongNhap)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null) return RedirectToAction("Login", "Home", new { area = "" });

            if (SoLuongNhap <= 0)
            {
                TempData["MessageError_NV"] = "Số lượng nhập phải lớn hơn 0!";
                return RedirectToAction("Them");
            }

            try
            {
                var khoHienTai = _db.Khos.FirstOrDefault(k => k.MaCn == currentUser.MaCn && k.MaDv == MaDv);

                if (khoHienTai != null)
                {
                    khoHienTai.SoLuongTon = (khoHienTai.SoLuongTon ?? 0) + SoLuongNhap;
                    _db.Khos.Update(khoHienTai);
                }
                else
                {
                    Kho khoMoi = new Kho
                    {
                        MaCn = currentUser.MaCn,
                        MaDv = MaDv,
                        SoLuongTon = SoLuongNhap
                    };
                    _db.Khos.Add(khoMoi);
                }

                await _db.SaveChangesAsync();

                TempData["MessageSuccess_NV"] = $"Đã nhập thêm {SoLuongNhap} sản phẩm vào kho thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                string realError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                TempData["MessageError_NV"] = "Lỗi hệ thống: " + realError;
                return RedirectToAction("Them");
            }
        }
    }
}