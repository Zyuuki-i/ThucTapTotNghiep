using System;
using System.Collections.Generic;

namespace CinemaVN.Models
{
    public partial class LoaiGhe
    {
        public LoaiGhe()
        {
            Ghes = new HashSet<Ghe>();
        }

        public string MaLg { get; set; } = null!;
        public string? TenLg { get; set; }
        public decimal? PhuThu { get; set; }

        public virtual ICollection<Ghe> Ghes { get; set; }
    }
}
