using CinemaVN.DatModels;
using CinemaVN.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;

namespace CinemaVN.Controllers
{
    public class HoaDonController : Controller
    {
        private CinemaVNContext db = new CinemaVNContext();
        public IActionResult Index(int? phim, string? macn)
        {
            var nd = db.NguoiDungs.FirstOrDefault(n => n.MaNd == HttpContext.Session.GetInt32("UserId"));
            if (nd == null)
                return RedirectToAction("Index", "NguoiDung");

            ViewBag.Phims = db.Phims.Where(t=>t.TrangThai==1).ToList();
            ViewBag.PhimChon = phim;

            ViewBag.ChiNhanhs = db.ChiNhanhs.ToList();
            ViewBag.ChiNhanhChon = macn;

            var suats = new List<SuatChieu>();

            if (phim.HasValue)
            {
                suats = db.SuatChieus
                    .Include(sc => sc.MaPcNavigation)
                    .Where(sc => sc.MaPhim == phim && sc.NgayChieu >= DateTime.Today && sc.TrangThai == 1)
                    .OrderBy(sc => sc.NgayChieu)
                    .ThenBy(sc => sc.GioBd)
                    .ToList();
            }

            if (!string.IsNullOrEmpty(macn))
                suats = suats.Where(s => s.MaPcNavigation?.MaCn == macn).ToList();

            ViewBag.SuatChieus = suats;

            return View();
        }

        public IActionResult chonGhe(int masc)
        {
            var nd = db.NguoiDungs.FirstOrDefault(n => n.MaNd == HttpContext.Session.GetInt32("UserId"));
            if (nd == null)
                return RedirectToAction("Index", "NguoiDung");

            string? sessionKey = HttpContext.Session.GetString("SessionKey");
            if (string.IsNullOrEmpty(sessionKey))
            {
                sessionKey = Guid.NewGuid().ToString();
                HttpContext.Session.SetString("SessionKey", sessionKey);
            }

            var suat = db.SuatChieus
                .Include(s => s.MaPcNavigation)
                    .ThenInclude(pc => pc.MaCnNavigation)
                .Include(s => s.MaPhimNavigation)
                .Include(s => s.MaDdNavigation)
                .FirstOrDefault(s => s.MaSc == masc);

            if (suat == null)
                return RedirectToAction("Index");

            var ghes = db.Ghes
                .Where(g => g.MaPc == suat.MaPc)
                .OrderBy(g => g.Hang)
                .ThenBy(g => g.SoGhe)
                .ToList();

            var timeExpire = DateTime.Now.AddMinutes(-10);

            var hetHan = db.Ves
                .Where(v => v.TrangThai == 0 && v.ThoiGianGiu < timeExpire)
                .ToList();

            if (hetHan.Any())
            {
                db.Ves.RemoveRange(hetHan);
                db.SaveChanges();
            }

            var gheData = db.Ves
                .Where(v => v.MaSc == masc &&
                    (
                        v.TrangThai == 1 
                        ||
                        (v.TrangThai == 0 && v.ThoiGianGiu > timeExpire)
                    )
                )
                 .Select(v => new GheStatus
                 {
                     MaGhe = v.MaGhe.Value,
                     SessionKey = v.SessionKey,
                     TrangThai = v.TrangThai
                 })
                .ToList();

           
            var gheCuaToi = gheData
                .Where(g => g.SessionKey == sessionKey)
                .Select(g => g.MaGhe)
                .ToList();

            ViewBag.Ghes = ghes;
            ViewBag.GheData = gheData;       
            ViewBag.GheCuaToi = gheCuaToi;   
            ViewBag.SessionKey = sessionKey;
            ViewBag.MaSc = masc;
            ViewBag.Suat = suat;
            ViewBag.LoaiGhes = db.LoaiGhes.ToList();

            return View();
        }

