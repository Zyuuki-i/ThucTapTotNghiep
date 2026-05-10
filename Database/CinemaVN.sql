IF DB_ID('CinemaVN') IS NOT NULL
BEGIN
    ALTER DATABASE CinemaVN SET SINGLE_USER WITH ROLLBACK IMMEDIATE
    DROP DATABASE CinemaVN
END
GO

CREATE DATABASE CinemaVN
GO

USE CinemaVN;
GO

CREATE TABLE VaiTro (
    MaVT VARCHAR(20) PRIMARY KEY, -- ADMIN, MANAGER, STAFF, CUSTOMER
    TenVT NVARCHAR(50) NOT NULL
);

CREATE TABLE ChiNhanh (
    MaCN VARCHAR(10) PRIMARY KEY, -- CNQ7,CNQ8
    TenCN NVARCHAR(100),
    DiaChi NVARCHAR(255),
    SDT VARCHAR(15)
);

CREATE TABLE TheLoai (
    MaTL INT PRIMARY KEY IDENTITY(3,3),
    TenTL NVARCHAR(50)
);

CREATE TABLE DinhDang (
    MaDD VARCHAR(10) PRIMARY KEY, -- 2D, 3D, IMAX
    TenDD NVARCHAR(20),
    PhuThu DECIMAL(18,2)
);

CREATE TABLE LoaiGhe (
    MaLG VARCHAR(10) PRIMARY KEY, -- VIP, DOI
    TenLG NVARCHAR(50),
    PhuThu DECIMAL(18,2)
);

CREATE TABLE DichVu (
    MaDV INT PRIMARY KEY IDENTITY(100,11), -- TỰ TĂNG
    TenDV NVARCHAR(50) NOT NULL,
    Gia DECIMAL(18, 2) NOT NULL,
    LoaiDV NVARCHAR(50),
    HinhAnh VARCHAR(255),
	CONSTRAINT CK_DV_Gia CHECK (Gia >= 0)
);

CREATE TABLE Kho (
    MaCN VARCHAR(10) NOT NULL,
    MaDV INT NOT NULL,
    SoLuongTon INT DEFAULT 0,
    CONSTRAINT PK_KhoChiNhanh PRIMARY KEY (MaCN, MaDV),
    CONSTRAINT FK_Kho_ChiNhanh FOREIGN KEY (MaCN) REFERENCES ChiNhanh(MaCN),
    CONSTRAINT FK_Kho_DichVu FOREIGN KEY (MaDV) REFERENCES DichVu(MaDV),
    CONSTRAINT CK_Kho_Ton CHECK (SoLuongTon >= 0)
);
GO

CREATE TABLE NguoiDung (
    MaND INT PRIMARY KEY IDENTITY(1000,8), 
    MaVT VARCHAR(20) FOREIGN KEY REFERENCES VaiTro(MaVT),
    MaCN VARCHAR(10) FOREIGN KEY REFERENCES ChiNhanh(MaCN),
    Email VARCHAR(100) UNIQUE NOT NULL,
    MatKhau VARCHAR(255) NOT NULL,
	Hinh NVARCHAR(255) NULL,
    HoTen NVARCHAR(100),
    NgaySinh DATE,
    Phai BIT,
    SDT VARCHAR(15),
    CCCD VARCHAR(15),
    DiaChi NVARCHAR(255),
    ChucVu NVARCHAR(50),
    DiemHoiVien INT DEFAULT 0,
    TrangThai BIT DEFAULT 1, --MỞ/ĐÓNG
	CONSTRAINT UQ_SDT UNIQUE (SDT)
);

CREATE TABLE KhuyenMai (
    MaKM INT PRIMARY KEY IDENTITY(10,10),
    Code VARCHAR(20) UNIQUE NOT NULL,
	MoTa NVARCHAR(150),
    DieuKien DECIMAL(18,1),
    SoDiem INT DEFAULT 1,
    PhanTramGiam INT,
    GiamToiDa DECIMAL(18, 2),
    NgayBD DATE,
    NgayKT DATE,
	TrangThai INT DEFAULT 0,
	-- 0: Sắp diễn ra
	-- 1: Hiệu lực
	-- 2: Hết hạn
	CONSTRAINT CK_KM_Ngay CHECK (NgayKT >= NgayBD),
	CONSTRAINT CK_KM_Giam CHECK (GiamToiDa >= 0),
	CONSTRAINT CK_KM_PhanTram CHECK (PhanTramGiam BETWEEN 0 AND 100)
);

CREATE TABLE Banner (
    MaBN INT PRIMARY KEY IDENTITY(1,1),
    TieuDe NVARCHAR(100),
    HinhAnh VARCHAR(255) NOT NULL,
    DuongDan VARCHAR(255),
    TrangThai BIT DEFAULT 1 --HIỆN/ẨN
);

CREATE TABLE Phim (
    MaPhim INT PRIMARY KEY IDENTITY(10000,2),
    TenPhim NVARCHAR(255) NOT NULL,
    MoTa NVARCHAR(MAX),
    ThoiLuong INT,
    NgayChieu DATE,
    DaoDien NVARCHAR(100),
    Poster VARCHAR(255),
    Trailer VARCHAR(255),
    DoTuoi INT,
    TrangThai INT DEFAULT 0
	-- 0: sắp chiếu
	-- 1: đang chiếu
	-- 2: ngừng chiếu
);

CREATE TABLE PhongChieu (
    MaPC INT IDENTITY(100,1) PRIMARY KEY,
    TenPC NVARCHAR(50),
    MaCN VARCHAR(10) FOREIGN KEY REFERENCES ChiNhanh(MaCN),
    SucChua INT
);

CREATE TABLE ChiTietTheLoai (
    MaPhim INT FOREIGN KEY REFERENCES Phim(MaPhim),
    MaTL INT FOREIGN KEY REFERENCES TheLoai(MaTL),
    PRIMARY KEY (MaPhim, MaTL)
);

CREATE TABLE Ghe (
    MaGhe INT IDENTITY PRIMARY KEY,
    MaPC INT FOREIGN KEY REFERENCES PhongChieu(MaPC),
    MaLG VARCHAR(10) FOREIGN KEY REFERENCES LoaiGhe(MaLG),
    Hang VARCHAR(5),
    SoGhe INT,
    ToaDoX INT,
    ToaDoY INT,
    TrangThai BIT DEFAULT 1, --Hoạt động/Hư hỏng
	CONSTRAINT UQ_Ghe UNIQUE (MaPC, Hang, SoGhe)
);

CREATE TABLE SuatChieu (
    MaSC INT PRIMARY KEY IDENTITY(100,1),
    MaPhim INT FOREIGN KEY REFERENCES Phim(MaPhim),
    MaPC INT FOREIGN KEY REFERENCES PhongChieu(MaPC),
    MaDD VARCHAR(10) FOREIGN KEY REFERENCES DinhDang(MaDD),
    NgayChieu DATE,
    GioBD TIME,
    GioKT TIME,
    GiaGoc DECIMAL(18,2),
    TrangThai INT DEFAULT 0,
	-- 0: sắp mở (chưa thể đặt vé)
	-- 1: đang mở (cho phép đặt vé)
	-- 2: đã chiếu (không thể đặt vé)
	-- 3: đã hủy (lỗi kỹ thuật/dời lịch)
    CONSTRAINT CHK_Gio CHECK (GioKT > GioBD),
	CONSTRAINT CK_SC_Gia CHECK (GiaGoc >= 0)
);

-- 15. Hóa đơn
CREATE TABLE HoaDon (
    MaHD INT IDENTITY(100,1) PRIMARY KEY,
    MaND INT FOREIGN KEY REFERENCES NguoiDung(MaND),
    MaCN VARCHAR(10) FOREIGN KEY REFERENCES ChiNhanh(MaCN),
    MaKM INT FOREIGN KEY REFERENCES KhuyenMai(MaKM) NULL,
    NgayLap DATETIME DEFAULT GETDATE(),
    TongTien DECIMAL(18,2),
    TrangThai INT DEFAULT 0
	-- 0: Chưa thanh toán
	-- 1: Đã thanh toán
	-- 2: Đã hủy
	-- 3: Hoàn tiền
);

-- 16. Vé xem phim
CREATE TABLE Ve (
    MaVe INT IDENTITY(900,1) PRIMARY KEY,
    MaHD INT FOREIGN KEY REFERENCES HoaDon(MaHD),
    MaSC INT FOREIGN KEY REFERENCES SuatChieu(MaSC),
    MaGhe INT FOREIGN KEY REFERENCES Ghe(MaGhe),
    Gia DECIMAL(18,2),
    TrangThai INT DEFAULT 0,
	ThoiGianGiu DATETIME,
	SessionKey VARCHAR(100)
	-- 0: Chưa thanh toán (giữ ghế)
	-- 1: Đã thanh toán (chưa soát vé)
	-- 2: Đã dùng (đã soát vé)
	-- 3: Hết hạn
	-- 4: Đã hủy (hủy/hoàn tiền)
);

-- 17. Chi tiết dịch vụ
CREATE TABLE ChiTietDichVu (
    MaHD INT FOREIGN KEY REFERENCES HoaDon(MaHD),
    MaDV INT FOREIGN KEY REFERENCES DichVu(MaDV),
    DonGia DECIMAL(18, 2),
    SoLuong INT NOT NULL DEFAULT 1,
    ThanhTien AS (DonGia * SoLuong),
    PRIMARY KEY (MaHD, MaDV)
);
GO

CREATE TABLE PhieuNhap (
    MaPN INT IDENTITY(100,1) PRIMARY KEY,
    MaCN VARCHAR(10) FOREIGN KEY REFERENCES ChiNhanh(MaCN), 
    MaND INT FOREIGN KEY REFERENCES NguoiDung(MaND),        
    NhaCungCap NVARCHAR(255),                               
    NgayNhap DATETIME DEFAULT GETDATE(),
    TongTien DECIMAL(18,2) DEFAULT 0,
	TrangThai INT DEFAULT 0
);
GO

