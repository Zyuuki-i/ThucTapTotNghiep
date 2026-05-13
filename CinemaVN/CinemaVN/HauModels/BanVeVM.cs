using System.Collections.Generic;

namespace CinemaVN.HauModels
{
    public class BanVeVM
    {
        public int MaSuatChieu { get; set; }

        public int? MaKhuyenMai { get; set; }

        public decimal TongTienSauGiam { get; set; }

        public List<int> DanhSachMaGhe { get; set; } = new();

        public List<DichVuVM> DanhSachDichVu { get; set; } = new();
    }

    public class DichVuVM
    {
        public int MaDv { get; set; }

        public int SoLuong { get; set; }

        public decimal DonGia { get; set; }
    }
}