using CinemaVN.Models;

namespace CinemaVN.DatModels
{
    public class SearchResultViewModel
    {
        public List<Phim> DanhSachPhim { get; set; } = new List<Phim>();
        public List<ChiNhanh> DanhSachRap { get; set; } = new List<ChiNhanh>();
        public string Keyword { get; set; }
        public int tongKQ { get; set; }

    }
}
