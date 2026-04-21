using System;
using System.Collections.Generic;

namespace CinemaVN.Models
{
    public partial class VaiTro
    {
        public VaiTro()
        {
            NguoiDungs = new HashSet<NguoiDung>();
        }

        public string MaVt { get; set; } = null!;
        public string TenVt { get; set; } = null!;

        public virtual ICollection<NguoiDung> NguoiDungs { get; set; }
    }
}
