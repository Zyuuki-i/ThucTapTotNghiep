using CinemaVN.Models;
using System.ComponentModel.DataAnnotations;

namespace CinemaVN.DatModels
{
    public class CChiNhanh
    {
        [Display(Name = "Mã chi nhánh")]
        [Required(ErrorMessage = "Mã chi nhánh không được để trống")]
        public string MaCn { get; set; } = null!;
        [Display(Name = "Tên chi nhánh")]
        [Required(ErrorMessage = "Tên chi nhánh không được để trống")]
        public string? TenCn { get; set; }
        [Display(Name = "Địa chỉ")]
        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        public string? DiaChi { get; set; }
        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Số điện thoại phải gồm 10 chữ số")]
        public string? Sdt { get; set; }

        public List<HoaDon>? HoaDons { get; set; }
        public List<NguoiDung>? NguoiDungs { get; set; }
        public List<PhongChieu>? PhongChieus { get; set; }

        public static CChiNhanh ToCChiNhanh(ChiNhanh? x)
        {
            if (x == null) return null;
            return new CChiNhanh
            {
                MaCn = x.MaCn,
                TenCn = x.TenCn,
                DiaChi = x.DiaChi,
                Sdt = x.Sdt,
                HoaDons = x.HoaDons != null ? x.HoaDons.ToList() : new List<HoaDon>(),
                NguoiDungs = x.NguoiDungs != null ? x.NguoiDungs.ToList() : new List<NguoiDung>(),
                PhongChieus = x.PhongChieus != null ? x.PhongChieus.ToList() : new List<PhongChieu>()
            };
        }

        public static ChiNhanh ToChiNhanh(CChiNhanh x)
        {
            if (x == null) return null;
            return new ChiNhanh
            {
                MaCn = x.MaCn,
                TenCn = x.TenCn,
                DiaChi = x.DiaChi,
                Sdt = x.Sdt,
                HoaDons = x.HoaDons != null ? x.HoaDons.ToList() : new List<HoaDon>(),
                NguoiDungs = x.NguoiDungs != null ? x.NguoiDungs.ToList() : new List<NguoiDung>(),
                PhongChieus = x.PhongChieus != null ? x.PhongChieus.ToList() : new List<PhongChieu>()
            };
        }
    }
}
