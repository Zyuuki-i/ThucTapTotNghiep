using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaVN.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CinemaVN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PhieuNhapController : Controller
    {
        private readonly CinemaVNContext _db;

        public PhieuNhapController(CinemaVNContext db)
        {
            _db = db;
        }

        public IActionResult Index(string searchStr, DateTime? fromDate, DateTime? toDate, int? status, string sortOrder)
        {
            var query = _db.PhieuNhaps
                .Include(p => p.MaCnNavigation)
                .Include(p => p.MaNdNavigation)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchStr))
            {
                searchStr = searchStr.ToLower().Trim();
                query = query.Where(p =>
                    p.MaPn.ToString().Contains(searchStr) ||
                    (p.NhaCungCap != null && p.NhaCungCap.ToLower().Contains(searchStr)) ||
                    (p.MaCnNavigation.TenCn != null && p.MaCnNavigation.TenCn.ToLower().Contains(searchStr))
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
                default:
                    query = query.OrderBy(p => p.TrangThai).ThenByDescending(p => p.NgayNhap);
                    break;
            }

            ViewBag.SearchStr = searchStr;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.Status = status;
            ViewBag.SortOrder = sortOrder;

            var allData = _db.PhieuNhaps.ToList();
            ViewBag.ChoDuyetCount = allData.Count(p => p.TrangThai == 0);
            ViewBag.DaDuyetCount = allData.Count(p => p.TrangThai == 1);

            return View(query.ToList());
        }

        public IActionResult ChiTiet(int id)
        {
            var phieu = _db.PhieuNhaps
                .Include(p => p.MaCnNavigation)
                .Include(p => p.MaNdNavigation)
                .Include(p => p.ChiTietPhieuNhaps).ThenInclude(c => c.MaDvNavigation)
                .FirstOrDefault(p => p.MaPn == id);

            if (phieu == null) return NotFound();
            return View(phieu);
        }

        [HttpPost]
        public async Task<IActionResult> DuyetPhieu(int id)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var phieu = await _db.PhieuNhaps
                    .Include(p => p.ChiTietPhieuNhaps)
                    .FirstOrDefaultAsync(p => p.MaPn == id);

                if (phieu == null) throw new Exception("Không tìm thấy phiếu nhập.");
                if (phieu.TrangThai == 1) throw new Exception("Phiếu này đã được duyệt trước đó!");

                phieu.TrangThai = 1;
                _db.PhieuNhaps.Update(phieu);

                foreach (var item in phieu.ChiTietPhieuNhaps)
                {
                    var kho = await _db.Khos.FirstOrDefaultAsync(k => k.MaCn == phieu.MaCn && k.MaDv == item.MaDv);

                    if (kho != null)
                    {
                        kho.SoLuongTon = (kho.SoLuongTon ?? 0) + item.SoLuong;
                        _db.Khos.Update(kho);
                    }
                    else
                    {
                        var newKho = new Kho
                        {
                            MaCn = phieu.MaCn,
                            MaDv = item.MaDv,
                            SoLuongTon = item.SoLuong
                        };
                        _db.Khos.Add(newKho);
                    }
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "Đã duyệt phiếu và cộng số lượng vào kho chi nhánh thành công!";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Lỗi khi duyệt: " + ex.Message;
            }

            return RedirectToAction("ChiTiet", new { id = id });
        }

        [HttpPost]
        public async Task<IActionResult> Xoa(int id)
        {
            try
            {
                var phieu = await _db.PhieuNhaps
                    .Include(p => p.ChiTietPhieuNhaps)
                    .FirstOrDefaultAsync(p => p.MaPn == id);

                if (phieu != null)
                {
                    if (phieu.TrangThai == 1)
                    {
                        TempData["Error"] = "Không thể xóa phiếu ĐÃ DUYỆT vì hàng đã vào kho. Vui lòng sửa kho thay vì xóa phiếu!";
                        return RedirectToAction("Index");
                    }
                    _db.ChiTietPhieuNhaps.RemoveRange(phieu.ChiTietPhieuNhaps);
                    _db.PhieuNhaps.Remove(phieu);
                    await _db.SaveChangesAsync();

                    TempData["Success"] = "Đã xóa phiếu nhập thành công.";
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Không thể xóa do ràng buộc dữ liệu!";
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> TuChoiPhieu(int id)
        {
            try
            {
                var phieu = await _db.PhieuNhaps.FindAsync(id);
                if (phieu == null) return NotFound();

                if (phieu.TrangThai != 0)
                {
                    TempData["Error"] = "Chỉ có thể từ chối phiếu đang ở trạng thái chờ duyệt!";
                    return RedirectToAction("Index");
                }

                phieu.TrangThai = 2; 
                _db.PhieuNhaps.Update(phieu);
                await _db.SaveChangesAsync();

                TempData["Success"] = "Đã từ chối phiếu nhập #" + id;
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
            }
            return RedirectToAction("Index");
        }
        
    }
}