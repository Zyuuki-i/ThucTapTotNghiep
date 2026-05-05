using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CinemaVN.Models
{
    public partial class CinemaVNContext : DbContext
    {
        public CinemaVNContext()
        {
        }

        public CinemaVNContext(DbContextOptions<CinemaVNContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Banner> Banners { get; set; } = null!;
        public virtual DbSet<ChiNhanh> ChiNhanhs { get; set; } = null!;
        public virtual DbSet<ChiTietDichVu> ChiTietDichVus { get; set; } = null!;
        public virtual DbSet<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; } = null!;
        public virtual DbSet<DichVu> DichVus { get; set; } = null!;
        public virtual DbSet<DinhDang> DinhDangs { get; set; } = null!;
        public virtual DbSet<Ghe> Ghes { get; set; } = null!;
        public virtual DbSet<HoaDon> HoaDons { get; set; } = null!;
        public virtual DbSet<Kho> Khos { get; set; } = null!;
        public virtual DbSet<KhuyenMai> KhuyenMais { get; set; } = null!;
        public virtual DbSet<LoaiGhe> LoaiGhes { get; set; } = null!;
        public virtual DbSet<NguoiDung> NguoiDungs { get; set; } = null!;
        public virtual DbSet<PhieuNhap> PhieuNhaps { get; set; } = null!;
        public virtual DbSet<Phim> Phims { get; set; } = null!;
        public virtual DbSet<PhongChieu> PhongChieus { get; set; } = null!;
        public virtual DbSet<SuatChieu> SuatChieus { get; set; } = null!;
        public virtual DbSet<TheLoai> TheLoais { get; set; } = null!;
        public virtual DbSet<VaiTro> VaiTros { get; set; } = null!;
        public virtual DbSet<Ve> Ves { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Data Source=ZYUUKI\\SQLEXPRESS;Initial Catalog=CinemaVN;Integrated Security=True;Encrypt=False");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Banner>(entity =>
            {
                entity.HasKey(e => e.MaBn)
                    .HasName("PK__Banner__272475ADE3C74385");

                entity.ToTable("Banner");

                entity.Property(e => e.MaBn).HasColumnName("MaBN");

                entity.Property(e => e.DuongDan)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.HinhAnh)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.TieuDe).HasMaxLength(100);

                entity.Property(e => e.TrangThai).HasDefaultValueSql("((1))");
            });

            modelBuilder.Entity<ChiNhanh>(entity =>
            {
                entity.HasKey(e => e.MaCn)
                    .HasName("PK__ChiNhanh__27258E0E90E8AE28");

                entity.ToTable("ChiNhanh");

                entity.Property(e => e.MaCn)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("MaCN");

                entity.Property(e => e.DiaChi).HasMaxLength(255);

                entity.Property(e => e.Sdt)
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasColumnName("SDT");

                entity.Property(e => e.TenCn)
                    .HasMaxLength(100)
                    .HasColumnName("TenCN");
            });

            modelBuilder.Entity<ChiTietDichVu>(entity =>
            {
                entity.HasKey(e => new { e.MaHd, e.MaDv })
                    .HasName("PK__ChiTietD__4557FE8536E15A08");

                entity.ToTable("ChiTietDichVu");

                entity.Property(e => e.MaHd).HasColumnName("MaHD");

                entity.Property(e => e.MaDv).HasColumnName("MaDV");

                entity.Property(e => e.DonGia).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.SoLuong).HasDefaultValueSql("((1))");

                entity.Property(e => e.ThanhTien)
                    .HasColumnType("decimal(29, 2)")
                    .HasComputedColumnSql("([DonGia]*[SoLuong])", false);

                entity.HasOne(d => d.MaDvNavigation)
                    .WithMany(p => p.ChiTietDichVus)
                    .HasForeignKey(d => d.MaDv)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ChiTietDic__MaDV__7B5B524B");

                entity.HasOne(d => d.MaHdNavigation)
                    .WithMany(p => p.ChiTietDichVus)
                    .HasForeignKey(d => d.MaHd)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ChiTietDic__MaHD__7A672E12");
            });

            modelBuilder.Entity<ChiTietPhieuNhap>(entity =>
            {
                entity.HasKey(e => new { e.MaPn, e.MaDv })
                    .HasName("PK__ChiTietP__4557BF95233AC9AF");

                entity.ToTable("ChiTietPhieuNhap");

                entity.Property(e => e.MaPn).HasColumnName("MaPN");

                entity.Property(e => e.MaDv).HasColumnName("MaDV");

                entity.Property(e => e.DonGia).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.ThanhTien)
                    .HasColumnType("decimal(29, 2)")
                    .HasComputedColumnSql("([DonGia]*[SoLuong])", false);

                entity.HasOne(d => d.MaDvNavigation)
                    .WithMany(p => p.ChiTietPhieuNhaps)
                    .HasForeignKey(d => d.MaDv)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ChiTietPhi__MaDV__55F4C372");

                entity.HasOne(d => d.MaPnNavigation)
                    .WithMany(p => p.ChiTietPhieuNhaps)
                    .HasForeignKey(d => d.MaPn)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ChiTietPhi__MaPN__55009F39");
            });

            modelBuilder.Entity<DichVu>(entity =>
            {
                entity.HasKey(e => e.MaDv)
                    .HasName("PK__DichVu__272586570F732A80");

                entity.ToTable("DichVu");

                entity.Property(e => e.MaDv).HasColumnName("MaDV");

                entity.Property(e => e.Gia).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.HinhAnh)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.LoaiDv)
                    .HasMaxLength(50)
                    .HasColumnName("LoaiDV");

                entity.Property(e => e.TenDv)
                    .HasMaxLength(50)
                    .HasColumnName("TenDV");
            });

            modelBuilder.Entity<DinhDang>(entity =>
            {
                entity.HasKey(e => e.MaDd)
                    .HasName("PK__DinhDang__27258665145AA0B9");

                entity.ToTable("DinhDang");

                entity.Property(e => e.MaDd)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("MaDD");

                entity.Property(e => e.PhuThu).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.TenDd)
                    .HasMaxLength(20)
                    .HasColumnName("TenDD");
            });

            modelBuilder.Entity<Ghe>(entity =>
            {
                entity.HasKey(e => e.MaGhe)
                    .HasName("PK__Ghe__3CD3C67BCE691F22");

                entity.ToTable("Ghe");

                entity.HasIndex(e => new { e.MaPc, e.Hang, e.SoGhe }, "UQ_Ghe")
                    .IsUnique();

                entity.Property(e => e.Hang)
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.MaLg)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("MaLG");

                entity.Property(e => e.MaPc).HasColumnName("MaPC");

                entity.Property(e => e.TrangThai).HasDefaultValueSql("((1))");

                entity.HasOne(d => d.MaLgNavigation)
                    .WithMany(p => p.Ghes)
                    .HasForeignKey(d => d.MaLg)
                    .HasConstraintName("FK__Ghe__MaLG__628FA481");

                entity.HasOne(d => d.MaPcNavigation)
                    .WithMany(p => p.Ghes)
                    .HasForeignKey(d => d.MaPc)
                    .HasConstraintName("FK__Ghe__MaPC__619B8048");
            });

            modelBuilder.Entity<HoaDon>(entity =>
            {
                entity.HasKey(e => e.MaHd)
                    .HasName("PK__HoaDon__2725A6E03153E350");

                entity.ToTable("HoaDon");

                entity.Property(e => e.MaHd).HasColumnName("MaHD");

                entity.Property(e => e.MaCn)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("MaCN");

                entity.Property(e => e.MaKm).HasColumnName("MaKM");

                entity.Property(e => e.MaNd).HasColumnName("MaND");

                entity.Property(e => e.NgayLap)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.TongTien).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.TrangThai).HasDefaultValueSql("((0))");

                entity.HasOne(d => d.MaCnNavigation)
                    .WithMany(p => p.HoaDons)
                    .HasForeignKey(d => d.MaCn)
                    .HasConstraintName("FK__HoaDon__MaCN__6EF57B66");

                entity.HasOne(d => d.MaKmNavigation)
                    .WithMany(p => p.HoaDons)
                    .HasForeignKey(d => d.MaKm)
                    .HasConstraintName("FK__HoaDon__MaKM__6FE99F9F");

                entity.HasOne(d => d.MaNdNavigation)
                    .WithMany(p => p.HoaDons)
                    .HasForeignKey(d => d.MaNd)
                    .HasConstraintName("FK__HoaDon__MaND__6E01572D");
            });

            modelBuilder.Entity<Kho>(entity =>
            {
                entity.HasKey(e => new { e.MaCn, e.MaDv })
                    .HasName("PK_KhoChiNhanh");

                entity.ToTable("Kho");

                entity.Property(e => e.MaCn)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("MaCN");

                entity.Property(e => e.MaDv).HasColumnName("MaDV");

                entity.Property(e => e.SoLuongTon).HasDefaultValueSql("((0))");

                entity.HasOne(d => d.MaCnNavigation)
                    .WithMany(p => p.Khos)
                    .HasForeignKey(d => d.MaCn)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Kho_ChiNhanh");

                entity.HasOne(d => d.MaDvNavigation)
                    .WithMany(p => p.Khos)
                    .HasForeignKey(d => d.MaDv)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Kho_DichVu");
            });

            modelBuilder.Entity<KhuyenMai>(entity =>
            {
                entity.HasKey(e => e.MaKm)
                    .HasName("PK__KhuyenMa__2725CF1536A53982");

                entity.ToTable("KhuyenMai");

                entity.HasIndex(e => e.MoTa, "UQ__KhuyenMa__A25C5AA7F18A1DF2")
                    .IsUnique();

                entity.Property(e => e.MaKm).HasColumnName("MaKM");

                entity.Property(e => e.Code)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.DieuKien).HasColumnType("decimal(18, 1)");

                entity.Property(e => e.GiamToiDa).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.MoTa).HasMaxLength(150);

                entity.Property(e => e.NgayBd)
                    .HasColumnType("date")
                    .HasColumnName("NgayBD");

                entity.Property(e => e.NgayKt)
                    .HasColumnType("date")
                    .HasColumnName("NgayKT");

                entity.Property(e => e.SoDiem).HasDefaultValueSql("((1))");

                entity.Property(e => e.TrangThai).HasDefaultValueSql("((0))");
            });

            modelBuilder.Entity<LoaiGhe>(entity =>
            {
                entity.HasKey(e => e.MaLg)
                    .HasName("PK__LoaiGhe__2725C77E3451B7C5");

                entity.ToTable("LoaiGhe");

                entity.Property(e => e.MaLg)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("MaLG");

                entity.Property(e => e.PhuThu).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.TenLg)
                    .HasMaxLength(50)
                    .HasColumnName("TenLG");
            });

            modelBuilder.Entity<NguoiDung>(entity =>
            {
                entity.HasKey(e => e.MaNd)
                    .HasName("PK__NguoiDun__2725D7240DC117EF");

                entity.ToTable("NguoiDung");

                entity.HasIndex(e => e.Sdt, "UQ_SDT")
                    .IsUnique();

                entity.HasIndex(e => e.Email, "UQ__NguoiDun__A9D10534DF79E78B")
                    .IsUnique();

                entity.Property(e => e.MaNd).HasColumnName("MaND");

                entity.Property(e => e.Cccd)
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasColumnName("CCCD");

                entity.Property(e => e.ChucVu).HasMaxLength(50);

                entity.Property(e => e.DiaChi).HasMaxLength(255);

                entity.Property(e => e.DiemHoiVien).HasDefaultValueSql("((0))");

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Hinh).HasMaxLength(255);

                entity.Property(e => e.HoTen).HasMaxLength(100);

                entity.Property(e => e.MaCn)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("MaCN");

                entity.Property(e => e.MaVt)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("MaVT");

                entity.Property(e => e.MatKhau)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.NgaySinh).HasColumnType("date");

                entity.Property(e => e.Sdt)
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasColumnName("SDT");

                entity.Property(e => e.TrangThai).HasDefaultValueSql("((1))");

                entity.HasOne(d => d.MaCnNavigation)
                    .WithMany(p => p.NguoiDungs)
                    .HasForeignKey(d => d.MaCn)
                    .HasConstraintName("FK__NguoiDung__MaCN__47DBAE45");

                entity.HasOne(d => d.MaVtNavigation)
                    .WithMany(p => p.NguoiDungs)
                    .HasForeignKey(d => d.MaVt)
                    .HasConstraintName("FK__NguoiDung__MaVT__46E78A0C");
            });

            modelBuilder.Entity<PhieuNhap>(entity =>
            {
                entity.HasKey(e => e.MaPn)
                    .HasName("PK__PhieuNha__2725E7F0A22680F0");

                entity.ToTable("PhieuNhap");

                entity.Property(e => e.MaPn).HasColumnName("MaPN");

                entity.Property(e => e.MaCn)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("MaCN");

                entity.Property(e => e.MaNd).HasColumnName("MaND");

                entity.Property(e => e.NgayNhap)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.NhaCungCap).HasMaxLength(255);

                entity.Property(e => e.TongTien)
                    .HasColumnType("decimal(18, 2)")
                    .HasDefaultValueSql("((0))");

                entity.HasOne(d => d.MaCnNavigation)
                    .WithMany(p => p.PhieuNhaps)
                    .HasForeignKey(d => d.MaCn)
                    .HasConstraintName("FK__PhieuNhap__MaCN__4F47C5E3");

                entity.HasOne(d => d.MaNdNavigation)
                    .WithMany(p => p.PhieuNhaps)
                    .HasForeignKey(d => d.MaNd)
                    .HasConstraintName("FK__PhieuNhap__MaND__503BEA1C");
            });

            modelBuilder.Entity<Phim>(entity =>
            {
                entity.HasKey(e => e.MaPhim)
                    .HasName("PK__Phim__4AC03DE3772FAD73");

                entity.ToTable("Phim");

                entity.Property(e => e.DaoDien).HasMaxLength(100);

                entity.Property(e => e.NgayChieu).HasColumnType("date");

                entity.Property(e => e.Poster)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.TenPhim).HasMaxLength(255);

                entity.Property(e => e.Trailer)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.TrangThai).HasDefaultValueSql("((0))");

                entity.HasMany(d => d.MaTls)
                    .WithMany(p => p.MaPhims)
                    .UsingEntity<Dictionary<string, object>>(
                        "ChiTietTheLoai",
                        l => l.HasOne<TheLoai>().WithMany().HasForeignKey("MaTl").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK__ChiTietThe__MaTL__5DCAEF64"),
                        r => r.HasOne<Phim>().WithMany().HasForeignKey("MaPhim").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK__ChiTietTh__MaPhi__5CD6CB2B"),
                        j =>
                        {
                            j.HasKey("MaPhim", "MaTl").HasName("PK__ChiTietT__48B26DE4EB014B15");

                            j.ToTable("ChiTietTheLoai");

                            j.IndexerProperty<int>("MaTl").HasColumnName("MaTL");
                        });
            });

            modelBuilder.Entity<PhongChieu>(entity =>
            {
                entity.HasKey(e => e.MaPc)
                    .HasName("PK__PhongChi__2725E7E5152EAF4A");

                entity.ToTable("PhongChieu");

                entity.Property(e => e.MaPc).HasColumnName("MaPC");

                entity.Property(e => e.MaCn)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("MaCN");

                entity.Property(e => e.TenPc)
                    .HasMaxLength(50)
                    .HasColumnName("TenPC");

                entity.HasOne(d => d.MaCnNavigation)
                    .WithMany(p => p.PhongChieus)
                    .HasForeignKey(d => d.MaCn)
                    .HasConstraintName("FK__PhongChieu__MaCN__59FA5E80");
            });

            modelBuilder.Entity<SuatChieu>(entity =>
            {
                entity.HasKey(e => e.MaSc)
                    .HasName("PK__SuatChie__27250809CA26F642");

                entity.ToTable("SuatChieu");

                entity.Property(e => e.MaSc).HasColumnName("MaSC");

                entity.Property(e => e.GiaGoc).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.GioBd).HasColumnName("GioBD");

                entity.Property(e => e.GioKt).HasColumnName("GioKT");

                entity.Property(e => e.MaDd)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("MaDD");

                entity.Property(e => e.MaPc).HasColumnName("MaPC");

                entity.Property(e => e.NgayChieu).HasColumnType("date");

                entity.Property(e => e.TrangThai).HasDefaultValueSql("((0))");

                entity.HasOne(d => d.MaDdNavigation)
                    .WithMany(p => p.SuatChieus)
                    .HasForeignKey(d => d.MaDd)
                    .HasConstraintName("FK__SuatChieu__MaDD__68487DD7");

                entity.HasOne(d => d.MaPcNavigation)
                    .WithMany(p => p.SuatChieus)
                    .HasForeignKey(d => d.MaPc)
                    .HasConstraintName("FK__SuatChieu__MaPC__6754599E");

                entity.HasOne(d => d.MaPhimNavigation)
                    .WithMany(p => p.SuatChieus)
                    .HasForeignKey(d => d.MaPhim)
                    .HasConstraintName("FK__SuatChieu__MaPhi__66603565");
            });

            modelBuilder.Entity<TheLoai>(entity =>
            {
                entity.HasKey(e => e.MaTl)
                    .HasName("PK__TheLoai__272500710824C732");

                entity.ToTable("TheLoai");

                entity.Property(e => e.MaTl).HasColumnName("MaTL");

                entity.Property(e => e.TenTl)
                    .HasMaxLength(50)
                    .HasColumnName("TenTL");
            });

            modelBuilder.Entity<VaiTro>(entity =>
            {
                entity.HasKey(e => e.MaVt)
                    .HasName("PK__VaiTro__2725103EE2911703");

                entity.ToTable("VaiTro");

                entity.Property(e => e.MaVt)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("MaVT");

                entity.Property(e => e.TenVt)
                    .HasMaxLength(50)
                    .HasColumnName("TenVT");
            });

            modelBuilder.Entity<Ve>(entity =>
            {
                entity.HasKey(e => e.MaVe)
                    .HasName("PK__Ve__2725100F55FBD4FB");

                entity.ToTable("Ve");

                entity.Property(e => e.Gia).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.MaHd).HasColumnName("MaHD");

                entity.Property(e => e.MaSc).HasColumnName("MaSC");

                entity.Property(e => e.SessionKey)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ThoiGianGiu).HasColumnType("datetime");

                entity.Property(e => e.TrangThai).HasDefaultValueSql("((0))");

                entity.HasOne(d => d.MaGheNavigation)
                    .WithMany(p => p.Ves)
                    .HasForeignKey(d => d.MaGhe)
                    .HasConstraintName("FK__Ve__MaGhe__76969D2E");

                entity.HasOne(d => d.MaHdNavigation)
                    .WithMany(p => p.Ves)
                    .HasForeignKey(d => d.MaHd)
                    .HasConstraintName("FK__Ve__MaHD__74AE54BC");

                entity.HasOne(d => d.MaScNavigation)
                    .WithMany(p => p.Ves)
                    .HasForeignKey(d => d.MaSc)
                    .HasConstraintName("FK__Ve__MaSC__75A278F5");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
