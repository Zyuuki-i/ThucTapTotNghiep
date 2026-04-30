using System;
using System.Collections.Generic;

namespace CinemaVN.Models
{
    public partial class Kho
    {
        public string MaCn { get; set; } = null!;
        public int MaDv { get; set; }
        public int? SoLuongTon { get; set; }

        public virtual ChiNhanh MaCnNavigation { get; set; } = null!;
        public virtual DichVu MaDvNavigation { get; set; } = null!;
    }
}
