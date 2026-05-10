using CinemaVN.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaVN.Areas.Manage.Controllers
{
    [Area("Manage")]
    public class GheController : Controller
    {
        private readonly CinemaVNContext _db;
        public GheController(CinemaVNContext db) => _db = db;
        // 1. Trang liệt kê danh sách phòng của chi nhánh để chọn cấu hình
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var user = _db.NguoiDungs.Include(u => u.MaCnNavigation).FirstOrDefault(u => u.MaNd == userId);

            if (user == null) return RedirectToAction("dangNhap", "Home", new { area = "" });

            var dsPhong = _db.PhongChieus
                .Where(p => p.MaCn == user.MaCn)
                .ToList();

            ViewBag.TenChiNhanh = user.MaCnNavigation?.TenCn ?? "Không xác định";
            return View(dsPhong);
        }

        

        // 2. Giao diện hiển thị sơ đồ & bảng điều khiển
        public IActionResult CauHinhGhe(int maPc)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var user = _db.NguoiDungs.Find(userId);
            var phong = _db.PhongChieus.FirstOrDefault(p => p.MaPc == maPc && p.MaCn == user.MaCn);
            if (phong == null) return NotFound("Phòng không tồn tại hoặc bạn không có quyền truy cập.");

            ViewBag.Phong = phong;
            ViewBag.DanhSachLoaiGhe = _db.LoaiGhes.ToList();
            var dsGhe = _db.Ghes
                .Include(g => g.MaLgNavigation)
                .Where(g => g.MaPc == maPc)
                .OrderBy(g => g.Hang)
                .ThenBy(g => g.SoGhe)
                .ToList();

            return View(dsGhe);
        }

        // 3. Thêm/Cập nhật cấu hình cho 1 Hàng ghế
        [HttpPost]
        public IActionResult LuuHangGhe(int maPc, string hang, int soLuong, string maLg)
        {
            var user = _db.NguoiDungs.Find(HttpContext.Session.GetInt32("UserId"));
            var phong = _db.PhongChieus.FirstOrDefault(p => p.MaPc == maPc && p.MaCn == user.MaCn);

            if (phong == null || string.IsNullOrEmpty(hang)) return NotFound();

            hang = hang.ToUpper().Trim();
            int toaDoY = hang[0] - 64;
            int tongGheVatLyThemMoi = (maLg == "DOI") ? soLuong * 2 : soLuong;

            // 1. Tìm số ghế lớn nhất hiện tại của hàng này (Nếu hàng chưa có ghế nào thì gán bằng 0)
            int maxSoGheHienTai = _db.Ghes
                .Where(g => g.MaPc == maPc && g.Hang == hang)
                .Select(g => (int?)g.SoGhe)
                .Max() ?? 0;

            // 2. Thêm ghế mới nối tiếp vào sau số ghế lớn nhất
            for (int i = 1; i <= tongGheVatLyThemMoi; i++)
            {
                int soGheMoi = maxSoGheHienTai + i; // Ghế mới = Max cũ + i

                _db.Ghes.Add(new Ghe
                {
                    MaPc = maPc,
                    Hang = hang,
                    SoGhe = soGheMoi,
                    MaLg = maLg,
                    ToaDoX = soGheMoi, 
                    ToaDoY = toaDoY,
                    TrangThai = true
                });
            }

            _db.SaveChanges();

            // 3. Cập nhật lại sức chứa của phòng nếu tăng lên
            int sucChuaMoi = _db.Ghes.Count(g => g.MaPc == maPc);
            if (sucChuaMoi > (phong.SucChua ?? 0))
            {
                phong.SucChua = sucChuaMoi;
                _db.PhongChieus.Update(phong);
                _db.SaveChanges();
            }

            TempData["MessageSuccess_Ghe"] = $"Đã thêm nối tiếp {soLuong} ghế ({maLg}) vào hàng {hang}. Tổng ghế hiện tại: {sucChuaMoi}";
            return RedirectToAction("CauHinhGhe", new { maPc = maPc });
        }

        // 4. Xóa một hàng ghế
        [HttpPost]
        public IActionResult XoaHang(int maPc, string hang)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var user = _db.NguoiDungs.Find(userId);
            var phong = _db.PhongChieus.FirstOrDefault(p => p.MaPc == maPc && p.MaCn == user.MaCn);

            if (phong == null || string.IsNullOrEmpty(hang)) return NotFound();

            hang = hang.ToUpper().Trim();

            var dsGhe = _db.Ghes.Where(g => g.MaPc == maPc && g.Hang == hang).ToList();
            if (dsGhe.Any())
            {
                _db.Ghes.RemoveRange(dsGhe);
                _db.SaveChanges(); 

                int sucChuaMoi = _db.Ghes.Count(g => g.MaPc == maPc);
             
                _db.PhongChieus.Update(phong);
                _db.SaveChanges();

                TempData["MessageSuccess_Ghe"] = $"Đã xóa sạch hàng {hang}. Tổng ghế vật lý hiện tại: {sucChuaMoi}/{phong.SucChua} ghế.";
            }
            else
            {
                TempData["MessageError_Ghe"] = $"Không tìm thấy hàng {hang} để xóa!";
            }

            return RedirectToAction("CauHinhGhe", new { maPc = maPc });
        }

        // 5. Reset xóa toàn bộ ghế của phòng
        [HttpPost]
        public IActionResult XoaTatCa(int maPc)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var user = _db.NguoiDungs.Find(userId);
            var phong = _db.PhongChieus.FirstOrDefault(p => p.MaPc == maPc && p.MaCn == user.MaCn);

            if (phong == null)
            {
                TempData["MessageError_Ghe"] = "Không tìm thấy phòng chiếu để thực hiện reset.";
                return RedirectToAction("Index");
            }

            try
            {
                var dsGhe = _db.Ghes.Where(g => g.MaPc == maPc).ToList();
                if (dsGhe.Any())
                {
                    _db.Ghes.RemoveRange(dsGhe);
                }

                phong.SucChua = 0;
                _db.PhongChieus.Update(phong);


                _db.SaveChanges();

                TempData["MessageSuccess_Ghe"] = $"Đã xóa sạch toàn bộ sơ đồ ghế của phòng {phong.TenPc}.";
            }
            catch (Exception ex)
            {
                TempData["MessageError_Ghe"] = "Đã xảy ra lỗi khi reset sơ đồ: " + ex.Message;
            }

            return RedirectToAction("CauHinhGhe", new { maPc = maPc });
        }
    }
}