        public IActionResult DichVu(int masc, string gheIds)
        {
            var nd = db.NguoiDungs.FirstOrDefault(n => n.MaNd == HttpContext.Session.GetInt32("UserId"));
            if (nd == null)
                return RedirectToAction("Index", "NguoiDung");

            string? sessionKey = HttpContext.Session.GetString("SessionKey");
            if (string.IsNullOrEmpty(sessionKey))
                return RedirectToAction("Index");

            var suat = db.SuatChieus
                .Include(s => s.MaPcNavigation)
                .FirstOrDefault(s => s.MaSc == masc);

            if (suat == null)
                return RedirectToAction("Index");

            var dinhDang = db.DinhDangs.FirstOrDefault(d => d.MaDd == suat.MaDd);

            var dsGhe = gheIds.Split(',')
                .Select(x => int.TryParse(x, out var id) ? id : 0)
                .Where(x => x > 0)
                .Distinct()
                .ToList();

            if (!dsGhe.Any())
                return RedirectToAction("chonGhe", new { masc });

            var gheList = db.Ghes
                .Where(g => dsGhe.Contains(g.MaGhe))
                .ToList();

            var loaiGheList = db.LoaiGhes.ToList();

            var timeExpire = DateTime.Now.AddMinutes(-10);
            var hetHan = db.Ves
                .Where(v => v.TrangThai == 0 && v.ThoiGianGiu < timeExpire)
                .ToList();

            if (hetHan.Any())
            {
                db.Ves.RemoveRange(hetHan);
                db.SaveChanges();
            }

            var gheDangBiGiu = db.Ves
                .Where(v => v.MaSc == masc &&
                    (
                        v.TrangThai == 1 ||
                        (v.TrangThai == 0 && v.ThoiGianGiu > timeExpire)
                    )
                )
                .Select(v => v.MaGhe)
                .ToList();

            foreach (var gheId in dsGhe)
            {
                if (gheDangBiGiu.Contains(gheId))
                    continue;

                var ghe = gheList.FirstOrDefault(g => g.MaGhe == gheId);
                if (ghe == null)
                    continue;

                if (ghe.MaPc != suat.MaPc)
                    continue;

                var loaiGhe = loaiGheList.FirstOrDefault(lg => lg.MaLg == ghe.MaLg);

                decimal gia =
                    (suat.GiaGoc ?? 0) +
                    (loaiGhe?.PhuThu ?? 0) +
                    (dinhDang?.PhuThu ?? 0);

                db.Ves.Add(new Ve
                {
                    MaSc = masc,
                    MaGhe = gheId,
                    Gia = gia,
                    TrangThai = 0,
                    ThoiGianGiu = DateTime.Now,
                    SessionKey = sessionKey
                });
            }

            db.SaveChanges();

            var dichVus = db.DichVus.ToList();

            ViewBag.MaSc = masc;
            ViewBag.GheIds = gheIds;
            ViewBag.SoDiemKhaDung = nd.DiemHoiVien;
            ViewBag.KhuyenMais = db.KhuyenMais.Where(t=>t.TrangThai==1&&t.SoDiem<=nd.DiemHoiVien && t.NgayKt >= DateTime.Now).ToList();

            return View(dichVus);
        }

