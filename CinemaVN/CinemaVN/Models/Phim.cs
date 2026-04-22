using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CinemaVN.Models
{
    public partial class Phim
    {
        public Phim()
        {
            SuatChieus = new HashSet<SuatChieu>();
            MaTls = new HashSet<TheLoai>();
        }

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
        public int? DoTuoi { get; set; }
        [Display(Name = "Trạng thái")]
        public int? TrangThai { get; set; }

        public virtual ICollection<SuatChieu> SuatChieus { get; set; }

        public virtual ICollection<TheLoai> MaTls { get; set; }
    }
}
