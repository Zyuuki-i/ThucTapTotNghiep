using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CinemaVN.Models
{
    public partial class NguoiDung
    {
        public NguoiDung()
        {
            HoaDons = new HashSet<HoaDon>();
        }
        [Display(Name = "Mã người dùng")]
        public int MaNd { get; set; }
        [Display(Name = "Mã vai trò")]
        public string? MaVt { get; set; }
        [Display(Name = "Mã chi nhánh")]
        public string? MaCn { get; set; }
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; } = null!;
        [Display(Name = "Avatar")]
        public string? Hinh { get; set; }
        [Display(Name = "Họ tên")]
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [RegularExpression(@"^[a-zA-ZÀ-ỹ\s]+$", ErrorMessage = "Họ tên chỉ được chứa chữ cái và khoảng trắng")]
        public string? HoTen { get; set; }
        [Display(Name = "Ngày sinh")]
        [Required(ErrorMessage = "Ngày sinh không được để trống")]
        [DataType(DataType.Date, ErrorMessage = "Ngày sinh phải là một ngày hợp lệ")]
        public DateTime? NgaySinh { get; set; }
        [Display(Name = "Giới tính")]
        [Required(ErrorMessage = "Vui lòng chọn giới tính")]
        public bool? Phai { get; set; }
        [Display(Name = "Số điện thoại")]
        public string? Sdt { get; set; }
        [Display(Name = "Căn cước công dân")]
        public string? Cccd { get; set; }
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }
        [Display(Name = "Chức vụ")]
        public string? ChucVu { get; set; }
        [Display(Name = "Điểm hội viên")]
        public int? DiemHoiVien { get; set; }
        [Display(Name = "Trạng thái")]
        public bool? TrangThai { get; set; }

        public virtual ChiNhanh? MaCnNavigation { get; set; }
        public virtual VaiTro? MaVtNavigation { get; set; }
        public virtual ICollection<HoaDon> HoaDons { get; set; }
    }
}
