using System;
using System.Collections.Generic;

namespace CinemaVN.Models
{
    public partial class TheLoai
    {
        public TheLoai()
        {
            MaPhims = new HashSet<Phim>();
        }

        public int MaTl { get; set; }
        public string? TenTl { get; set; }

        public virtual ICollection<Phim> MaPhims { get; set; }
    }
}
