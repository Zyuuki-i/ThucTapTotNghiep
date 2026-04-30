using System;
using System.Collections.Generic;

namespace CinemaVN.Models
{
    public partial class Banner
    {
        public int MaBn { get; set; }
        public string? TieuDe { get; set; }
        public string HinhAnh { get; set; } = null!;
        public string? DuongDan { get; set; }
        public bool? TrangThai { get; set; }
    }
}
