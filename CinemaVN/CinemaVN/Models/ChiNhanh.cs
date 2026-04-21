using System;
using System.Collections.Generic;

namespace CinemaVN.Models
{
    public partial class ChiNhanh
    {
        public ChiNhanh()
        {
            HoaDons = new HashSet<HoaDon>();
            NguoiDungs = new HashSet<NguoiDung>();
            PhongChieus = new HashSet<PhongChieu>();
        }

        public string MaCn { get; set; } = null!;
        public string? TenCn { get; set; }
        public string? DiaChi { get; set; }
        public string? Sdt { get; set; }

        public virtual ICollection<HoaDon> HoaDons { get; set; }
        public virtual ICollection<NguoiDung> NguoiDungs { get; set; }
        public virtual ICollection<PhongChieu> PhongChieus { get; set; }
    }
}
