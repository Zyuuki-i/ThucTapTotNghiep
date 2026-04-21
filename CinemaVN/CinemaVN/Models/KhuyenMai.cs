using System;
using System.Collections.Generic;

namespace CinemaVN.Models
{
    public partial class KhuyenMai
    {
        public KhuyenMai()
        {
            HoaDons = new HashSet<HoaDon>();
        }

        public int MaKm { get; set; }
        public string Code { get; set; } = null!;
        public string? MoTa { get; set; }
        public decimal? DieuKien { get; set; }
        public int? SoDiem { get; set; }
        public int? PhanTramGiam { get; set; }
        public decimal? GiamToiDa { get; set; }
        public DateTime? NgayBd { get; set; }
        public DateTime? NgayKt { get; set; }
        public int? TrangThai { get; set; }

        public virtual ICollection<HoaDon> HoaDons { get; set; }
    }
}
