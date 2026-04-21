using System;
using System.Collections.Generic;

namespace CinemaVN.Models
{
    public partial class PhongChieu
    {
        public PhongChieu()
        {
            Ghes = new HashSet<Ghe>();
            SuatChieus = new HashSet<SuatChieu>();
        }

        public int MaPc { get; set; }
        public string? TenPc { get; set; }
        public string? MaCn { get; set; }
        public int? SucChua { get; set; }

        public virtual ChiNhanh? MaCnNavigation { get; set; }
        public virtual ICollection<Ghe> Ghes { get; set; }
        public virtual ICollection<SuatChieu> SuatChieus { get; set; }
    }
}
