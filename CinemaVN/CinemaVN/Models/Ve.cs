using System;
using System.Collections.Generic;

namespace CinemaVN.Models
{
    public partial class Ve
    {
        public int MaVe { get; set; }
        public int? MaHd { get; set; }
        public int? MaSc { get; set; }
        public int? MaGhe { get; set; }
        public decimal? Gia { get; set; }
        public int? TrangThai { get; set; }
        public DateTime? ThoiGianGiu { get; set; }
        public string? SessionKey { get; set; }

        public virtual Ghe? MaGheNavigation { get; set; }
        public virtual HoaDon? MaHdNavigation { get; set; }
        public virtual SuatChieu? MaScNavigation { get; set; }
    }
}
