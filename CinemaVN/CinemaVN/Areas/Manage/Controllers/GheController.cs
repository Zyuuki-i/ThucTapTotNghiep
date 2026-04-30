using CinemaVN.Models;
using CinemaVN.MyModels;
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
            int tongGheVatLy = (maLg == "DOI") ? soLuong * 2 : soLuong;

           
            int sucChuaHienTai = _db.Ghes.Count(g => g.MaPc == maPc && g.Hang != hang);
            
            for (int i = 1; i <= tongGheVatLy; i++)
            {
                var ghe = _db.Ghes.FirstOrDefault(g => g.MaPc == maPc && g.Hang == hang && g.SoGhe == i);
                if (ghe != null)
                {
                    ghe.MaLg = maLg;
                    ghe.ToaDoX = i;       
                    ghe.ToaDoY = toaDoY;  
                    _db.Ghes.Update(ghe);
                }
                else
                {
                    _db.Ghes.Add(new Ghe
                    {
                        MaPc = maPc,
                        Hang = hang,
                        SoGhe = i,
                        MaLg = maLg,
                        ToaDoX = i,       
                        ToaDoY = toaDoY,  
                        TrangThai = true
                    });
                }
            }
 
            var gheThua = _db.Ghes.Where(g => g.MaPc == maPc && g.Hang == hang && g.SoGhe > tongGheVatLy).ToList();
            if (gheThua.Any()) _db.Ghes.RemoveRange(gheThua);

            _db.SaveChanges();
            
            int sucChuaMoi = _db.Ghes.Count(g => g.MaPc == maPc);
            if (sucChuaMoi > sucChuaHienTai)
            {
                phong.SucChua = sucChuaMoi;
                _db.PhongChieus.Update(phong);
                _db.SaveChanges();
            }
                

            TempData["MessageSuccess_Ghe"] = $"Đã lưu cấu hình hàng {hang}. Tổng ghế vật lý hiện tại: {sucChuaMoi}/{phong.SucChua}";
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