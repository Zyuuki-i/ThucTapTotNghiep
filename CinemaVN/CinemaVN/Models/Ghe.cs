using System;
using System.Collections.Generic;

namespace CinemaVN.Models
{
    public partial class Ghe
    {
        public Ghe()
        {
            Ves = new HashSet<Ve>();
        }

        public int MaGhe { get; set; }
        public int? MaPc { get; set; }
        public string? MaLg { get; set; }
        public string? Hang { get; set; }
        public int? SoGhe { get; set; }
        public int? ToaDoX { get; set; }
        public int? ToaDoY { get; set; }
        public bool? TrangThai { get; set; }

        public virtual LoaiGhe? MaLgNavigation { get; set; }
        public virtual PhongChieu? MaPcNavigation { get; set; }
        public virtual ICollection<Ve> Ves { get; set; }
    }
}
