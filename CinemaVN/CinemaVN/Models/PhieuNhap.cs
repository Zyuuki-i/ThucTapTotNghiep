using System;
using System.Collections.Generic;

namespace CinemaVN.Models
{
    public partial class PhieuNhap
    {
        public PhieuNhap()
        {
            ChiTietPhieuNhaps = new HashSet<ChiTietPhieuNhap>();
        }

        public int MaPn { get; set; }
        public string? MaCn { get; set; }
        public int? MaNd { get; set; }
        public string? NhaCungCap { get; set; }
        public DateTime? NgayNhap { get; set; }
        public decimal? TongTien { get; set; }
        public int? TrangThai { get; set; }

        public virtual ChiNhanh? MaCnNavigation { get; set; }
        public virtual NguoiDung? MaNdNavigation { get; set; }
        public virtual ICollection<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; }
    }
}