CREATE TABLE ChiTietPhieuNhap (
    MaPN INT FOREIGN KEY REFERENCES PhieuNhap(MaPN),
    MaDV INT FOREIGN KEY REFERENCES DichVu(MaDV),           
    SoLuong INT NOT NULL,
    DonGia DECIMAL(18, 2) NOT NULL,							
    ThanhTien AS (DonGia * SoLuong),						
    PRIMARY KEY (MaPN, MaDV),
    CONSTRAINT CK_CTPN_SoLuong CHECK (SoLuong > 0),
    CONSTRAINT CK_CTPN_DonGia CHECK (DonGia >= 0)
);
GO
SET IDENTITY_INSERT [dbo].[Banner] ON 
INSERT [dbo].[Banner] ([MaBN], [TieuDe], [HinhAnh], [DuongDan], [TrangThai]) VALUES (1, N'Tưng Bừng Khai Trương', N'cinemavn-grand-opening.png', N'https://www.facebook.com/tuyendungcgv.vn/', 1)
INSERT [dbo].[Banner] ([MaBN], [TieuDe], [HinhAnh], [DuongDan], [TrangThai]) VALUES (2, N'Movie Time', N'cinemavn-movie-time.jpg', NULL, 1)
INSERT [dbo].[Banner] ([MaBN], [TieuDe], [HinhAnh], [DuongDan], [TrangThai]) VALUES (3, N'Movie Night', N'cinemavn-movie-night.png', NULL, 1)
INSERT [dbo].[Banner] ([MaBN], [TieuDe], [HinhAnh], [DuongDan], [TrangThai]) VALUES (4, N'Quà tặng mỗi ngày', N'cinemavn-popcorn-gift.jpg', NULL, 0)
SET IDENTITY_INSERT [dbo].[Banner] OFF
GO

INSERT [dbo].[ChiNhanh] ([MaCN], [TenCN], [DiaChi], [SDT]) VALUES (N'CNQ1', N'CinemaVN Bến Thành', N'1 Lê Lợi, Phường Bến Thành, Quận 1, TP.HCM', N'0988333883')
INSERT [dbo].[ChiNhanh] ([MaCN], [TenCN], [DiaChi], [SDT]) VALUES (N'CNQ7', N'CinemaVN Phú Mỹ Hưng', N'123 Đại lộ Nguyễn Văn Linh, Quận 7, TP.HCM', N'0988333882')
INSERT [dbo].[ChiNhanh] ([MaCN], [TenCN], [DiaChi], [SDT]) VALUES (N'CNQ8', N'CinemaVN Cao Lỗ', N'222 Cao Lỗ, Phường 4, Quận 8, TP.HCM', N'0988333881')
GO

INSERT [dbo].[ChiTietDichVu] ([MaHD], [MaDV], [DonGia], [SoLuong]) VALUES (100, 133, CAST(75000.00 AS Decimal(18, 2)), 1)
INSERT [dbo].[ChiTietDichVu] ([MaHD], [MaDV], [DonGia], [SoLuong]) VALUES (111, 100, CAST(45000.00 AS Decimal(18, 2)), 1)
INSERT [dbo].[ChiTietDichVu] ([MaHD], [MaDV], [DonGia], [SoLuong]) VALUES (111, 122, CAST(30000.00 AS Decimal(18, 2)), 2)
INSERT [dbo].[ChiTietDichVu] ([MaHD], [MaDV], [DonGia], [SoLuong]) VALUES (112, 144, CAST(100000.00 AS Decimal(18, 2)), 1)
INSERT [dbo].[ChiTietDichVu] ([MaHD], [MaDV], [DonGia], [SoLuong]) VALUES (116, 144, CAST(100000.00 AS Decimal(18, 2)), 1)
INSERT [dbo].[ChiTietDichVu] ([MaHD], [MaDV], [DonGia], [SoLuong]) VALUES (120, 155, CAST(150000.00 AS Decimal(18, 2)), 1)
INSERT [dbo].[ChiTietDichVu] ([MaHD], [MaDV], [DonGia], [SoLuong]) VALUES (121, 100, CAST(45000.00 AS Decimal(18, 2)), 1)
INSERT [dbo].[ChiTietDichVu] ([MaHD], [MaDV], [DonGia], [SoLuong]) VALUES (121, 122, CAST(30000.00 AS Decimal(18, 2)), 1)
INSERT [dbo].[ChiTietDichVu] ([MaHD], [MaDV], [DonGia], [SoLuong]) VALUES (124, 100, CAST(45000.00 AS Decimal(18, 2)), 2)
GO

INSERT [dbo].[ChiTietTheLoai] ([MaPhim], [MaTL]) VALUES (10000, 3)
INSERT [dbo].[ChiTietTheLoai] ([MaPhim], [MaTL]) VALUES (10000, 18)
INSERT [dbo].[ChiTietTheLoai] ([MaPhim], [MaTL]) VALUES (10002, 3)
INSERT [dbo].[ChiTietTheLoai] ([MaPhim], [MaTL]) VALUES (10004, 9)
INSERT [dbo].[ChiTietTheLoai] ([MaPhim], [MaTL]) VALUES (10006, 15)
INSERT [dbo].[ChiTietTheLoai] ([MaPhim], [MaTL]) VALUES (10008, 3)
INSERT [dbo].[ChiTietTheLoai] ([MaPhim], [MaTL]) VALUES (10010, 6)
INSERT [dbo].[ChiTietTheLoai] ([MaPhim], [MaTL]) VALUES (10010, 12)
INSERT [dbo].[ChiTietTheLoai] ([MaPhim], [MaTL]) VALUES (10012, 18)
INSERT [dbo].[ChiTietTheLoai] ([MaPhim], [MaTL]) VALUES (10014, 3)
INSERT [dbo].[ChiTietTheLoai] ([MaPhim], [MaTL]) VALUES (10014, 18)
INSERT [dbo].[ChiTietTheLoai] ([MaPhim], [MaTL]) VALUES (10016, 3)
INSERT [dbo].[ChiTietTheLoai] ([MaPhim], [MaTL]) VALUES (10016, 15)
INSERT [dbo].[ChiTietTheLoai] ([MaPhim], [MaTL]) VALUES (10018, 15)
INSERT [dbo].[ChiTietTheLoai] ([MaPhim], [MaTL]) VALUES (10020, 18)
INSERT [dbo].[ChiTietTheLoai] ([MaPhim], [MaTL]) VALUES (10020, 21)
INSERT [dbo].[ChiTietTheLoai] ([MaPhim], [MaTL]) VALUES (10020, 24)
INSERT [dbo].[ChiTietTheLoai] ([MaPhim], [MaTL]) VALUES (10022, 3)
INSERT [dbo].[ChiTietTheLoai] ([MaPhim], [MaTL]) VALUES (10022, 9)
INSERT [dbo].[ChiTietTheLoai] ([MaPhim], [MaTL]) VALUES (10024, 3)
GO

SET IDENTITY_INSERT [dbo].[DichVu] ON 
INSERT [dbo].[DichVu] ([MaDV], [TenDV], [Gia], [LoaiDV], [HinhAnh]) VALUES (100, N'Bắp nhỏ', CAST(45000.00 AS Decimal(18, 2)), N'Bắp', NULL)
INSERT [dbo].[DichVu] ([MaDV], [TenDV], [Gia], [LoaiDV], [HinhAnh]) VALUES (111, N'Bắp lớn', CAST(65000.00 AS Decimal(18, 2)), N'Bắp', NULL)
INSERT [dbo].[DichVu] ([MaDV], [TenDV], [Gia], [LoaiDV], [HinhAnh]) VALUES (122, N'Nước ngọt', CAST(30000.00 AS Decimal(18, 2)), N'Nước', NULL)
INSERT [dbo].[DichVu] ([MaDV], [TenDV], [Gia], [LoaiDV], [HinhAnh]) VALUES (133, N'Combo 1 (Bắp nhỏ + Nước)', CAST(75000.00 AS Decimal(18, 2)), N'Combo', NULL)
INSERT [dbo].[DichVu] ([MaDV], [TenDV], [Gia], [LoaiDV], [HinhAnh]) VALUES (144, N'Combo 2 (Bắp lớn + Nước)', CAST(100000.00 AS Decimal(18, 2)), N'Combo', NULL)
INSERT [dbo].[DichVu] ([MaDV], [TenDV], [Gia], [LoaiDV], [HinhAnh]) VALUES (155, N'Combo đôi ta', CAST(150000.00 AS Decimal(18, 2)), N'Combo', NULL)
SET IDENTITY_INSERT [dbo].[DichVu] OFF
GO

INSERT [dbo].[DinhDang] ([MaDD], [TenDD], [PhuThu]) VALUES (N'2D', N'2D tiêu chuẩn', CAST(0.00 AS Decimal(18, 2)))
INSERT [dbo].[DinhDang] ([MaDD], [TenDD], [PhuThu]) VALUES (N'3D', N'3D kỹ thuật số', CAST(30000.00 AS Decimal(18, 2)))
INSERT [dbo].[DinhDang] ([MaDD], [TenDD], [PhuThu]) VALUES (N'IMAX', N'IMAX siêu thực', CAST(60000.00 AS Decimal(18, 2)))
GO

