using CinemaVN.DatModels;
using CinemaVN.Models;
using Microsoft.AspNetCore.Mvc;

namespace CinemaVN.Areas.Admin.Controllers
{ 

    [Area("Admin")]
    public class NguoiDungController : Controller
    {
        private readonly CinemaVNContext db = new();
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult quanLy(string? macn, string? keyword, int trang = 1, bool trangthai = true)
        {
            var query = db.NguoiDungs.Where(t => t.MaVt != null && t.MaVt.ToLower() == "manage").AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(t =>
                    (t.HoTen != null && t.HoTen.ToLower().Contains(keyword.ToLower())) ||
                    (t.Cccd != null && t.Cccd.ToLower().Contains(keyword.ToLower())) ||
                    (t.Sdt != null && t.Sdt.ToLower().Contains(keyword.ToLower())) ||
                    (t.Email != null && t.Email.ToLower().Contains(keyword.ToLower())) ||
                    (t.MaNd.ToString().Contains(keyword.ToLower()))
                );
            }

            if (!string.IsNullOrEmpty(macn))
            {
                query = query.Where(t => t.MaCn != null && t.MaCn.ToLower() == macn.ToLower());
            }
            if (trangthai) {
                query = query.Where(t => t.TrangThai == true);
            }
            else
            {
                query = query.Where(t => t.TrangThai == false);
            }

            int kichThuoc = 5;
            int tongTrang = (int)Math.Ceiling((double)query.Count() / kichThuoc);
            query = query.Skip((trang - 1) * kichThuoc).Take(kichThuoc);

            ViewBag.trangHT = trang;
            ViewBag.tongTrang = tongTrang;

            ViewBag.Macn = macn;
            ViewBag.Keyword = keyword;
            ViewBag.Trangthai = trangthai;

            ViewBag.ChiNhanhs = db.ChiNhanhs.ToList();

            List<CNguoiDung> cNguoiDungs = new List<CNguoiDung>();
            foreach(var item in query.ToList())
            {
                cNguoiDungs.Add(CNguoiDung.ToCNguoiDung(item));
            }
            return View(cNguoiDungs);
        }

        public IActionResult nhanVien(string? macn, string? keyword, int trang = 1, bool trangthai = true)
        {
            var query = db.NguoiDungs.Where(t => t.MaVt != null && t.MaVt.ToLower() == "staff").AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(t =>
                    (t.HoTen != null && t.HoTen.ToLower().Contains(keyword.ToLower())) ||
                    (t.Cccd != null && t.Cccd.ToLower().Contains(keyword.ToLower())) ||
                    (t.Sdt != null && t.Sdt.ToLower().Contains(keyword.ToLower())) ||
                    (t.Email != null && t.Email.ToLower().Contains(keyword.ToLower())) ||
                    (t.MaNd.ToString().Contains(keyword.ToLower()))
                );
            }

            if (!string.IsNullOrEmpty(macn))
            {
                query = query.Where(t => t.MaCn != null && t.MaCn.ToLower() == macn.ToLower());
            }
            if (trangthai)
            {
                query = query.Where(t => t.TrangThai == true);
            }
            else
            {
                query = query.Where(t => t.TrangThai == false);
            }

            int kichThuoc = 5;
            int tongTrang = (int)Math.Ceiling((double)query.Count() / kichThuoc);
            query = query.Skip((trang - 1) * kichThuoc).Take(kichThuoc);

            ViewBag.trangHT = trang;
            ViewBag.tongTrang = tongTrang;

            ViewBag.Macn = macn;
            ViewBag.Keyword = keyword;
            ViewBag.Trangthai = trangthai;

            ViewBag.ChiNhanhs = db.ChiNhanhs.ToList();

            List<CNguoiDung> cNguoiDungs = new List<CNguoiDung>();
            foreach (var item in query.ToList())
            {
                cNguoiDungs.Add(CNguoiDung.ToCNguoiDung(item));
            }
            return View(cNguoiDungs);
        }

        public IActionResult khachHang(string? keyword, int trang = 1, bool trangthai = true)
        {
            var query = db.NguoiDungs.Where(t => t.MaVt != null && t.MaVt.ToLower() == "customer").AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(t =>
                    (t.HoTen != null && t.HoTen.ToLower().Contains(keyword.ToLower())) ||
                    (t.Sdt != null && t.Sdt.ToLower().Contains(keyword.ToLower())) ||
                    (t.Email != null && t.Email.ToLower().Contains(keyword.ToLower())) ||
                    (t.MaNd.ToString().Contains(keyword.ToLower()))
                );
            }

            if (trangthai)
            {
                query = query.Where(t => t.TrangThai == true);
            }
            else
            {
                query = query.Where(t => t.TrangThai == false);
            }

            int kichThuoc = 5;
            int tongTrang = (int)Math.Ceiling((double)query.Count() / kichThuoc);
            query = query.Skip((trang - 1) * kichThuoc).Take(kichThuoc);

            ViewBag.trangHT = trang;
            ViewBag.tongTrang = tongTrang;

            ViewBag.Keyword = keyword;
            ViewBag.Trangthai = trangthai;

            List<CNguoiDung> cNguoiDungs = new List<CNguoiDung>();
            foreach (var item in query.ToList())
            {
                cNguoiDungs.Add(CNguoiDung.ToCNguoiDung(item));
            }
            return View(cNguoiDungs);
        }

    }
}
