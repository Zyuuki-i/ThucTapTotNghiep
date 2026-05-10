using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using CinemaVN.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CinemaVN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DichVuController : Controller
    {
        private readonly CinemaVNContext _db;

        public DichVuController(CinemaVNContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View(_db.DichVus.OrderByDescending(d => d.MaDv).ToList());
        }

        public IActionResult Them() => View();

        [HttpPost]
        public async Task<IActionResult> Them(DichVu dv, IFormFile? HinhAnhUpload)
        {
            try
            {
                if (HinhAnhUpload != null && HinhAnhUpload.Length > 0)
                {
                    string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/dichvu");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(HinhAnhUpload.FileName);
                    string filePath = Path.Combine(folderPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await HinhAnhUpload.CopyToAsync(stream);
                    }
                    dv.HinhAnh = fileName;
                }
                else
                {
                    dv.HinhAnh = "default-service.png";
                }

                _db.DichVus.Add(dv);
                await _db.SaveChangesAsync();

                TempData["Success"] = "Đã thêm dịch vụ '" + dv.TenDv + "' thành công.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi lưu dữ liệu: " + ex.Message;
                return View(dv);
            }
        }

        public IActionResult Sua(int id)
        {
            var dv = _db.DichVus.Find(id);
            if (dv == null) return NotFound();
            return View(dv);
        }

        [HttpPost]
        public async Task<IActionResult> Sua(DichVu dv, IFormFile? HinhAnhUpload, string? HinhAnhHienTai)
        {
            try
            {
                if (HinhAnhUpload != null && HinhAnhUpload.Length > 0)
                {
                    string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/dichvu");
                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(HinhAnhUpload.FileName);
                    string filePath = Path.Combine(folderPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await HinhAnhUpload.CopyToAsync(stream);
                    }
                    dv.HinhAnh = fileName;
                }
                else
                {
                    dv.HinhAnh = HinhAnhHienTai;
                }

                _db.DichVus.Update(dv);
                await _db.SaveChangesAsync();

                TempData["Success"] = "Cập nhật dịch vụ thành công.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi cập nhật: " + ex.Message;
                return View(dv);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Xoa(int id)
        {
            try
            {
                var dv = await _db.DichVus.FindAsync(id);
                if (dv != null)
                {
                    _db.DichVus.Remove(dv);
                    await _db.SaveChangesAsync();
                    TempData["Success"] = "Đã xóa dịch vụ.";
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Lỗi: Không thể xóa dịch vụ này!";
            }
            return RedirectToAction("Index");
        }
    }
}