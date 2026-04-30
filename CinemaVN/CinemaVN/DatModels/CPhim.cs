using CinemaVN.Models;
using System.ComponentModel.DataAnnotations;

namespace CinemaVN.DatModels
{
    public class CPhim
    {
        [Display(Name = "Mã")]
        public int MaPhim { get; set; }
        [Display(Name = "Tên phim")]
        [Required(ErrorMessage = "Tên phim không được để trống.")]
        public string TenPhim { get; set; } = null!;
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }
        [Display(Name = "Thời lượng")]
        [Required(ErrorMessage = "Thời lượng không được để trống.")]
        [Range(1, int.MaxValue, ErrorMessage = "Thời lượng phải là một số dương.")]
        public int? ThoiLuong { get; set; }
        [Display(Name = "Ngày chiếu")]
        [Required(ErrorMessage = "Ngày chiếu không được để trống.")]
        public DateTime? NgayChieu { get; set; }
        [Display(Name = "Đạo diễn")]
        public string? DaoDien { get; set; }
        [Display(Name = "Poster")]
        public string? Poster { get; set; }
        [Display(Name = "Trailer")]
        public string? Trailer { get; set; }
        [Display(Name = "Độ tuổi")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Độ tuổi phải là một số nguyên.")]
        public int? DoTuoi { get; set; }
        [Display(Name = "Trạng thái")]
        public int? TrangThai { get; set; }

        public static CPhim ToCPhim(Phim? phim)
        {
            if(phim == null) return new CPhim();
            return new CPhim
            {
                MaPhim = phim.MaPhim,
                TenPhim = phim.TenPhim,
                MoTa = phim.MoTa,
                ThoiLuong = phim.ThoiLuong,
                NgayChieu = phim.NgayChieu,
                DaoDien = phim.DaoDien,
                Poster = phim.Poster,
                Trailer = phim.Trailer,
                DoTuoi = phim.DoTuoi,
                TrangThai = phim.TrangThai
            };
        }
        
        public static Phim ToPhim(CPhim cPhim)
        {
            if(cPhim == null) return new Phim();
            return new Phim
            {
                MaPhim = cPhim.MaPhim,
                TenPhim = cPhim.TenPhim,
                MoTa = cPhim.MoTa,
                ThoiLuong = cPhim.ThoiLuong,
                NgayChieu = cPhim.NgayChieu,
                DaoDien = cPhim.DaoDien,
                Poster = cPhim.Poster,
                Trailer = cPhim.Trailer,
                DoTuoi = cPhim.DoTuoi,
                TrangThai = cPhim.TrangThai
            };
        }
    }
}
