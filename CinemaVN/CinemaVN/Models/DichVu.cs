using System;
using System.Collections.Generic;

namespace CinemaVN.Models
{
    public partial class DichVu
    {
        public DichVu()
        {
            ChiTietDichVus = new HashSet<ChiTietDichVu>();
            ChiTietPhieuNhaps = new HashSet<ChiTietPhieuNhap>();
            Khos = new HashSet<Kho>();
        }

        public int MaDv { get; set; }
        public string TenDv { get; set; } = null!;
        public decimal Gia { get; set; }
        public string? LoaiDv { get; set; }
        public string? HinhAnh { get; set; }

        public virtual ICollection<ChiTietDichVu> ChiTietDichVus { get; set; }
        public virtual ICollection<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; }
        public virtual ICollection<Kho> Khos { get; set; }
    }
}
