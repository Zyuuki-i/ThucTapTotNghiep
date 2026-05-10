using System;
using System.Collections.Generic;

namespace CinemaVN.Models
{
    public partial class ChiNhanh
    {
        public ChiNhanh()
        {
            HoaDons = new HashSet<HoaDon>();
            Khos = new HashSet<Kho>();
            NguoiDungs = new HashSet<NguoiDung>();
            PhieuNhaps = new HashSet<PhieuNhap>();
            PhongChieus = new HashSet<PhongChieu>();
        }

        public string MaCn { get; set; } = null!;
        public string? TenCn { get; set; }
        public string? DiaChi { get; set; }
        public string? Sdt { get; set; }

        public virtual ICollection<HoaDon> HoaDons { get; set; }
        public virtual ICollection<Kho> Khos { get; set; }
        public virtual ICollection<NguoiDung> NguoiDungs { get; set; }
        public virtual ICollection<PhieuNhap> PhieuNhaps { get; set; }
        public virtual ICollection<PhongChieu> PhongChieus { get; set; }
    }
}
