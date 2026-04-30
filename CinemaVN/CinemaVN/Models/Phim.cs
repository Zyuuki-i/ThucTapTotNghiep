using System;
using System.Collections.Generic;

namespace CinemaVN.Models
{
    public partial class Phim
    {
        public Phim()
        {
            SuatChieus = new HashSet<SuatChieu>();
            MaTls = new HashSet<TheLoai>();
        }

        public int MaPhim { get; set; }
        public string TenPhim { get; set; } = null!;
        public string? MoTa { get; set; }
        public int? ThoiLuong { get; set; }
        public DateTime? NgayChieu { get; set; }
        public string? DaoDien { get; set; }
        public string? Poster { get; set; }
        public string? Trailer { get; set; }
        public int? DoTuoi { get; set; }
        public int? TrangThai { get; set; }

        public virtual ICollection<SuatChieu> SuatChieus { get; set; }

        public virtual ICollection<TheLoai> MaTls { get; set; }
    }
}