        public IActionResult ThanhToan(int masc, IFormCollection form)
        {
            using var tran = db.Database.BeginTransaction();
            try
            {
                var nd = db.NguoiDungs.FirstOrDefault(n => n.MaNd == HttpContext.Session.GetInt32("UserId"));
                if (nd == null)
                    return RedirectToAction("Index", "NguoiDung");

                string? sessionKey = HttpContext.Session.GetString("SessionKey");

                var ves = db.Ves
                    .Where(v => v.MaSc == masc &&
                                v.SessionKey == sessionKey &&
                                v.TrangThai == 0 &&
                                v.ThoiGianGiu > DateTime.Now.AddMinutes(-10))
                    .ToList();

                if (!ves.Any())
                {
                    TempData["MessageError_DatVe"] = "Ghế đã hết hạn!";
                    return RedirectToAction("Index");
                }

                SuatChieu? sc = db.SuatChieus
                                  .Include(s => s.MaPcNavigation)
                                  .FirstOrDefault(t => t.MaSc == ves.First().MaSc);
                string maCn = sc?.MaPcNavigation?.MaCn ?? "CNQ1";

                var dsDv = new List<ChiTietDichVu>();
                foreach (var key in form.Keys)
                {
                    if (key.StartsWith("soLuong_"))
                    {
                        int maDv = int.Parse(key.Replace("soLuong_", ""));
                        int soLuong = int.Parse(form[key]);

                        if (soLuong > 0)
                        {
                            dsDv.Add(new ChiTietDichVu
                            {
                                MaDv = maDv,
                                DonGia = db.DichVus.FirstOrDefault(t => t.MaDv == maDv)?.Gia ?? 0,
                                SoLuong = soLuong
                            });
                        }
                    }
                }

                foreach (var dv in dsDv)
                {
                    var kho = db.Khos.FirstOrDefault(k => k.MaDv == dv.MaDv && k.MaCn == maCn);

                    if (kho == null || kho.SoLuongTon < dv.SoLuong)
                    {
                        tran.Rollback(); 

                        var tenDv = db.DichVus.FirstOrDefault(d => d.MaDv == dv.MaDv)?.TenDv ?? "Dịch vụ";
                        int tonHienTai = kho?.SoLuongTon ?? 0;

                        TempData["MessageError_DichVu"] = $"Rất tiếc, {tenDv} hiện chỉ còn {tonHienTai} phần. Vui lòng điều chỉnh lại số lượng!";

                        string gheIds = string.Join(",", ves.Select(v => v.MaGhe));

                        return RedirectToAction("DichVu", new { masc = masc, gheIds = gheIds });
                    }
                }

                decimal tongTienVe = ves.Sum(v => v.Gia ?? 0);
                decimal tongTienDv = 0;

                foreach (var dv in dsDv)
                {
                    var gia = db.DichVus.First(x => x.MaDv == dv.MaDv).Gia;
                    tongTienDv += gia * dv.SoLuong;
                }

                decimal tongTien = tongTienVe + tongTienDv;
                decimal tongTienGiam = 0;

                int? maKm = string.IsNullOrEmpty(form["maKm"]) ? null : int.Parse(form["maKm"]);
                KhuyenMai? km = null;

                if (maKm != null) km = db.KhuyenMais.Where(t => t.MaKm == maKm).FirstOrDefault();
                if (km != null)
                {
                    if (km.NgayBd <= DateTime.Now && km.NgayKt >= DateTime.Now)
                    {
                        if (km.DieuKien == null || tongTien >= km.DieuKien)
                        {
                            if (km.SoDiem == null || nd.DiemHoiVien >= km.SoDiem)
                            {
                                if (km.PhanTramGiam != null)
                                {
                                    tongTienGiam = tongTien * km.PhanTramGiam.Value / 100;
                                }

                                if (km.GiamToiDa != null && tongTienGiam > km.GiamToiDa)
                                {
                                    tongTienGiam = km.GiamToiDa.Value;
                                }
                            }
                        }
                    }
                }

                decimal tongSauGiam = Math.Max(0, tongTien - tongTienGiam);

                var hd = new HoaDon
                {
                    MaNd = nd.MaNd,
                    MaKm = km?.MaKm,
                    MaCn = maCn,
                    NgayLap = DateTime.Now,
                    TongTien = tongSauGiam,
                    TrangThai = 0
                };

                db.HoaDons.Add(hd);
                db.SaveChanges();

                foreach (var v in ves)
                {
                    v.MaHd = hd.MaHd;
                }

                foreach (var dv in dsDv)
                {
                    db.ChiTietDichVus.Add(new ChiTietDichVu
                    {
                        MaHd = hd.MaHd,
                        MaDv = dv.MaDv,
                        DonGia = dv.DonGia,
                        SoLuong = dv.SoLuong
                    });
                }

                db.SaveChanges();
                tran.Commit();

                return RedirectToAction("ThanhToanVnpay", "Payment", new { mahd = hd.MaHd });
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }




    }
}
