using System;
using System.Collections.Generic;

namespace CinemaVN.Models
{
    public partial class SuatChieu
    {
        public SuatChieu()
        {
            Ves = new HashSet<Ve>();
        }

        public int MaSc { get; set; }
        public int? MaPhim { get; set; }
        public int? MaPc { get; set; }
        public string? MaDd { get; set; }
        public DateTime? NgayChieu { get; set; }
        public TimeSpan? GioBd { get; set; }
        public TimeSpan? GioKt { get; set; }
        public decimal? GiaGoc { get; set; }
        public int? TrangThai { get; set; }

        public virtual DinhDang? MaDdNavigation { get; set; }
        public virtual PhongChieu? MaPcNavigation { get; set; }
        public virtual Phim? MaPhimNavigation { get; set; }
        public virtual ICollection<Ve> Ves { get; set; }
    }
}
