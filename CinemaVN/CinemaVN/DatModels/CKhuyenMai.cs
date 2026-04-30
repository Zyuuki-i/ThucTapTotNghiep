using CinemaVN.Models;
using System.ComponentModel.DataAnnotations;

namespace CinemaVN.DatModels
{
    public class CKhuyenMai
    {
        [Display(Name = "ID")]
        public int MaKm { get; set; }
        [Display(Name = "CODE")]
        [Required(ErrorMessage = "Code không được để trống")]
        public string Code { get; set; } = null!;
        [Display(Name = "Mô tả")]
        [Required(ErrorMessage = "Mô tả không được để trống")]
        public string? MoTa { get; set; }
        [Display(Name = "Điều kiện")]
        [Required(ErrorMessage = "Điều kiện không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Điều kiện phải là số dương")]
        public decimal? DieuKien { get; set; }
        [Display(Name = "Số điểm")]
        [Required(ErrorMessage = "Số điểm không được để trống")]
        [Range(0, int.MaxValue, ErrorMessage = "Số điểm phải là số dương")]
        public int? SoDiem { get; set; }
        [Display(Name = "Phần trăm giảm")]
        [Required(ErrorMessage = "Phần trăm giảm không được để trống")]
        [Range(0, 100, ErrorMessage = "Phần trăm giảm phải từ 0 đến 100")]
        public int? PhanTramGiam { get; set; }
        [Display(Name = "Giảm tối đa")]
        [Required(ErrorMessage = "Giảm tối đa không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Giảm tối đa phải là số dương")]
        public decimal? GiamToiDa { get; set; }
        [Display(Name = "Ngày bắt đầu")]
        [Required(ErrorMessage = "Ngày bắt đầu không được để trống")]
        public DateTime? NgayBd { get; set; }
        [Display(Name = "Ngày kết thúc")]
        [Required(ErrorMessage = "Ngày kết thúc không được để trống")]
        public DateTime? NgayKt { get; set; }
        [Display(Name = "Trạng thái")]
        public int? TrangThai { get; set; }

        public static CKhuyenMai ToCKhuyenMai(KhuyenMai? x)
        {
            if (x == null) return new CKhuyenMai();
            return new CKhuyenMai
            {
                MaKm = x.MaKm,
                Code = x.Code,
                MoTa = x.MoTa,
                DieuKien = x.DieuKien,
                SoDiem = x.SoDiem,
                PhanTramGiam = x.PhanTramGiam,
                GiamToiDa = x.GiamToiDa,
                NgayBd = x.NgayBd,
                NgayKt = x.NgayKt,
                TrangThai = x.TrangThai
            };
        }

        public static KhuyenMai ToKhuyenMai(CKhuyenMai x)
        {
            return new KhuyenMai
            {
                MaKm = x.MaKm,
                Code = x.Code,
                MoTa = x.MoTa,
                DieuKien = x.DieuKien,
                SoDiem = x.SoDiem,
                PhanTramGiam = x.PhanTramGiam,
                GiamToiDa = x.GiamToiDa,
                NgayBd = x.NgayBd,
                NgayKt = x.NgayKt,
                TrangThai = x.TrangThai
            };
        }
    }
}
