using CinemaVN.Models;
using System.ComponentModel.DataAnnotations;

namespace CinemaVN.DatModels
{
    public class CDichVu
    {
        [Display(Name = "Mã Dịch Vụ")]
        public int MaDv { get; set; }
        [Display(Name = "Tên Dịch Vụ")]
        [Required(ErrorMessage = "Tên dịch vụ không được để trống!")]
        public string TenDv { get; set; } = null!;
        [Display(Name = "Giá")]
        [Required(ErrorMessage = "Giá không được để trống!")]
        [Range(0, 10000000, ErrorMessage = "Giá không hợp lệ")]
        public decimal Gia { get; set; }
        [Display(Name = "Loại Dịch Vụ")]
        public string? LoaiDv { get; set; }
        [Display(Name = "Hình Ảnh")]
        public string? HinhAnh { get; set; }

        public static CDichVu toCDichVu(DichVu? x)
        {
            if (x == null) return null;
            return new CDichVu()
            {
                MaDv = x.MaDv,
                TenDv = x.TenDv,
                Gia = x.Gia,
                LoaiDv = x.LoaiDv,
                HinhAnh = x.HinhAnh,
            };
        }

        public static DichVu toDichVu(CDichVu x)
        {
            if (x == null) return null;
            return new DichVu()
            {
                MaDv = x.MaDv,
                TenDv = x.TenDv,
                Gia = x.Gia,
                LoaiDv = x.LoaiDv,
                HinhAnh = x.HinhAnh,
            };
        }
    }
}
