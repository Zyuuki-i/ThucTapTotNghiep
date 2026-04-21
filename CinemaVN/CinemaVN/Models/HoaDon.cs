using System;
using System.Collections.Generic;

namespace CinemaVN.Models
{
    public partial class HoaDon
    {
        public HoaDon()
        {
            ChiTietDichVus = new HashSet<ChiTietDichVu>();
            Ves = new HashSet<Ve>();
        }

        public int MaHd { get; set; }
        public int? MaNd { get; set; }
        public string? MaCn { get; set; }
        public int? MaKm { get; set; }
        public DateTime? NgayLap { get; set; }
        public decimal? TongTien { get; set; }
        public int? TrangThai { get; set; }

        public virtual ChiNhanh? MaCnNavigation { get; set; }
        public virtual KhuyenMai? MaKmNavigation { get; set; }
        public virtual NguoiDung? MaNdNavigation { get; set; }
        public virtual ICollection<ChiTietDichVu> ChiTietDichVus { get; set; }
        public virtual ICollection<Ve> Ves { get; set; }
    }
}
