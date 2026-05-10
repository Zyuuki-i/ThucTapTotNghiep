using System;
using System.Collections.Generic;

namespace CinemaVN.Models
{
    public partial class ChiTietPhieuNhap
    {
        public int MaPn { get; set; }
        public int MaDv { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal? ThanhTien { get; set; }

        public virtual DichVu MaDvNavigation { get; set; } = null!;
        public virtual PhieuNhap MaPnNavigation { get; set; } = null!;
    }
}
