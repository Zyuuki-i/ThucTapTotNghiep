using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CinemaVN.Models;

namespace CinemaVN.Areas.Manage.Controllers
{
    [Area("Manage")]
    public class SuatChieuController : Controller
    {
        private readonly CinemaVNContext _db;
        public SuatChieuController(CinemaVNContext db) => _db = db;

        public IActionResult Index(string searchString, DateTime? filterDate, int? filterMovie, int? filterRoom)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var user = _db.NguoiDungs.Include(u => u.MaCnNavigation).FirstOrDefault(u => u.MaNd == userId);

            var query = _db.SuatChieus
                .Include(s => s.MaPhimNavigation)
                .Include(s => s.MaPcNavigation)
                .Where(s => s.MaPcNavigation.MaCn == user.MaCn)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s =>
                    s.MaPhimNavigation.TenPhim.Contains(searchString) ||
                    s.MaPcNavigation.TenPc.Contains(searchString));
            }

            if (filterDate.HasValue)
            {
                query = query.Where(s => s.NgayChieu == filterDate.Value.Date);
            }

            if (filterMovie.HasValue)
            {
                query = query.Where(s => s.MaPhim == filterMovie.Value);
            }

            if (filterRoom.HasValue)
            {
                query = query.Where(s => s.MaPc == filterRoom.Value);
            }

            var dsSuatChieu = query
                .OrderByDescending(s => s.NgayChieu)
                .ThenBy(s => s.GioBd)
                .ToList();

            ViewBag.TenChiNhanh = user.MaCnNavigation?.TenCn;

            var danhSachPhim = _db.Phims.Where(p => p.TrangThai == 1).ToList();
            ViewBag.PhimList = new SelectList(danhSachPhim, "MaPhim", "TenPhim", filterMovie);

            var danhSachPhong = _db.PhongChieus.Where(p => p.MaCn == user.MaCn).ToList();
            ViewBag.RoomList = new SelectList(danhSachPhong, "MaPc", "TenPc", filterRoom);

            ViewBag.SearchString = searchString;
            ViewBag.FilterDate = filterDate?.ToString("yyyy-MM-dd");
            ViewBag.FilterMovie = filterMovie;
            ViewBag.FilterRoom = filterRoom; 
            return View(dsSuatChieu);
        }

        public IActionResult Them()
        {
            PrepareDropdowns();
            return View();
        }

        [HttpPost]
        [HttpPost]
        public IActionResult Them(SuatChieu sc)
        {
            var phim = _db.Phims.Find(sc.MaPhim);
            var phong = _db.PhongChieus.Find(sc.MaPc);

            if (sc.NgayChieu == null || sc.GioBd == null || phim == null || phong == null)
            {
                TempData["MessageError_SC"] = "Vui lòng nhập đầy đủ thông tin phim, phòng, ngày và giờ chiếu!";
                PrepareDropdowns();
                return View(sc);
            }

            DateTime thoiGianBatDau = sc.NgayChieu.Value.Date + sc.GioBd.Value;

            if (thoiGianBatDau <= DateTime.Now)
            {
                TempData["MessageError_SC"] = "Ngày và giờ chiếu không hợp lệ! Vui lòng chọn thời gian sau thời điểm hiện tại.";
                PrepareDropdowns();
                return View(sc);
            }

            sc.GioKt = sc.GioBd.Value.Add(TimeSpan.FromMinutes((double)phim.ThoiLuong + 15));

            bool biTrungLich = _db.SuatChieus.Any(s =>
                s.MaPc == sc.MaPc &&
                s.NgayChieu == sc.NgayChieu &&
                sc.GioBd < s.GioKt &&
                sc.GioKt > s.GioBd);

            if (biTrungLich)
            {
                TempData["MessageError_SC"] = $"Trùng lịch! Phòng {phong.TenPc} đã có suất chiếu khác trong khoảng thời gian từ {sc.GioBd.Value.ToString(@"hh\:mm")} đến {sc.GioKt.Value.ToString(@"hh\:mm")}.";
                PrepareDropdowns();
                return View(sc);
            }
            try
            {
                sc.TrangThai = 1;
                _db.SuatChieus.Add(sc);
                _db.SaveChanges();
                TempData["MessageSuccess_SC"] = "Thêm suất chiếu mới thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["MessageError_SC"] = "Lỗi hệ thống! Không thể lưu suất chiếu.";
                PrepareDropdowns();
                return View(sc);
            }
        }

        private bool KiemTraTrungLich(int maPc, DateTime ngayChieu, TimeSpan gioBd, TimeSpan gioKt, int? maScHienTai = null)
        {
            var query = _db.SuatChieus.Where(s => s.MaPc == maPc && s.NgayChieu == ngayChieu);

            if (maScHienTai.HasValue)
                query = query.Where(s => s.MaSc != maScHienTai.Value);

            return query.AsEnumerable().Any(s =>
                s.GioBd.HasValue && s.GioKt.HasValue &&
                gioBd < s.GioKt.Value && gioKt > s.GioBd.Value);
        }


        private void PrepareDropdowns()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var user = _db.NguoiDungs.Find(userId);

            ViewBag.Phims = _db.Phims.Where(p => p.TrangThai == 1).ToList();
            ViewBag.Phongs = _db.PhongChieus.Where(p => p.MaCn == user.MaCn).ToList();

            
            ViewBag.MaDdList = new SelectList(_db.DinhDangs.ToList(), "MaDd", "TenDd");
        }

        [HttpPost]
        public IActionResult Xoa(int id)
        {
            var suatChieu = _db.SuatChieus.Find(id);

            if (suatChieu != null)
            {
                try
                { 
                    bool daCoVe = _db.Ves.Any(v => v.MaSc == id);
                    if (daCoVe)
                    {
                        TempData["MessageError_SC"] = "Không thể xóa suất chiếu đã có khách đặt vé!";
                        return RedirectToAction("Index");
                    }

                    _db.SuatChieus.Remove(suatChieu);
                    _db.SaveChanges();
                    TempData["MessageSuccess_SC"] = "Đã xóa suất chiếu thành công.";
                }
                catch (Exception)
                {
                    TempData["MessageError_SC"] = "Không thể xóa suất chiếu này do ràng buộc dữ liệu.";
                }
            }

            return RedirectToAction("Index");
        }
        public IActionResult Sua(int id)
        {
            var sc = _db.SuatChieus.Find(id);
            if (sc == null) return NotFound();

            PrepareDropdowns(); 
            return View(sc);
        }

        [HttpPost]
        public IActionResult Sua(SuatChieu sc)
        {
            var phim = _db.Phims.Find(sc.MaPhim);

            DateTime thoiGianBatDau = (sc.NgayChieu ?? DateTime.Now).Date + (sc.GioBd ?? TimeSpan.Zero);
            if (thoiGianBatDau < DateTime.Now)
            {
                TempData["MessageError_SC"] = "Thời gian bắt đầu không được nhỏ hơn thời gian hiện tại!";
                PrepareDropdowns();
                return View(sc);
            }

            if (phim != null && sc.GioBd.HasValue)
            {
                sc.GioKt = sc.GioBd.Value.Add(TimeSpan.FromMinutes((double)phim.ThoiLuong + 15));
            }

            if (KiemTraTrungLich(sc.MaPc ?? 0, sc.NgayChieu ?? DateTime.Now, sc.GioBd.Value, sc.GioKt.Value, sc.MaSc))
            {
                TempData["MessageError_SC"] = "Trùng lịch! Phòng đã chọn đã có suất chiếu khác trong khung giờ này.";
                PrepareDropdowns();
                return View(sc);
            }

            try
            {
                _db.SuatChieus.Update(sc);
                _db.SaveChanges();
                TempData["MessageSuccess_SC"] = "Cập nhật suất chiếu thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["MessageError_SC"] = "Đã xảy ra lỗi hệ thống khi cập nhật.";
                PrepareDropdowns();
                return View(sc);
            }
        }
        [HttpGet]
        public IActionResult LayLichChieuTheoNgay(int maPc, DateTime ngayChieu)
        {
            var dsSuat = _db.SuatChieus
                .Where(s => s.MaPc == maPc && s.NgayChieu == ngayChieu)
                .OrderBy(s => s.GioBd)
                .Select(s => new {
                    maSc = s.MaSc,
                    tenPhim = s.MaPhimNavigation.TenPhim,
                    gioBd = s.GioBd.HasValue ? s.GioBd.Value.ToString(@"hh\:mm") : "",
                    gioKt = s.GioKt.HasValue ? s.GioKt.Value.ToString(@"hh\:mm") : ""
                })
                .ToList();

            return Json(dsSuat);
        }

    }
}