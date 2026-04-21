using System;
using System.Collections.Generic;

namespace CinemaVN.Models
{
    public partial class DinhDang
    {
        public DinhDang()
        {
            SuatChieus = new HashSet<SuatChieu>();
        }

        public string MaDd { get; set; } = null!;
        public string? TenDd { get; set; }
        public decimal? PhuThu { get; set; }

        public virtual ICollection<SuatChieu> SuatChieus { get; set; }
    }
}
