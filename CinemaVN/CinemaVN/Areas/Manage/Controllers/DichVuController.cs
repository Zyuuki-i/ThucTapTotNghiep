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
        public IActionResult TaoPhieuNhap()
        {
            var dsDichVu = _db.DichVus.ToList();
            return View(dsDichVu);
        }

        [HttpPost]
        public async Task<IActionResult> LuuPhieuNhap(string NhaCungCap, int[] MaDv, int[] SoLuong, decimal[] DonGia)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                var user = _db.NguoiDungs.FirstOrDefault(u => u.MaNd == userId);

                if (MaDv == null || MaDv.Length == 0)
                {
                    TempData["Error"] = "Vui lòng chọn ít nhất 1 sản phẩm để nhập hàng!";
                    return RedirectToAction("TaoPhieuNhap");
                }

                decimal tongTien = 0;
                for (int i = 0; i < MaDv.Length; i++)
                {
                    tongTien += SoLuong[i] * DonGia[i];
                }
                var phieuNhap = new PhieuNhap
                {
                    MaCn = user.MaCn,
                    MaNd = user.MaNd,
                    NhaCungCap = string.IsNullOrEmpty(NhaCungCap) ? "Nội bộ / Khác" : NhaCungCap,
                    NgayNhap = DateTime.Now,
                    TongTien = tongTien,
                    TrangThai = 0 
                };

                _db.PhieuNhaps.Add(phieuNhap);
                await _db.SaveChangesAsync(); 
                for (int i = 0; i < MaDv.Length; i++)
                {
                    var chiTiet = new ChiTietPhieuNhap
                    {
                        MaPn = phieuNhap.MaPn,
                        MaDv = MaDv[i],
                        SoLuong = SoLuong[i],
                        DonGia = DonGia[i]
                    };
                    _db.ChiTietPhieuNhaps.Add(chiTiet);
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "Đã tạo phiếu nhập thành công! Vui lòng chờ Admin duyệt để cập nhật tồn kho.";
                return RedirectToAction("LichSuNhap");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Lỗi hệ thống: " + ex.Message;
                return RedirectToAction("TaoPhieuNhap");
            }
        }

        public IActionResult LichSuNhap(string searchStr, DateTime? fromDate, DateTime? toDate, int? status, string sortOrder)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var user = _db.NguoiDungs.FirstOrDefault(u => u.MaNd == userId);

            if (user == null) return RedirectToAction("Login", "Home", new { area = "" });
            var query = _db.PhieuNhaps
                .Include(p => p.MaNdNavigation)
                .Where(p => p.MaCn == user.MaCn)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchStr))
            {
                searchStr = searchStr.ToLower().Trim();
                query = query.Where(p =>
                    p.MaPn.ToString().Contains(searchStr) ||
                    (p.NhaCungCap != null && p.NhaCungCap.ToLower().Contains(searchStr))
                );
            }
            if (fromDate.HasValue)
            {
                query = query.Where(p => p.NgayNhap >= fromDate.Value.Date);
            }
            if (toDate.HasValue)
            {
                var toDateEnd = toDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(p => p.NgayNhap <= toDateEnd);
            }
            if (status.HasValue)
            {
                query = query.Where(p => p.TrangThai == status.Value);
            }
            switch (sortOrder)
            {
                case "tien_asc":
                    query = query.OrderBy(p => p.TongTien);
                    break;
                case "tien_desc":
                    query = query.OrderByDescending(p => p.TongTien);
                    break;
                case "ngay_asc":
                    query = query.OrderBy(p => p.NgayNhap);
                    break;
                default:
                    query = query.OrderByDescending(p => p.NgayNhap);
                    break;
            }
            ViewBag.SearchStr = searchStr;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.Status = status;
            ViewBag.SortOrder = sortOrder;

            return View(query.ToList());
        }

        [HttpGet]
        public IActionResult GetChiTietPhieuNhap(int id)
        {
            var chiTiet = _db.ChiTietPhieuNhaps
                .Include(c => c.MaDvNavigation)
                .Where(c => c.MaPn == id)
                .Select(c => new
                {
                    tenDv = c.MaDvNavigation.TenDv,
                    soLuong = c.SoLuong,
                    donGia = c.DonGia,
                    thanhTien = c.SoLuong * c.DonGia
                }).ToList();

            return Json(new { success = true, data = chiTiet });
        }
        public IActionResult ChiTietPhieuNhap(int id)
        {
            var phieuNhap = _db.PhieuNhaps
                .Include(p => p.MaNdNavigation) 
                .Include(p => p.MaCnNavigation)
                .Include(p => p.ChiTietPhieuNhaps) 
                    .ThenInclude(c => c.MaDvNavigation) 
                .FirstOrDefault(p => p.MaPn == id);

            if (phieuNhap == null) return NotFound("Không tìm thấy phiếu nhập này!");

            return View(phieuNhap);
        }
    }

}