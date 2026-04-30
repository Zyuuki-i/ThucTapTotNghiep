using CinemaVN.Models;
using System.ComponentModel.DataAnnotations;

namespace CinemaVN.DatModels
{
    public class CNguoiDung
    {
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
        [Display(Name = "Họ & Tên")]
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

        public ChiNhanh? MaCnNavigation { get; set; }
        public VaiTro? MaVtNavigation { get; set; }

        public static CNguoiDung ToCNguoiDung(NguoiDung? nguoiDung)
        {
            if(nguoiDung == null) return new CNguoiDung();
            return new CNguoiDung
            {
                MaNd = nguoiDung.MaNd,
                MaVt = nguoiDung.MaVt,
                MaCn = nguoiDung.MaCn,
                Email = nguoiDung.Email,
                MatKhau = nguoiDung.MatKhau,
                Hinh = nguoiDung.Hinh,
                HoTen = nguoiDung.HoTen,
                NgaySinh = nguoiDung.NgaySinh,
                Phai = nguoiDung.Phai,
                Sdt = nguoiDung.Sdt,
                Cccd = nguoiDung.Cccd,
                DiaChi = nguoiDung.DiaChi,
                ChucVu = nguoiDung.ChucVu,
                DiemHoiVien = nguoiDung.DiemHoiVien,
                TrangThai = nguoiDung.TrangThai,
                MaCnNavigation = nguoiDung.MaCnNavigation,
                MaVtNavigation = nguoiDung.MaVtNavigation
            };
        }

        public static NguoiDung ToNguoiDung(CNguoiDung cNguoiDung)
        {
            if(cNguoiDung == null) return new NguoiDung();
            return new NguoiDung
            {
                MaNd = cNguoiDung.MaNd,
                MaVt = cNguoiDung.MaVt,
                MaCn = cNguoiDung.MaCn,
                Email = cNguoiDung.Email,
                MatKhau = cNguoiDung.MatKhau,
                Hinh = cNguoiDung.Hinh,
                HoTen = cNguoiDung.HoTen,
                NgaySinh = cNguoiDung.NgaySinh,
                Phai = cNguoiDung.Phai,
                Sdt = cNguoiDung.Sdt,
                Cccd = cNguoiDung.Cccd,
                DiaChi = cNguoiDung.DiaChi,
                ChucVu = cNguoiDung.ChucVu,
                DiemHoiVien = cNguoiDung.DiemHoiVien,
                TrangThai = cNguoiDung.TrangThai,
                MaCnNavigation = cNguoiDung.MaCnNavigation,
                MaVtNavigation = cNguoiDung.MaVtNavigation
            };
        }
    }
}
