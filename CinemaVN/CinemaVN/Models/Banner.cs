using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CinemaVN.Models
{
    public partial class Banner
    {
        [Display(Name = "Mã")]
        public int MaBn { get; set; }
        [Display(Name = "Tiêu đề")]
        [Required(ErrorMessage = "Tiêu đề không được để trống!")]
        public string? TieuDe { get; set; }
        [Display(Name = "Hình ảnh")]
        [Required(ErrorMessage = "Tên hình không được để trống!")]
        public string HinhAnh { get; set; } = null!;
        [Display(Name = "Đường dẫn")]
        public string? DuongDan { get; set; }
        [Display(Name = "Trạng thái")]
        public bool? TrangThai { get; set; }
    }
}
