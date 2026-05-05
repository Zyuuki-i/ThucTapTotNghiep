using System;
using System.Collections.Generic;

namespace CinemaVN.Models
{
    public partial class NguoiDung
    {
        public NguoiDung()
        {
            HoaDons = new HashSet<HoaDon>();
            PhieuNhaps = new HashSet<PhieuNhap>();
        }

        public int MaNd { get; set; }
        public string? MaVt { get; set; }
        public string? MaCn { get; set; }
        public string Email { get; set; } = null!;
        public string MatKhau { get; set; } = null!;
        public string? Hinh { get; set; }
        public string? HoTen { get; set; }
        public DateTime? NgaySinh { get; set; }
        public bool? Phai { get; set; }
        public string? Sdt { get; set; }
        public string? Cccd { get; set; }
        public string? DiaChi { get; set; }
        public string? ChucVu { get; set; }
        public int? DiemHoiVien { get; set; }
        public bool? TrangThai { get; set; }

        public virtual ChiNhanh? MaCnNavigation { get; set; }
        public virtual VaiTro? MaVtNavigation { get; set; }
        public virtual ICollection<HoaDon> HoaDons { get; set; }
        public virtual ICollection<PhieuNhap> PhieuNhaps { get; set; }
    }
}