SET IDENTITY_INSERT [dbo].[Ghe] ON 
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (1, 100, N'THUONG', N'A', 1, 1, 1, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (2, 100, N'THUONG', N'A', 2, 2, 1, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (3, 100, N'THUONG', N'A', 3, 3, 1, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (4, 100, N'THUONG', N'A', 4, 4, 1, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (5, 100, N'THUONG', N'A', 5, 5, 1, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (11, 100, N'THUONG', N'B', 1, 1, 2, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (12, 100, N'DOI', N'B', 2, 2, 2, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (13, 100, N'THUONG', N'F', 4, 5, 6, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (14, 100, N'THUONG', N'B', 3, 4, 2, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (15, 100, N'THUONG', N'B', 4, 5, 2, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (23, 100, N'THUONG', N'C', 1, 1, 3, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (24, 100, N'VIP', N'C', 2, 2, 3, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (25, 100, N'DOI', N'C', 3, 3, 3, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (26, 100, N'VIP', N'E', 3, 3, 5, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (27, 100, N'THUONG', N'C', 4, 5, 3, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (31, 100, N'THUONG', N'D', 1, 1, 4, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (32, 100, N'VIP', N'D', 2, 2, 4, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (33, 100, N'VIP', N'D', 3, 3, 4, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (34, 100, N'VIP', N'D', 4, 4, 4, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (35, 100, N'THUONG', N'D', 5, 5, 4, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (41, 100, N'THUONG', N'E', 1, 1, 5, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (42, 100, N'THUONG', N'E', 2, 2, 5, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (76, 100, N'DOI', N'E', 4, 4, 5, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (77, 100, N'THUONG', N'F', 1, 1, 6, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (78, 100, N'DOI', N'F', 2, 2, 6, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (79, 100, N'THUONG', N'F', 3, 4, 6, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (80, 101, N'DOI', N'A', 1, 1, 1, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (82, 101, N'THUONG', N'A', 2, 3, 1, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (83, 101, N'THUONG', N'A', 3, 4, 1, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (84, 101, N'THUONG', N'A', 4, 5, 1, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (85, 101, N'THUONG', N'B', 1, 1, 2, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (86, 101, N'VIP', N'B', 2, 2, 2, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (87, 101, N'VIP', N'B', 3, 3, 2, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (88, 101, N'VIP', N'B', 4, 4, 2, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (89, 101, N'THUONG', N'B', 5, 5, 2, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (90, 101, N'THUONG', N'C', 1, 1, 3, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (91, 101, N'DOI', N'C', 2, 2, 3, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (93, 101, N'VIP', N'C', 3, 4, 3, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (94, 101, N'THUONG', N'C', 4, 5, 3, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (95, 101, N'THUONG', N'D', 1, 1, 4, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (96, 101, N'VIP', N'D', 2, 2, 4, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (97, 101, N'VIP', N'D', 3, 3, 4, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (98, 101, N'DOI', N'D', 4, 4, 4, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (100, 101, N'THUONG', N'E', 1, 1, 5, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (101, 101, N'VIP', N'E', 2, 2, 5, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (102, 101, N'VIP', N'E', 3, 3, 5, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (103, 101, N'VIP', N'E', 4, 4, 5, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (104, 101, N'THUONG', N'E', 5, 5, 5, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (105, 101, N'DOI', N'F', 1, 1, 6, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (106, 101, N'DOI', N'F', 3, 3, 6, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (107, 101, N'THUONG', N'F', 5, 5, 6, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (108, 102, N'THUONG', N'A', 1, 1, 1, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (109, 102, N'DOI', N'A', 2, 2, 1, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (111, 102, N'VIP', N'A', 3, 4, 1, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (112, 102, N'THUONG', N'A', 4, 5, 1, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (113, 102, N'THUONG', N'B', 1, 1, 2, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (114, 102, N'VIP', N'B', 2, 2, 2, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (115, 102, N'DOI', N'B', 3, 3, 2, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (117, 102, N'THUONG', N'B', 4, 5, 2, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (118, 102, N'THUONG', N'C', 1, 1, 3, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (119, 102, N'VIP', N'C', 2, 2, 3, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (120, 102, N'VIP', N'C', 3, 3, 3, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (121, 102, N'VIP', N'C', 4, 4, 3, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (122, 102, N'THUONG', N'C', 5, 5, 3, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (123, 102, N'DOI', N'D', 1, 1, 4, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (124, 102, N'DOI', N'D', 3, 3, 4, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (125, 102, N'THUONG', N'D', 5, 5, 4, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (126, 103, N'THUONG', N'A', 1, 1, 1, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (127, 103, N'VIP', N'A', 2, 2, 1, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (128, 103, N'DOI', N'A', 3, 3, 1, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (130, 103, N'THUONG', N'A', 4, 5, 1, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (131, 103, N'THUONG', N'B', 1, 1, 2, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (132, 103, N'VIP', N'B', 2, 2, 2, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (134, 103, N'DOI', N'B', 3, 4, 2, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (136, 103, N'THUONG', N'C', 1, 1, 3, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (137, 103, N'DOI', N'C', 2, 2, 3, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (139, 103, N'VIP', N'C', 3, 4, 3, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (140, 103, N'THUONG', N'C', 4, 5, 3, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (141, 103, N'THUONG', N'D', 1, 1, 4, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (142, 103, N'VIP', N'D', 2, 2, 4, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (143, 103, N'DOI', N'D', 3, 3, 4, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (146, 103, N'THUONG', N'E', 1, 1, 5, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (147, 103, N'VIP', N'E', 2, 2, 5, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (148, 103, N'VIP', N'E', 3, 3, 5, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (149, 103, N'VIP', N'E', 4, 4, 5, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (150, 103, N'THUONG', N'E', 5, 5, 5, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (151, 103, N'DOI', N'F', 1, 1, 6, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (152, 103, N'DOI', N'F', 3, 3, 6, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (153, 103, N'THUONG', N'F', 5, 5, 6, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (154, 104, N'THUONG', N'A', 1, 1, 1, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (155, 104, N'THUONG', N'A', 2, 2, 1, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (156, 104, N'THUONG', N'A', 3, 3, 1, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (157, 104, N'THUONG', N'A', 4, 4, 1, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (158, 104, N'THUONG', N'A', 5, 5, 1, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (159, 104, N'THUONG', N'B', 1, 1, 2, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (160, 104, N'VIP', N'B', 2, 2, 2, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (161, 104, N'VIP', N'B', 3, 3, 2, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (162, 104, N'VIP', N'B', 4, 4, 2, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (163, 104, N'THUONG', N'B', 5, 5, 2, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (164, 104, N'THUONG', N'C', 1, 1, 3, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (165, 104, N'VIP', N'C', 2, 2, 3, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (166, 104, N'VIP', N'C', 3, 3, 3, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (167, 104, N'VIP', N'C', 4, 4, 3, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (168, 104, N'THUONG', N'C', 5, 5, 3, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (169, 104, N'DOI', N'D', 1, 1, 4, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (170, 104, N'DOI', N'D', 3, 3, 4, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (171, 104, N'THUONG', N'D', 5, 5, 4, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (172, 105, N'THUONG', N'A', 1, 1, 1, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (173, 105, N'THUONG', N'A', 2, 2, 1, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (174, 105, N'THUONG', N'A', 3, 3, 1, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (175, 105, N'THUONG', N'A', 4, 4, 1, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (176, 105, N'THUONG', N'A', 5, 5, 1, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (177, 105, N'THUONG', N'B', 1, 1, 2, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (178, 105, N'VIP', N'B', 2, 2, 2, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (179, 105, N'VIP', N'B', 3, 3, 2, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (180, 105, N'VIP', N'B', 4, 4, 2, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (181, 105, N'THUONG', N'B', 5, 5, 2, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (182, 105, N'THUONG', N'C', 1, 1, 3, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (183, 105, N'VIP', N'C', 2, 2, 3, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (184, 105, N'VIP', N'C', 3, 3, 3, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (185, 105, N'VIP', N'C', 4, 4, 3, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (186, 105, N'THUONG', N'C', 5, 5, 3, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (187, 105, N'THUONG', N'D', 1, 1, 4, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (188, 105, N'VIP', N'D', 2, 2, 4, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (189, 105, N'VIP', N'D', 3, 3, 4, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (190, 105, N'VIP', N'D', 4, 4, 4, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (191, 105, N'THUONG', N'D', 5, 5, 4, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (192, 105, N'THUONG', N'E', 1, 1, 5, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (193, 105, N'VIP', N'E', 2, 2, 5, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (194, 105, N'VIP', N'E', 3, 3, 5, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (195, 105, N'VIP', N'E', 4, 4, 5, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (196, 105, N'THUONG', N'E', 5, 5, 5, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (197, 105, N'DOI', N'F', 1, 1, 6, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (198, 105, N'DOI', N'F', 3, 3, 6, 1)
INSERT [dbo].[Ghe] ([MaGhe], [MaPC], [MaLG], [Hang], [SoGhe], [ToaDoX], [ToaDoY], [TrangThai]) VALUES (199, 105, N'THUONG', N'F', 5, 5, 6, 1)
SET IDENTITY_INSERT [dbo].[Ghe] OFF
GO

SET IDENTITY_INSERT [dbo].[HoaDon] ON 
INSERT [dbo].[HoaDon] ([MaHD], [MaND], [MaCN], [MaKM], [NgayLap], [TongTien], [TrangThai]) VALUES (100, 1176, N'CNQ1', NULL, CAST(N'2026-05-01T07:30:00.000' AS DateTime), CAST(235000.00 AS Decimal(18, 2)), 1)
INSERT [dbo].[HoaDon] ([MaHD], [MaND], [MaCN], [MaKM], [NgayLap], [TongTien], [TrangThai]) VALUES (101, 1184, N'CNQ8', 10, CAST(N'2026-05-01T18:00:00.000' AS DateTime), CAST(152000.00 AS Decimal(18, 2)), 1)
INSERT [dbo].[HoaDon] ([MaHD], [MaND], [MaCN], [MaKM], [NgayLap], [TongTien], [TrangThai]) VALUES (110, 1224, N'CNQ7', NULL, CAST(N'2026-05-02T23:18:13.757' AS DateTime), CAST(100000.00 AS Decimal(18, 2)), 1)
INSERT [dbo].[HoaDon] ([MaHD], [MaND], [MaCN], [MaKM], [NgayLap], [TongTien], [TrangThai]) VALUES (111, 1224, N'CNQ7', NULL, CAST(N'2026-05-02T23:28:44.917' AS DateTime), CAST(365000.00 AS Decimal(18, 2)), 1)
INSERT [dbo].[HoaDon] ([MaHD], [MaND], [MaCN], [MaKM], [NgayLap], [TongTien], [TrangThai]) VALUES (112, 1224, N'CNQ7', NULL, CAST(N'2026-05-02T23:39:27.897' AS DateTime), CAST(330000.00 AS Decimal(18, 2)), 1)
INSERT [dbo].[HoaDon] ([MaHD], [MaND], [MaCN], [MaKM], [NgayLap], [TongTien], [TrangThai]) VALUES (116, 1224, N'CNQ1', NULL, CAST(N'2026-05-03T01:17:22.653' AS DateTime), CAST(100000.00 AS Decimal(18, 2)), 1)
INSERT [dbo].[HoaDon] ([MaHD], [MaND], [MaCN], [MaKM], [NgayLap], [TongTien], [TrangThai]) VALUES (120, 1224, N'CNQ7', 30, CAST(N'2026-05-03T12:37:18.993' AS DateTime), CAST(340000.00 AS Decimal(18, 2)), 1)
INSERT [dbo].[HoaDon] ([MaHD], [MaND], [MaCN], [MaKM], [NgayLap], [TongTien], [TrangThai]) VALUES (121, 1224, N'CNQ1', 20, CAST(N'2026-05-03T12:53:50.643' AS DateTime), CAST(255000.00 AS Decimal(18, 2)), 1)
INSERT [dbo].[HoaDon] ([MaHD], [MaND], [MaCN], [MaKM], [NgayLap], [TongTien], [TrangThai]) VALUES (124, 1224, N'CNQ1', NULL, CAST(N'2026-05-05T22:17:54.293' AS DateTime), CAST(220000.00 AS Decimal(18, 2)), 1)
SET IDENTITY_INSERT [dbo].[HoaDon] OFF
GO

INSERT [dbo].[Kho] ([MaCN], [MaDV], [SoLuongTon]) VALUES (N'CNQ1', 100, 28)
INSERT [dbo].[Kho] ([MaCN], [MaDV], [SoLuongTon]) VALUES (N'CNQ1', 111, 40)
INSERT [dbo].[Kho] ([MaCN], [MaDV], [SoLuongTon]) VALUES (N'CNQ1', 122, 100)
INSERT [dbo].[Kho] ([MaCN], [MaDV], [SoLuongTon]) VALUES (N'CNQ1', 133, 20)
INSERT [dbo].[Kho] ([MaCN], [MaDV], [SoLuongTon]) VALUES (N'CNQ1', 144, 25)
INSERT [dbo].[Kho] ([MaCN], [MaDV], [SoLuongTon]) VALUES (N'CNQ1', 155, 10)
INSERT [dbo].[Kho] ([MaCN], [MaDV], [SoLuongTon]) VALUES (N'CNQ7', 100, 40)
INSERT [dbo].[Kho] ([MaCN], [MaDV], [SoLuongTon]) VALUES (N'CNQ7', 111, 30)
INSERT [dbo].[Kho] ([MaCN], [MaDV], [SoLuongTon]) VALUES (N'CNQ7', 122, 50)
INSERT [dbo].[Kho] ([MaCN], [MaDV], [SoLuongTon]) VALUES (N'CNQ7', 133, 15)
INSERT [dbo].[Kho] ([MaCN], [MaDV], [SoLuongTon]) VALUES (N'CNQ7', 144, 20)
INSERT [dbo].[Kho] ([MaCN], [MaDV], [SoLuongTon]) VALUES (N'CNQ7', 155, 10)
INSERT [dbo].[Kho] ([MaCN], [MaDV], [SoLuongTon]) VALUES (N'CNQ8', 100, 35)
INSERT [dbo].[Kho] ([MaCN], [MaDV], [SoLuongTon]) VALUES (N'CNQ8', 111, 30)
INSERT [dbo].[Kho] ([MaCN], [MaDV], [SoLuongTon]) VALUES (N'CNQ8', 122, 50)
INSERT [dbo].[Kho] ([MaCN], [MaDV], [SoLuongTon]) VALUES (N'CNQ8', 133, 15)
INSERT [dbo].[Kho] ([MaCN], [MaDV], [SoLuongTon]) VALUES (N'CNQ8', 144, 25)
INSERT [dbo].[Kho] ([MaCN], [MaDV], [SoLuongTon]) VALUES (N'CNQ8', 155, 10)
GO

SET IDENTITY_INSERT [dbo].[KhuyenMai] ON 
INSERT [dbo].[KhuyenMai] ([MaKM], [Code], [MoTa], [DieuKien], [SoDiem], [PhanTramGiam], [GiamToiDa], [NgayBD], [NgayKT], [TrangThai]) VALUES (10, N'UUDAI5', N'Đổi 100 điểm lấy mã giảm 5% (Tối đa 20k) cho đơn từ 100K', CAST(100000.0 AS Decimal(18, 1)), 100, 5, CAST(20000.00 AS Decimal(18, 2)), CAST(N'2026-01-01' AS Date), CAST(N'2026-12-31' AS Date), 1)
INSERT [dbo].[KhuyenMai] ([MaKM], [Code], [MoTa], [DieuKien], [SoDiem], [PhanTramGiam], [GiamToiDa], [NgayBD], [NgayKT], [TrangThai]) VALUES (20, N'UUDAI20', N'Đổi 300 điểm lấy mã giảm 20% (Tối đa 50k) cho đơn từ 200K', CAST(200000.0 AS Decimal(18, 1)), 300, 20, CAST(50000.00 AS Decimal(18, 2)), CAST(N'2026-01-01' AS Date), CAST(N'2026-12-31' AS Date), 1)
INSERT [dbo].[KhuyenMai] ([MaKM], [Code], [MoTa], [DieuKien], [SoDiem], [PhanTramGiam], [GiamToiDa], [NgayBD], [NgayKT], [TrangThai]) VALUES (30, N'UUDAI50', N'Đổi 800 điểm lấy mã giảm 50% (Tối đa 100k) cho đơn từ 400K', CAST(400000.0 AS Decimal(18, 1)), 800, 50, CAST(100000.00 AS Decimal(18, 2)), CAST(N'2026-01-01' AS Date), CAST(N'2026-12-31' AS Date), 1)
INSERT [dbo].[KhuyenMai] ([MaKM], [Code], [MoTa], [DieuKien], [SoDiem], [PhanTramGiam], [GiamToiDa], [NgayBD], [NgayKT], [TrangThai]) VALUES (40, N'UUDAI100', N'Đổi 1000 điểm lấy mã giảm 100K', CAST(1000.0 AS Decimal(18, 1)), 1000, 100, CAST(100000.00 AS Decimal(18, 2)), CAST(N'2026-01-01' AS Date), CAST(N'2026-12-31' AS Date), 1)
INSERT [dbo].[KhuyenMai] ([MaKM], [Code], [MoTa], [DieuKien], [SoDiem], [PhanTramGiam], [GiamToiDa], [NgayBD], [NgayKT], [TrangThai]) VALUES (50, N'MAX2000', N'Đổi 2000 điểm lấy mã giảm 30% tối đa 250K, áp dụng cho đơn có giá trị tối thiểu 500K.', CAST(500000.0 AS Decimal(18, 1)), 2000, 30, CAST(250000.00 AS Decimal(18, 2)), CAST(N'2026-05-01' AS Date), CAST(N'2026-12-31' AS Date), 1)
SET IDENTITY_INSERT [dbo].[KhuyenMai] OFF
GO

INSERT [dbo].[LoaiGhe] ([MaLG], [TenLG], [PhuThu]) VALUES (N'DOI', N'Ghế đôi Sweetbox', CAST(50000.00 AS Decimal(18, 2)))
INSERT [dbo].[LoaiGhe] ([MaLG], [TenLG], [PhuThu]) VALUES (N'THUONG', N'Ghế tiêu chuẩn', CAST(0.00 AS Decimal(18, 2)))
INSERT [dbo].[LoaiGhe] ([MaLG], [TenLG], [PhuThu]) VALUES (N'VIP', N'Ghế VIP đơn', CAST(20000.00 AS Decimal(18, 2)))
GO

SET IDENTITY_INSERT [dbo].[NguoiDung] ON 
INSERT [dbo].[NguoiDung] ([MaND], [MaVT], [MaCN], [Email], [MatKhau], [Hinh], [HoTen], [NgaySinh], [Phai], [SDT], [CCCD], [DiaChi], [ChucVu], [DiemHoiVien], [TrangThai]) VALUES (1096, N'admin', NULL, N'admin@cinemavn.vn', N'$2a$12$bO5VLznxRUDDLIvl33GU1eJAIXJH0zNBJ8aVmRqekVGv/hruWdtUu', NULL, N'Lê Văn Đạt', CAST(N'2003-11-22' AS Date), 1, N'0869347040', N'060203002450', N'180 Cao Lỗ, Phường 4, Quận 8', N'Quản trị viên', 9999, 1)
INSERT [dbo].[NguoiDung] ([MaND], [MaVT], [MaCN], [Email], [MatKhau], [Hinh], [HoTen], [NgaySinh], [Phai], [SDT], [CCCD], [DiaChi], [ChucVu], [DiemHoiVien], [TrangThai]) VALUES (1104, N'manage', N'CNQ1', N'thao.manage@cinemavn.vn', N'$2a$12$bO5VLznxRUDDLIvl33GU1eJAIXJH0zNBJ8aVmRqekVGv/hruWdtUu', NULL, N'Trần Thị Thanh Thảo', CAST(N'1990-08-15' AS Date), 0, N'0912345678', N'079090123456', N'Lê Lợi, Quận 1, TP.HCM', N'Quản lý rạp', 0, 1)
INSERT [dbo].[NguoiDung] ([MaND], [MaVT], [MaCN], [Email], [MatKhau], [Hinh], [HoTen], [NgaySinh], [Phai], [SDT], [CCCD], [DiaChi], [ChucVu], [DiemHoiVien], [TrangThai]) VALUES (1112, N'manage', N'CNQ8', N'hau.manage@cinemavn.vn', N'$2a$11$GEB1sS.5s6wv4mYGjYFctuqnFwBOk5nNksjs6ljefeK7tPFMekPUG', NULL, N'Nguyễn Trung Hậu', CAST(N'2003-03-05' AS Date), 1, N'0901112233', N'079092001122', N'Tạ Quang Bửu, Quận 8', N'Quản lý rạp', 0, 1)
INSERT [dbo].[NguoiDung] ([MaND], [MaVT], [MaCN], [Email], [MatKhau], [Hinh], [HoTen], [NgaySinh], [Phai], [SDT], [CCCD], [DiaChi], [ChucVu], [DiemHoiVien], [TrangThai]) VALUES (1120, N'manage', N'CNQ7', N'minh.manage@cinemavn.vn', N'$2a$12$bO5VLznxRUDDLIvl33GU1eJAIXJH0zNBJ8aVmRqekVGv/hruWdtUu', NULL, N'Đặng Hoàng Minh', CAST(N'1988-06-18' AS Date), 1, N'0944556677', N'079088005544', N'Nguyễn Thị Thập, Quận 7', N'Quản lý rạp', 0, 1)
INSERT [dbo].[NguoiDung] ([MaND], [MaVT], [MaCN], [Email], [MatKhau], [Hinh], [HoTen], [NgaySinh], [Phai], [SDT], [CCCD], [DiaChi], [ChucVu], [DiemHoiVien], [TrangThai]) VALUES (1128, N'staff', N'CNQ8', N'tram.staff@cinemavn.vn', N'$2a$12$bO5VLznxRUDDLIvl33GU1eJAIXJH0zNBJ8aVmRqekVGv/hruWdtUu', NULL, N'Võ Thị Bích Trâm', CAST(N'2005-09-12' AS Date), 0, N'0385554433', N'079205001234', N'Phạm Hùng, Quận 8', N'Nhân viên soát vé', 0, 1)
INSERT [dbo].[NguoiDung] ([MaND], [MaVT], [MaCN], [Email], [MatKhau], [Hinh], [HoTen], [NgaySinh], [Phai], [SDT], [CCCD], [DiaChi], [ChucVu], [DiemHoiVien], [TrangThai]) VALUES (1136, N'staff', N'CNQ8', N'sang.staff@cinemavn.vn', N'$2a$12$bO5VLznxRUDDLIvl33GU1eJAIXJH0zNBJ8aVmRqekVGv/hruWdtUu', NULL, N'Nguyễn Thanh Sang', CAST(N'2004-01-10' AS Date), 1, N'0988777666', N'079204123456', N'Tạ Quang Bửu, Quận 8', N'Nhân viên bán vé', 0, 1)
INSERT [dbo].[NguoiDung] ([MaND], [MaVT], [MaCN], [Email], [MatKhau], [Hinh], [HoTen], [NgaySinh], [Phai], [SDT], [CCCD], [DiaChi], [ChucVu], [DiemHoiVien], [TrangThai]) VALUES (1144, N'staff', N'CNQ1', N'thanh.staff@cinemavn.vn', N'$2a$12$bO5VLznxRUDDLIvl33GU1eJAIXJH0zNBJ8aVmRqekVGv/hruWdtUu', NULL, N'Lý Gia Thành', CAST(N'2004-11-20' AS Date), 1, N'0933445566', N'079204008899', N'Đinh Tiên Hoàng, Quận 1', N'Nhân viên bán vé', 0, 1)
INSERT [dbo].[NguoiDung] ([MaND], [MaVT], [MaCN], [Email], [MatKhau], [Hinh], [HoTen], [NgaySinh], [Phai], [SDT], [CCCD], [DiaChi], [ChucVu], [DiemHoiVien], [TrangThai]) VALUES (1152, N'staff', N'CNQ1', N'tu.staff@cinemavn.vn', N'$2a$12$bO5VLznxRUDDLIvl33GU1eJAIXJH0zNBJ8aVmRqekVGv/hruWdtUu', NULL, N'Huỳnh Minh Tú', CAST(N'2005-02-28' AS Date), 1, N'0971239876', N'079205007766', N'Nguyễn Trãi, Quận 1', N'Nhân viên soát vé', 0, 1)
INSERT [dbo].[NguoiDung] ([MaND], [MaVT], [MaCN], [Email], [MatKhau], [Hinh], [HoTen], [NgaySinh], [Phai], [SDT], [CCCD], [DiaChi], [ChucVu], [DiemHoiVien], [TrangThai]) VALUES (1160, N'staff', N'CNQ7', N'lan.staff@cinemavn.vn', N'$2a$12$bO5VLznxRUDDLIvl33GU1eJAIXJH0zNBJ8aVmRqekVGv/hruWdtUu', NULL, N'Nguyễn Huỳnh Tuyết Lan', CAST(N'2005-03-22' AS Date), 0, N'0977666555', N'079205987654', N'Huỳnh Tấn Phát, Quận 7', N'Nhân viên soát vé', 0, 1)
INSERT [dbo].[NguoiDung] ([MaND], [MaVT], [MaCN], [Email], [MatKhau], [Hinh], [HoTen], [NgaySinh], [Phai], [SDT], [CCCD], [DiaChi], [ChucVu], [DiemHoiVien], [TrangThai]) VALUES (1168, N'staff', N'CNQ7', N'ha.staff@cinemavn.vn', N'$2a$12$bO5VLznxRUDDLIvl33GU1eJAIXJH0zNBJ8aVmRqekVGv/hruWdtUu', NULL, N'Lê Thị Thu Hà', CAST(N'2004-04-04' AS Date), 0, N'0366998877', N'079204001155', N'Nguyễn Văn Linh, Quận 7', N'Nhân viên bán vé', 0, 1)
INSERT [dbo].[NguoiDung] ([MaND], [MaVT], [MaCN], [Email], [MatKhau], [Hinh], [HoTen], [NgaySinh], [Phai], [SDT], [CCCD], [DiaChi], [ChucVu], [DiemHoiVien], [TrangThai]) VALUES (1176, N'customer', NULL, N'gia.hoang@gmail.com', N'$2a$12$bO5VLznxRUDDLIvl33GU1eJAIXJH0zNBJ8aVmRqekVGv/hruWdtUu', NULL, N'Phan Hoàng Gia', CAST(N'1995-11-30' AS Date), 1, N'0388634456', NULL, N'Khu biệt thự Phú Mỹ Hưng, Quận 7', NULL, 235, 1)
INSERT [dbo].[NguoiDung] ([MaND], [MaVT], [MaCN], [Email], [MatKhau], [Hinh], [HoTen], [NgaySinh], [Phai], [SDT], [CCCD], [DiaChi], [ChucVu], [DiemHoiVien], [TrangThai]) VALUES (1184, N'customer', NULL, N'quy.trung@gmail.com', N'$2a$12$bO5VLznxRUDDLIvl33GU1eJAIXJH0zNBJ8aVmRqekVGv/hruWdtUu', NULL, N'Nguyễn Ngọc Trung Quý', CAST(N'2004-12-12' AS Date), 1, N'0388123456', NULL, N'Phạm Hùng, Quận 8', NULL, 152, 1)
INSERT [dbo].[NguoiDung] ([MaND], [MaVT], [MaCN], [Email], [MatKhau], [Hinh], [HoTen], [NgaySinh], [Phai], [SDT], [CCCD], [DiaChi], [ChucVu], [DiemHoiVien], [TrangThai]) VALUES (1192, N'customer', NULL, N'thinh.phuc@gmail.com', N'$2a$12$bO5VLznxRUDDLIvl33GU1eJAIXJH0zNBJ8aVmRqekVGv/hruWdtUu', NULL, N'Nguyễn Hồ Phúc Thịnh', CAST(N'2003-06-05' AS Date), 1, N'0355444333', NULL, N'Dương Bá Trạc, Quận 8', NULL, 0, 0)
INSERT [dbo].[NguoiDung] ([MaND], [MaVT], [MaCN], [Email], [MatKhau], [Hinh], [HoTen], [NgaySinh], [Phai], [SDT], [CCCD], [DiaChi], [ChucVu], [DiemHoiVien], [TrangThai]) VALUES (1224, N'customer', NULL, N'levandat10a14@gmail.com', N'$2a$11$PSbPQN1N4vT1UgvtZKUFpuzq3XA2PbDUegPuHid33Ibb2zM8AZXXG', NULL, N'Lê Văn Đạt', CAST(N'2003-11-22' AS Date), 1, N'0832542882', NULL, NULL, N'Khách hàng', 610, 1)
INSERT [dbo].[NguoiDung] ([MaND], [MaVT], [MaCN], [Email], [MatKhau], [Hinh], [HoTen], [NgaySinh], [Phai], [SDT], [CCCD], [DiaChi], [ChucVu], [DiemHoiVien], [TrangThai]) VALUES (1232, N'customer', NULL, N'vandatle57@gmail.com', N'$2a$11$QVL7yp5lQfJXNF9dlroyfuKmCxTEfzYcIsFN27xk33wNSKxp3FoJS', NULL, N'Lê Văn Đạt', CAST(N'2003-11-22' AS Date), 1, N'0984327632', NULL, N'C2/26 Miếu Ngũ Hành', NULL, 1000, 1)
SET IDENTITY_INSERT [dbo].[NguoiDung] OFF
GO

SET IDENTITY_INSERT [dbo].[Phim] ON 
INSERT [dbo].[Phim] ([MaPhim], [TenPhim], [MoTa], [ThoiLuong], [NgayChieu], [DaoDien], [Poster], [Trailer], [DoTuoi], [TrangThai]) VALUES (10000, N'Avengers: Secret Wars', N'Sau sự kiện đa vũ trụ sụp đổ, các siêu anh hùng từ nhiều dòng thời gian khác nhau buộc phải hợp tác để chống lại một thế lực có khả năng viết lại thực tại. Trận chiến cuối cùng sẽ quyết định số phận của toàn bộ vũ trụ Marvel.', 150, CAST(N'2026-05-01' AS Date), N'Marvel Studios', N'cinemavn-poster-10000.png', N'https://www.youtube.com/watch?v=jAdMRTJOvNI', 13, 1)
INSERT [dbo].[Phim] ([MaPhim], [TenPhim], [MoTa], [ThoiLuong], [NgayChieu], [DaoDien], [Poster], [Trailer], [DoTuoi], [TrangThai]) VALUES (10002, N'Detective Conan: Black Iron Submarine', N'Conan cùng tổ chức FBI đối đầu với tổ chức Áo Đen trong một âm mưu liên quan đến hệ thống nhận diện khuôn mặt toàn cầu. Một bí mật động trời có thể làm thay đổi mọi thứ.', 110, CAST(N'2026-04-20' AS Date), N'Gosho Aoyama', N'cinemavn-poster-10002.png', N'v=a9YyrgWpcMA', 13, 1)
INSERT [dbo].[Phim] ([MaPhim], [TenPhim], [MoTa], [ThoiLuong], [NgayChieu], [DaoDien], [Poster], [Trailer], [DoTuoi], [TrangThai]) VALUES (10004, N'Quỷ Nhập Tràng', N'Một gia đình chuyển đến vùng quê sinh sống và phát hiện ra những hiện tượng kỳ lạ liên quan đến một lời nguyền cổ xưa. Những cái chết bí ẩn bắt đầu xảy ra khi họ cố gắng khám phá sự thật.', 95, CAST(N'2026-04-10' AS Date), N'Nguyễn Thành Nam', N'cinemavn-poster-10004.png', N'youtube.com/watch?v=VCI9XTxlQxk', 18, 1)
INSERT [dbo].[Phim] ([MaPhim], [TenPhim], [MoTa], [ThoiLuong], [NgayChieu], [DaoDien], [Poster], [Trailer], [DoTuoi], [TrangThai]) VALUES (10006, N'Doraemon: Nobita và Bản Giao Hưởng Địa Cầu', N'Nobita và nhóm bạn bước vào một cuộc phiêu lưu âm nhạc kỳ diệu để cứu lấy hành tinh khỏi sự biến mất của âm thanh. Một hành trình đầy cảm xúc và ý nghĩa.', 100, CAST(N'2026-05-01' AS Date), N'Fujiko F Fujio', N'cinemavn-poster-10006.png', N'https://www.youtube.com/watch?v=Yug8gbDd5EQ', 0, 1)
INSERT [dbo].[Phim] ([MaPhim], [TenPhim], [MoTa], [ThoiLuong], [NgayChieu], [DaoDien], [Poster], [Trailer], [DoTuoi], [TrangThai]) VALUES (10008, N'Fast & Furious 11', N'Dom và gia đình phải đối đầu với kẻ thù nguy hiểm nhất từ trước đến nay – một người có mối liên hệ sâu sắc với quá khứ của họ. Những pha hành động nghẹt thở và tốc độ không giới hạn.', 140, CAST(N'2026-03-01' AS Date), N'Justin Lin', N'cinemavn-poster-10008.png', N'https://www.youtube.com/watch?v=hbQ7Tm25iQ4', 16, 2)
INSERT [dbo].[Phim] ([MaPhim], [TenPhim], [MoTa], [ThoiLuong], [NgayChieu], [DaoDien], [Poster], [Trailer], [DoTuoi], [TrangThai]) VALUES (10010, N'Nhà Bà Nữ 2', N'Câu chuyện tiếp nối xoay quanh những mâu thuẫn gia đình, tình yêu và sự trưởng thành của các thế hệ trong một gia đình Việt hiện đại.', 115, CAST(N'2026-03-20' AS Date), N'Trấn Thành', N'cinemavn-poster-10010.png', N'https://www.youtube.com/watch?v=IkaP0KJWTsQ', 13, 1)
INSERT [dbo].[Phim] ([MaPhim], [TenPhim], [MoTa], [ThoiLuong], [NgayChieu], [DaoDien], [Poster], [Trailer], [DoTuoi], [TrangThai]) VALUES (10012, N'Interstellar 2', N'Nhân loại tiếp tục hành trình khám phá vũ trụ để tìm kiếm sự sống mới. Những bí ẩn về thời gian và không gian được hé lộ.', 160, CAST(N'2026-08-15' AS Date), N'Christopher Nolan', N'cinemavn-poster-10012.png', N'https://www.youtube.com/watch?v=9wAjZkR_Qp4', 13, 0)
INSERT [dbo].[Phim] ([MaPhim], [TenPhim], [MoTa], [ThoiLuong], [NgayChieu], [DaoDien], [Poster], [Trailer], [DoTuoi], [TrangThai]) VALUES (10014, N'Superman', N'Hành trình của Clark Kent nhằm hàn gắn di sản Krypton với sự nuôi dưỡng của con người. Anh là hiện thân của sự thật, công lý và một ngày mai tươi sáng hơn.', 145, CAST(N'2025-07-11' AS Date), N'James Gunn', N'cinemavn-poster-639133341546262663.jpg', N'https://www.youtube.com/watch?v=Ox8ZLF6cGM0', 13, 1)
INSERT [dbo].[Phim] ([MaPhim], [TenPhim], [MoTa], [ThoiLuong], [NgayChieu], [DaoDien], [Poster], [Trailer], [DoTuoi], [TrangThai]) VALUES (10016, N'Spider-Man: Beyond the Spider-Verse', N'Phần kết của bộ ba phim hoạt hình Spider-Verse, nơi Miles Morales phải đối mặt với thực thể Spot và tìm cách cứu lấy đa vũ trụ cũng như gia đình mình.', 130, CAST(N'2026-03-20' AS Date), N'Joaquim Dos Santos', N'cinemavn-poster-639133341489716282.png', N'https://www.youtube.com/watch?v=qclHAbmDOJI', 13, 1)
INSERT [dbo].[Phim] ([MaPhim], [TenPhim], [MoTa], [ThoiLuong], [NgayChieu], [DaoDien], [Poster], [Trailer], [DoTuoi], [TrangThai]) VALUES (10018, N'Mufasa: The Lion King', N'Khám phá quá khứ của vị vua vĩ đại Mufasa, từ một chú sư tử mồ côi trở thành người đứng đầu Pride Rock cùng với sự đồng hành của người anh em Scar.', 118, CAST(N'2026-05-20' AS Date), N'Barry Jenkins', N'cinemavn-poster-639133341423807293.jpg', N'https://www.youtube.com/watch?v=o17MF9vnabg', 0, 1)
INSERT [dbo].[Phim] ([MaPhim], [TenPhim], [MoTa], [ThoiLuong], [NgayChieu], [DaoDien], [Poster], [Trailer], [DoTuoi], [TrangThai]) VALUES (10020, N'ANH HÙNG', N'Câu chuyện phim theo chân Hùng (Thái Hòa) - người cha đơn thân kiêm tài xế taxi và đồng nghiệp hãng xe là Tuấn (Võ Tấn Phát) bị cuốn vào một phi vụ lừa đảo từ thiện tiền tỉ trong khi sinh mạng cô con gái nhỏ của anh đang nằm gọn trong tay tử thần.', 122, CAST(N'2026-09-25' AS Date), N'Võ Thạch Thảo', N'cinemavn-poster-639128268236370469.jpg', N'https://www.youtube.com/watch?v=44c-RyL0YP4', 13, 0)
INSERT [dbo].[Phim] ([MaPhim], [TenPhim], [MoTa], [ThoiLuong], [NgayChieu], [DaoDien], [Poster], [Trailer], [DoTuoi], [TrangThai]) VALUES (10022, N'The Batman Part II', N'Bruce Wayne tiếp tục lấn sâu vào thế giới ngầm của Gotham khi những kẻ thù mới xuất hiện từ bóng tối sau trận lụt kinh hoàng.', 165, CAST(N'2026-10-02' AS Date), N'Matt Reeves', N'cinemavn-poster-639133343461022515.png', N'https://www.youtube.com/watch?v=VKuww279Mwk', 16, 0)
INSERT [dbo].[Phim] ([MaPhim], [TenPhim], [MoTa], [ThoiLuong], [NgayChieu], [DaoDien], [Poster], [Trailer], [DoTuoi], [TrangThai]) VALUES (10024, N'Avatar 3: Fire and Ash', N'Jake Sully và Neytiri gặp gỡ một bộ tộc Na''vi mới sống trong vùng núi lửa - Ash People, những người có cái nhìn khắc nghiệt hơn về Pandora.', 180, CAST(N'2026-12-19' AS Date), N'James Cameron', N'cinemavn-poster-639133342677986739.jpg', N'https://www.youtube.com/watch?v=nb_fFj_0rq8', 13, 0)
SET IDENTITY_INSERT [dbo].[Phim] OFF
GO

SET IDENTITY_INSERT [dbo].[PhongChieu] ON 
INSERT [dbo].[PhongChieu] ([MaPC], [TenPC], [MaCN], [SucChua]) VALUES (100, N'Phòng 1', N'CNQ1', 30)
INSERT [dbo].[PhongChieu] ([MaPC], [TenPC], [MaCN], [SucChua]) VALUES (101, N'Phòng 2', N'CNQ1', 30)
INSERT [dbo].[PhongChieu] ([MaPC], [TenPC], [MaCN], [SucChua]) VALUES (102, N'Phòng 1', N'CNQ7', 20)
INSERT [dbo].[PhongChieu] ([MaPC], [TenPC], [MaCN], [SucChua]) VALUES (103, N'Phòng 2', N'CNQ7', 30)
INSERT [dbo].[PhongChieu] ([MaPC], [TenPC], [MaCN], [SucChua]) VALUES (104, N'Phòng 1', N'CNQ8', 20)
INSERT [dbo].[PhongChieu] ([MaPC], [TenPC], [MaCN], [SucChua]) VALUES (105, N'Phòng 2', N'CNQ8', 30)
SET IDENTITY_INSERT [dbo].[PhongChieu] OFF
GO

SET IDENTITY_INSERT [dbo].[SuatChieu] ON 
INSERT [dbo].[SuatChieu] ([MaSC], [MaPhim], [MaPC], [MaDD], [NgayChieu], [GioBD], [GioKT], [GiaGoc], [TrangThai]) VALUES (100, 10002, 100, N'2D', CAST(N'2026-05-01' AS Date), CAST(N'08:00:00' AS Time), CAST(N'10:00:00' AS Time), CAST(80000.00 AS Decimal(18, 2)), 2)
INSERT [dbo].[SuatChieu] ([MaSC], [MaPhim], [MaPC], [MaDD], [NgayChieu], [GioBD], [GioKT], [GiaGoc], [TrangThai]) VALUES (101, 10002, 100, N'2D', CAST(N'2026-05-01' AS Date), CAST(N'10:30:00' AS Time), CAST(N'12:30:00' AS Time), CAST(80000.00 AS Decimal(18, 2)), 2)
INSERT [dbo].[SuatChieu] ([MaSC], [MaPhim], [MaPC], [MaDD], [NgayChieu], [GioBD], [GioKT], [GiaGoc], [TrangThai]) VALUES (102, 10000, 104, N'IMAX', CAST(N'2026-05-01' AS Date), CAST(N'19:00:00' AS Time), CAST(N'21:30:00' AS Time), CAST(80000.00 AS Decimal(18, 2)), 2)
INSERT [dbo].[SuatChieu] ([MaSC], [MaPhim], [MaPC], [MaDD], [NgayChieu], [GioBD], [GioKT], [GiaGoc], [TrangThai]) VALUES (103, 10012, 102, N'3D', CAST(N'2026-05-02' AS Date), CAST(N'14:00:00' AS Time), CAST(N'16:40:00' AS Time), CAST(80000.00 AS Decimal(18, 2)), 2)
INSERT [dbo].[SuatChieu] ([MaSC], [MaPhim], [MaPC], [MaDD], [NgayChieu], [GioBD], [GioKT], [GiaGoc], [TrangThai]) VALUES (104, 10000, 100, N'IMAX', CAST(N'2026-05-05' AS Date), CAST(N'13:00:00' AS Time), CAST(N'15:30:00' AS Time), CAST(80000.00 AS Decimal(18, 2)), 1)
INSERT [dbo].[SuatChieu] ([MaSC], [MaPhim], [MaPC], [MaDD], [NgayChieu], [GioBD], [GioKT], [GiaGoc], [TrangThai]) VALUES (105, 10000, 101, N'2D', CAST(N'2026-05-05' AS Date), CAST(N'20:00:00' AS Time), CAST(N'22:30:00' AS Time), CAST(80000.00 AS Decimal(18, 2)), 1)
INSERT [dbo].[SuatChieu] ([MaSC], [MaPhim], [MaPC], [MaDD], [NgayChieu], [GioBD], [GioKT], [GiaGoc], [TrangThai]) VALUES (106, 10000, 100, N'IMAX', CAST(N'2026-05-05' AS Date), CAST(N'09:00:00' AS Time), CAST(N'11:30:00' AS Time), CAST(80000.00 AS Decimal(18, 2)), 1)
INSERT [dbo].[SuatChieu] ([MaSC], [MaPhim], [MaPC], [MaDD], [NgayChieu], [GioBD], [GioKT], [GiaGoc], [TrangThai]) VALUES (107, 10002, 102, N'2D', CAST(N'2026-05-05' AS Date), CAST(N'08:30:00' AS Time), CAST(N'10:20:00' AS Time), CAST(80000.00 AS Decimal(18, 2)), 1)
INSERT [dbo].[SuatChieu] ([MaSC], [MaPhim], [MaPC], [MaDD], [NgayChieu], [GioBD], [GioKT], [GiaGoc], [TrangThai]) VALUES (108, 10002, 102, N'2D', CAST(N'2026-05-05' AS Date), CAST(N'14:00:00' AS Time), CAST(N'15:50:00' AS Time), CAST(80000.00 AS Decimal(18, 2)), 1)
INSERT [dbo].[SuatChieu] ([MaSC], [MaPhim], [MaPC], [MaDD], [NgayChieu], [GioBD], [GioKT], [GiaGoc], [TrangThai]) VALUES (109, 10012, 103, N'2D', CAST(N'2026-06-06' AS Date), CAST(N'08:00:00' AS Time), CAST(N'09:40:00' AS Time), CAST(80000.00 AS Decimal(18, 2)), 0)
INSERT [dbo].[SuatChieu] ([MaSC], [MaPhim], [MaPC], [MaDD], [NgayChieu], [GioBD], [GioKT], [GiaGoc], [TrangThai]) VALUES (110, 10012, 103, N'3D', CAST(N'2026-06-02' AS Date), CAST(N'10:30:00' AS Time), CAST(N'12:10:00' AS Time), CAST(80000.00 AS Decimal(18, 2)), 0)
INSERT [dbo].[SuatChieu] ([MaSC], [MaPhim], [MaPC], [MaDD], [NgayChieu], [GioBD], [GioKT], [GiaGoc], [TrangThai]) VALUES (111, 10012, 104, N'IMAX', CAST(N'2026-06-01' AS Date), CAST(N'19:00:00' AS Time), CAST(N'21:40:00' AS Time), CAST(80000.00 AS Decimal(18, 2)), 0)
INSERT [dbo].[SuatChieu] ([MaSC], [MaPhim], [MaPC], [MaDD], [NgayChieu], [GioBD], [GioKT], [GiaGoc], [TrangThai]) VALUES (112, 10020, 100, N'IMAX', CAST(N'2026-10-04' AS Date), CAST(N'19:00:00' AS Time), CAST(N'21:30:00' AS Time), CAST(80000.00 AS Decimal(18, 2)), 0)
INSERT [dbo].[SuatChieu] ([MaSC], [MaPhim], [MaPC], [MaDD], [NgayChieu], [GioBD], [GioKT], [GiaGoc], [TrangThai]) VALUES (113, 10020, 100, N'IMAX', CAST(N'2026-10-06' AS Date), CAST(N'18:30:00' AS Time), CAST(N'21:00:00' AS Time), CAST(80000.00 AS Decimal(18, 2)), 0)
INSERT [dbo].[SuatChieu] ([MaSC], [MaPhim], [MaPC], [MaDD], [NgayChieu], [GioBD], [GioKT], [GiaGoc], [TrangThai]) VALUES (114, 10020, 101, N'2D', CAST(N'2026-10-09' AS Date), CAST(N'20:30:00' AS Time), CAST(N'23:00:00' AS Time), CAST(80000.00 AS Decimal(18, 2)), 0)
INSERT [dbo].[SuatChieu] ([MaSC], [MaPhim], [MaPC], [MaDD], [NgayChieu], [GioBD], [GioKT], [GiaGoc], [TrangThai]) VALUES (115, 10020, 100, N'IMAX', CAST(N'2026-10-10' AS Date), CAST(N'14:00:00' AS Time), CAST(N'16:30:00' AS Time), CAST(80000.00 AS Decimal(18, 2)), 0)
INSERT [dbo].[SuatChieu] ([MaSC], [MaPhim], [MaPC], [MaDD], [NgayChieu], [GioBD], [GioKT], [GiaGoc], [TrangThai]) VALUES (116, 10020, 101, N'2D', CAST(N'2026-10-15' AS Date), CAST(N'19:00:00' AS Time), CAST(N'21:30:00' AS Time), CAST(80000.00 AS Decimal(18, 2)), 0)
INSERT [dbo].[SuatChieu] ([MaSC], [MaPhim], [MaPC], [MaDD], [NgayChieu], [GioBD], [GioKT], [GiaGoc], [TrangThai]) VALUES (117, 10002, 102, N'2D', CAST(N'2026-05-05' AS Date), CAST(N'15:00:00' AS Time), CAST(N'16:50:00' AS Time), CAST(80000.00 AS Decimal(18, 2)), 1)
INSERT [dbo].[SuatChieu] ([MaSC], [MaPhim], [MaPC], [MaDD], [NgayChieu], [GioBD], [GioKT], [GiaGoc], [TrangThai]) VALUES (118, 10002, 102, N'2D', CAST(N'2026-05-08' AS Date), CAST(N'17:30:00' AS Time), CAST(N'19:20:00' AS Time), CAST(80000.00 AS Decimal(18, 2)), 1)
INSERT [dbo].[SuatChieu] ([MaSC], [MaPhim], [MaPC], [MaDD], [NgayChieu], [GioBD], [GioKT], [GiaGoc], [TrangThai]) VALUES (119, 10002, 102, N'2D', CAST(N'2026-05-10' AS Date), CAST(N'10:00:00' AS Time), CAST(N'11:50:00' AS Time), CAST(80000.00 AS Decimal(18, 2)), 1)
INSERT [dbo].[SuatChieu] ([MaSC], [MaPhim], [MaPC], [MaDD], [NgayChieu], [GioBD], [GioKT], [GiaGoc], [TrangThai]) VALUES (120, 10002, 102, N'2D', CAST(N'2026-05-16' AS Date), CAST(N'14:00:00' AS Time), CAST(N'15:50:00' AS Time), CAST(80000.00 AS Decimal(18, 2)), 1)
INSERT [dbo].[SuatChieu] ([MaSC], [MaPhim], [MaPC], [MaDD], [NgayChieu], [GioBD], [GioKT], [GiaGoc], [TrangThai]) VALUES (121, 10006, 103, N'2D', CAST(N'2026-05-03' AS Date), CAST(N'08:00:00' AS Time), CAST(N'09:40:00' AS Time), CAST(80000.00 AS Decimal(18, 2)), 1)
INSERT [dbo].[SuatChieu] ([MaSC], [MaPhim], [MaPC], [MaDD], [NgayChieu], [GioBD], [GioKT], [GiaGoc], [TrangThai]) VALUES (122, 10006, 103, N'3D', CAST(N'2026-05-09' AS Date), CAST(N'09:30:00' AS Time), CAST(N'11:10:00' AS Time), CAST(80000.00 AS Decimal(18, 2)), 1)
INSERT [dbo].[SuatChieu] ([MaSC], [MaPhim], [MaPC], [MaDD], [NgayChieu], [GioBD], [GioKT], [GiaGoc], [TrangThai]) VALUES (123, 10006, 103, N'2D', CAST(N'2026-05-10' AS Date), CAST(N'08:30:00' AS Time), CAST(N'10:10:00' AS Time), CAST(80000.00 AS Decimal(18, 2)), 1)
INSERT [dbo].[SuatChieu] ([MaSC], [MaPhim], [MaPC], [MaDD], [NgayChieu], [GioBD], [GioKT], [GiaGoc], [TrangThai]) VALUES (124, 10006, 103, N'3D', CAST(N'2026-05-17' AS Date), CAST(N'10:00:00' AS Time), CAST(N'11:40:00' AS Time), CAST(80000.00 AS Decimal(18, 2)), 1)
INSERT [dbo].[SuatChieu] ([MaSC], [MaPhim], [MaPC], [MaDD], [NgayChieu], [GioBD], [GioKT], [GiaGoc], [TrangThai]) VALUES (125, 10010, 101, N'2D', CAST(N'2026-05-04' AS Date), CAST(N'16:00:00' AS Time), CAST(N'17:55:00' AS Time), CAST(80000.00 AS Decimal(18, 2)), 1)
INSERT [dbo].[SuatChieu] ([MaSC], [MaPhim], [MaPC], [MaDD], [NgayChieu], [GioBD], [GioKT], [GiaGoc], [TrangThai]) VALUES (126, 10010, 101, N'2D', CAST(N'2026-05-07' AS Date), CAST(N'19:30:00' AS Time), CAST(N'21:25:00' AS Time), CAST(80000.00 AS Decimal(18, 2)), 1)
INSERT [dbo].[SuatChieu] ([MaSC], [MaPhim], [MaPC], [MaDD], [NgayChieu], [GioBD], [GioKT], [GiaGoc], [TrangThai]) VALUES (127, 10010, 101, N'2D', CAST(N'2026-05-11' AS Date), CAST(N'20:00:00' AS Time), CAST(N'21:55:00' AS Time), CAST(80000.00 AS Decimal(18, 2)), 1)
INSERT [dbo].[SuatChieu] ([MaSC], [MaPhim], [MaPC], [MaDD], [NgayChieu], [GioBD], [GioKT], [GiaGoc], [TrangThai]) VALUES (128, 10010, 101, N'2D', CAST(N'2026-05-14' AS Date), CAST(N'18:00:00' AS Time), CAST(N'19:55:00' AS Time), CAST(80000.00 AS Decimal(18, 2)), 1)
SET IDENTITY_INSERT [dbo].[SuatChieu] OFF
GO

SET IDENTITY_INSERT [dbo].[TheLoai] ON 
INSERT [dbo].[TheLoai] ([MaTL], [TenTL]) VALUES (3, N'Hành động')
INSERT [dbo].[TheLoai] ([MaTL], [TenTL]) VALUES (6, N'Tình cảm')
INSERT [dbo].[TheLoai] ([MaTL], [TenTL]) VALUES (9, N'Kinh dị')
INSERT [dbo].[TheLoai] ([MaTL], [TenTL]) VALUES (12, N'Hài hước')
INSERT [dbo].[TheLoai] ([MaTL], [TenTL]) VALUES (15, N'Hoạt hình')
INSERT [dbo].[TheLoai] ([MaTL], [TenTL]) VALUES (18, N'Viễn tưởng')
INSERT [dbo].[TheLoai] ([MaTL], [TenTL]) VALUES (21, N'Gia đình')
INSERT [dbo].[TheLoai] ([MaTL], [TenTL]) VALUES (24, N'Tâm lý')
INSERT [dbo].[TheLoai] ([MaTL], [TenTL]) VALUES (27, N'Phiêu lưu')
INSERT [dbo].[TheLoai] ([MaTL], [TenTL]) VALUES (30, N'Lãng mạn')
INSERT [dbo].[TheLoai] ([MaTL], [TenTL]) VALUES (33, N'Tài liệu')
INSERT [dbo].[TheLoai] ([MaTL], [TenTL]) VALUES (36, N'Cổ trang')
INSERT [dbo].[TheLoai] ([MaTL], [TenTL]) VALUES (39, N'Chiến tranh')
INSERT [dbo].[TheLoai] ([MaTL], [TenTL]) VALUES (42, N'Nhạc kịch')
SET IDENTITY_INSERT [dbo].[TheLoai] OFF
GO

INSERT [dbo].[VaiTro] ([MaVT], [TenVT]) VALUES (N'admin', N'Quản trị')
INSERT [dbo].[VaiTro] ([MaVT], [TenVT]) VALUES (N'customer', N'Khách hàng')
INSERT [dbo].[VaiTro] ([MaVT], [TenVT]) VALUES (N'manage', N'Quản lý')
INSERT [dbo].[VaiTro] ([MaVT], [TenVT]) VALUES (N'staff', N'Nhân viên')
GO

SET IDENTITY_INSERT [dbo].[Ve] ON 
INSERT [dbo].[Ve] ([MaVe], [MaHD], [MaSC], [MaGhe], [Gia], [TrangThai], [ThoiGianGiu], [SessionKey]) VALUES (900, 100, 100, 3, CAST(80000.00 AS Decimal(18, 2)), 2, NULL, N'')
INSERT [dbo].[Ve] ([MaVe], [MaHD], [MaSC], [MaGhe], [Gia], [TrangThai], [ThoiGianGiu], [SessionKey]) VALUES (901, 100, 100, 4, CAST(80000.00 AS Decimal(18, 2)), 2, NULL, N'')
INSERT [dbo].[Ve] ([MaVe], [MaHD], [MaSC], [MaGhe], [Gia], [TrangThai], [ThoiGianGiu], [SessionKey]) VALUES (902, 101, 102, 161, CAST(160000.00 AS Decimal(18, 2)), 2, NULL, N'')
INSERT [dbo].[Ve] ([MaVe], [MaHD], [MaSC], [MaGhe], [Gia], [TrangThai], [ThoiGianGiu], [SessionKey]) VALUES (960, 110, 121, 132, CAST(100000.00 AS Decimal(18, 2)), 1, NULL, N'')
INSERT [dbo].[Ve] ([MaVe], [MaHD], [MaSC], [MaGhe], [Gia], [TrangThai], [ThoiGianGiu], [SessionKey]) VALUES (961, 111, 123, 151, CAST(130000.00 AS Decimal(18, 2)), 1, NULL, N'')
INSERT [dbo].[Ve] ([MaVe], [MaHD], [MaSC], [MaGhe], [Gia], [TrangThai], [ThoiGianGiu], [SessionKey]) VALUES (962, 111, 123, 152, CAST(130000.00 AS Decimal(18, 2)), 1, NULL, N'')
INSERT [dbo].[Ve] ([MaVe], [MaHD], [MaSC], [MaGhe], [Gia], [TrangThai], [ThoiGianGiu], [SessionKey]) VALUES (963, 112, 117, 119, CAST(100000.00 AS Decimal(18, 2)), 1, NULL, N'')
INSERT [dbo].[Ve] ([MaVe], [MaHD], [MaSC], [MaGhe], [Gia], [TrangThai], [ThoiGianGiu], [SessionKey]) VALUES (964, 112, 117, 123, CAST(130000.00 AS Decimal(18, 2)), 1, NULL, N'')
INSERT [dbo].[Ve] ([MaVe], [MaHD], [MaSC], [MaGhe], [Gia], [TrangThai], [ThoiGianGiu], [SessionKey]) VALUES (974, 120, 124, 152, CAST(160000.00 AS Decimal(18, 2)), 1, CAST(N'2026-05-03T12:37:07.830' AS DateTime), N'1c5f8b41-eba4-4940-a21c-3ea139c4dc3a')
INSERT [dbo].[Ve] ([MaVe], [MaHD], [MaSC], [MaGhe], [Gia], [TrangThai], [ThoiGianGiu], [SessionKey]) VALUES (975, 120, 124, 148, CAST(130000.00 AS Decimal(18, 2)), 1, CAST(N'2026-05-03T12:37:07.857' AS DateTime), N'1c5f8b41-eba4-4940-a21c-3ea139c4dc3a')
INSERT [dbo].[Ve] ([MaVe], [MaHD], [MaSC], [MaGhe], [Gia], [TrangThai], [ThoiGianGiu], [SessionKey]) VALUES (976, 121, 105, 106, CAST(130000.00 AS Decimal(18, 2)), 1, CAST(N'2026-05-03T12:53:42.413' AS DateTime), NULL)
INSERT [dbo].[Ve] ([MaVe], [MaHD], [MaSC], [MaGhe], [Gia], [TrangThai], [ThoiGianGiu], [SessionKey]) VALUES (977, 121, 105, 102, CAST(100000.00 AS Decimal(18, 2)), 1, CAST(N'2026-05-03T12:53:42.437' AS DateTime), NULL)
INSERT [dbo].[Ve] ([MaVe], [MaHD], [MaSC], [MaGhe], [Gia], [TrangThai], [ThoiGianGiu], [SessionKey]) VALUES (982, 124, 105, 80, CAST(130000.00 AS Decimal(18, 2)), 1, CAST(N'2026-05-05T22:17:02.233' AS DateTime), N'cbab9039-de90-4da0-84be-07d4aece6bf1')
SET IDENTITY_INSERT [dbo].[Ve] OFF
GO

