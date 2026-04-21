using System;
using System.Collections.Generic;

namespace CinemaVN.Models
{
    public partial class DichVu
    {
        public DichVu()
        {
            ChiTietDichVus = new HashSet<ChiTietDichVu>();
        }

        public int MaDv { get; set; }
        public string TenDv { get; set; } = null!;
        public decimal Gia { get; set; }
        public string? LoaiDv { get; set; }
        public string? HinhAnh { get; set; }
        public int? SoLuongTon { get; set; }

        public virtual ICollection<ChiTietDichVu> ChiTietDichVus { get; set; }
    }
}
