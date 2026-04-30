using CinemaVN.Models;
using System.ComponentModel.DataAnnotations;

namespace CinemaVN.DatModels
{
    public class CBanner
    {
        [Display(Name = "Mã")]
        public int MaBn { get; set; }
        [Display(Name = "Tiêu đề")]
        [Required(ErrorMessage = "Tiêu đề không được để trống!")]
        public string? TieuDe { get; set; }
        [Display(Name = "Hình ảnh")]
        [Required(ErrorMessage = "Tên hình không được để trống!")]
        public string HinhAnh { get; set; } = null!;
        [Display(Name = "Đường dẫn")]
        public string? DuongDan { get; set; }
        [Display(Name = "Trạng thái")]
        public bool? TrangThai { get; set; }

        public static CBanner ToCBanner(Banner? bn)
        {
            if(bn == null) return new CBanner();
            return new CBanner
            {
                MaBn = bn.MaBn,
                TieuDe = bn.TieuDe,
                HinhAnh = bn.HinhAnh,
                DuongDan = bn.DuongDan,
                TrangThai = bn.TrangThai
            };
        }

        public static Banner ToBanner(CBanner cbn)
        {
            if(cbn == null) return new Banner();
            return new Banner
            {
                MaBn = cbn.MaBn,
                TieuDe = cbn.TieuDe,
                HinhAnh = cbn.HinhAnh,
                DuongDan = cbn.DuongDan,
                TrangThai = cbn.TrangThai
            };
        }
    }
}
