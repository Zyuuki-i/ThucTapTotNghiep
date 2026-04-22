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
    SoLuongTon INT DEFAULT 0,
	CONSTRAINT CK_DV_Gia CHECK (Gia >= 0)
);

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
    TrangThai INT DEFAULT 0
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

SET IDENTITY_INSERT [dbo].[Banner] ON 
INSERT [dbo].[Banner] ([MaBN], [TieuDe], [HinhAnh], [DuongDan], [TrangThai]) VALUES (1, N'Tưng Bừng Khai Trương', N'cinemavn-grand-opening.png', N'https://www.facebook.com/tuyendungcgv.vn/', 1)
INSERT [dbo].[Banner] ([MaBN], [TieuDe], [HinhAnh], [DuongDan], [TrangThai]) VALUES (2, N'Movie Time', N'cinemavn-movie-time.jpg', NULL, 1)
INSERT [dbo].[Banner] ([MaBN], [TieuDe], [HinhAnh], [DuongDan], [TrangThai]) VALUES (3, N'Movie Night', N'cinemavn-movie-night.png', NULL, 1)
INSERT [dbo].[Banner] ([MaBN], [TieuDe], [HinhAnh], [DuongDan], [TrangThai]) VALUES (4, N'Quà tặng mỗi ngày', N'cinemavn-popcorn-gift.jpg', NULL, 0)
SET IDENTITY_INSERT [dbo].[Banner] OFF
GO

SET IDENTITY_INSERT [dbo].[TheLoai] ON
INSERT [dbo].[TheLoai] ([MaTL], [TenTL]) VALUES (3, N'Hành động')
INSERT [dbo].[TheLoai] ([MaTL], [TenTL]) VALUES (6, N'Tình cảm')
INSERT [dbo].[TheLoai] ([MaTL], [TenTL]) VALUES (9, N'Kinh dị')
INSERT [dbo].[TheLoai] ([MaTL], [TenTL]) VALUES (12, N'Hài')
INSERT [dbo].[TheLoai] ([MaTL], [TenTL]) VALUES (15, N'Hoạt hình')
INSERT [dbo].[TheLoai] ([MaTL], [TenTL]) VALUES (18, N'Viễn tưởng')
SET IDENTITY_INSERT [dbo].[TheLoai] OFF
GO

INSERT [dbo].[VaiTro] ([MaVT], [TenVT]) VALUES (N'admin', N'Quản trị')
INSERT [dbo].[VaiTro] ([MaVT], [TenVT]) VALUES (N'customer', N'Khách hàng')
INSERT [dbo].[VaiTro] ([MaVT], [TenVT]) VALUES (N'manage', N'Quản lý')
INSERT [dbo].[VaiTro] ([MaVT], [TenVT]) VALUES (N'staff', N'Nhân viên')
GO

INSERT [dbo].[DinhDang] ([MaDD], [TenDD], [PhuThu]) VALUES (N'2D', N'2D tiêu chuẩn', CAST(0.00 AS Decimal(18, 2)))
INSERT [dbo].[DinhDang] ([MaDD], [TenDD], [PhuThu]) VALUES (N'3D', N'3D kỹ thuật số', CAST(30000.00 AS Decimal(18, 2)))
INSERT [dbo].[DinhDang] ([MaDD], [TenDD], [PhuThu]) VALUES (N'IMAX', N'IMAX siêu thực', CAST(60000.00 AS Decimal(18, 2)))
GO

INSERT [dbo].[LoaiGhe] ([MaLG], [TenLG], [PhuThu]) VALUES (N'DOI', N'Ghế đôi Sweetbox', CAST(50000.00 AS Decimal(18, 2)))
INSERT [dbo].[LoaiGhe] ([MaLG], [TenLG], [PhuThu]) VALUES (N'THUONG', N'Ghế tiêu chuẩn', CAST(0.00 AS Decimal(18, 2)))
INSERT [dbo].[LoaiGhe] ([MaLG], [TenLG], [PhuThu]) VALUES (N'VIP', N'Ghế VIP đơn', CAST(20000.00 AS Decimal(18, 2)))
GO

INSERT [dbo].[ChiNhanh] ([MaCN], [TenCN], [DiaChi], [SDT]) VALUES (N'CNQ1', N'CinemaVN Bến Thành', N'1 Lê Lợi, Phường Bến Thành, Quận 1, TP.HCM', N'0988333883')
INSERT [dbo].[ChiNhanh] ([MaCN], [TenCN], [DiaChi], [SDT]) VALUES (N'CNQ7', N'CinemaVN Phú Mỹ Hưng', N'123 Đại lộ Nguyễn Văn Linh, Quận 7, TP.HCM', N'0988333882')
INSERT [dbo].[ChiNhanh] ([MaCN], [TenCN], [DiaChi], [SDT]) VALUES (N'CNQ8', N'CinemaVN Cao Lỗ', N'222 Cao Lỗ, Phường 4, Quận 8, TP.HCM', N'0988333881')
GO

SET IDENTITY_INSERT [dbo].[NguoiDung] ON
INSERT [dbo].[NguoiDung] ([MaND], [MaVT], [MaCN], [Email], [MatKhau], [Hinh], [HoTen], [NgaySinh], [Phai], [SDT], [CCCD], [DiaChi], [ChucVu], [DiemHoiVien], [TrangThai]) VALUES (1096, N'admin', NULL, N'admin@cinemavn.vn', N'$2a$12$bO5VLznxRUDDLIvl33GU1eJAIXJH0zNBJ8aVmRqekVGv/hruWdtUu', NULL, N'Lê Văn Đạt', CAST(N'2003-11-22' AS Date), 1, N'0869347040', N'060203002450', N'180 Cao Lỗ, Phường 4, Quận 8', N'Quản trị viên', 9999, 1)
INSERT [dbo].[NguoiDung] ([MaND], [MaVT], [MaCN], [Email], [MatKhau], [Hinh], [HoTen], [NgaySinh], [Phai], [SDT], [CCCD], [DiaChi], [ChucVu], [DiemHoiVien], [TrangThai]) VALUES (1104, N'manage', N'CNQ1', N'thao.manage@cinemavn.vn', N'$2a$12$bO5VLznxRUDDLIvl33GU1eJAIXJH0zNBJ8aVmRqekVGv/hruWdtUu', NULL, N'Trần Thị Thanh Thảo', CAST(N'1990-08-15' AS Date), 0, N'0912345678', N'079090123456', N'Lê Lợi, Quận 1, TP.HCM', N'Quản lý rạp', 0, 1)
INSERT [dbo].[NguoiDung] ([MaND], [MaVT], [MaCN], [Email], [MatKhau], [Hinh], [HoTen], [NgaySinh], [Phai], [SDT], [CCCD], [DiaChi], [ChucVu], [DiemHoiVien], [TrangThai]) VALUES (1112, N'manage', N'CNQ8', N'hau.manage@cinemavn.vn', N'$2a$12$bO5VLznxRUDDLIvl33GU1eJAIXJH0zNBJ8aVmRqekVGv/hruWdtUu', NULL, N'Nguyễn Trung Hậu', CAST(N'2003-03-05' AS Date), 1, N'0901112233', N'079092001122', N'Tạ Quang Bửu, Quận 8', N'Quản lý rạp', 0, 1)
INSERT [dbo].[NguoiDung] ([MaND], [MaVT], [MaCN], [Email], [MatKhau], [Hinh], [HoTen], [NgaySinh], [Phai], [SDT], [CCCD], [DiaChi], [ChucVu], [DiemHoiVien], [TrangThai]) VALUES (1120, N'manage', N'CNQ7', N'minh.manage@cinemavn.vn', N'$2a$12$bO5VLznxRUDDLIvl33GU1eJAIXJH0zNBJ8aVmRqekVGv/hruWdtUu', NULL, N'Đặng Hoàng Minh', CAST(N'1988-06-18' AS Date), 1, N'0944556677', N'079088005544', N'Nguyễn Thị Thập, Quận 7', N'Quản lý rạp', 0, 1)
INSERT [dbo].[NguoiDung] ([MaND], [MaVT], [MaCN], [Email], [MatKhau], [Hinh], [HoTen], [NgaySinh], [Phai], [SDT], [CCCD], [DiaChi], [ChucVu], [DiemHoiVien], [TrangThai]) VALUES (1128, N'staff', N'CNQ8', N'tram.staff@cinemavn.vn', N'$2a$12$bO5VLznxRUDDLIvl33GU1eJAIXJH0zNBJ8aVmRqekVGv/hruWdtUu', NULL, N'Võ Thị Bích Trâm', CAST(N'2005-09-12' AS Date), 0, N'0385554433', N'079205001234', N'Phạm Hùng, Quận 8', N'Nhân viên soát vé', 0, 1)
INSERT [dbo].[NguoiDung] ([MaND], [MaVT], [MaCN], [Email], [MatKhau], [Hinh], [HoTen], [NgaySinh], [Phai], [SDT], [CCCD], [DiaChi], [ChucVu], [DiemHoiVien], [TrangThai]) VALUES (1136, N'staff', N'CNQ8', N'sang.staff@cinemavn.vn', N'$2a$12$bO5VLznxRUDDLIvl33GU1eJAIXJH0zNBJ8aVmRqekVGv/hruWdtUu', NULL, N'Nguyễn Thanh Sang', CAST(N'2004-01-10' AS Date), 1, N'0988777666', N'079204123456', N'Tạ Quang Bửu, Quận 8', N'Nhân viên bán vé', 0, 1)
INSERT [dbo].[NguoiDung] ([MaND], [MaVT], [MaCN], [Email], [MatKhau], [Hinh], [HoTen], [NgaySinh], [Phai], [SDT], [CCCD], [DiaChi], [ChucVu], [DiemHoiVien], [TrangThai]) VALUES (1144, N'staff', N'CNQ1', N'thanh.staff@cinemavn.vn', N'$2a$12$bO5VLznxRUDDLIvl33GU1eJAIXJH0zNBJ8aVmRqekVGv/hruWdtUu', NULL, N'Lý Gia Thành', CAST(N'2004-11-20' AS Date), 1, N'0933445566', N'079204008899', N'Đinh Tiên Hoàng, Quận 1', N'Nhân viên bán vé', 0, 1)
INSERT [dbo].[NguoiDung] ([MaND], [MaVT], [MaCN], [Email], [MatKhau], [Hinh], [HoTen], [NgaySinh], [Phai], [SDT], [CCCD], [DiaChi], [ChucVu], [DiemHoiVien], [TrangThai]) VALUES (1152, N'staff', N'CNQ1', N'tu.staff@cinemavn.vn', N'$2a$12$bO5VLznxRUDDLIvl33GU1eJAIXJH0zNBJ8aVmRqekVGv/hruWdtUu', NULL, N'Huỳnh Minh Tú', CAST(N'2005-02-28' AS Date), 1, N'0971239876', N'079205007766', N'Nguyễn Trãi, Quận 1', N'Nhân viên soát vé', 0, 1)
INSERT [dbo].[NguoiDung] ([MaND], [MaVT], [MaCN], [Email], [MatKhau], [Hinh], [HoTen], [NgaySinh], [Phai], [SDT], [CCCD], [DiaChi], [ChucVu], [DiemHoiVien], [TrangThai]) VALUES (1160, N'staff', N'CNQ7', N'lan.staff@cinemavn.vn', N'$2a$12$bO5VLznxRUDDLIvl33GU1eJAIXJH0zNBJ8aVmRqekVGv/hruWdtUu', NULL, N'Nguyễn Huỳnh Tuyết Lan', CAST(N'2005-03-22' AS Date), 0, N'0977666555', N'079205987654', N'Huỳnh Tấn Phát, Quận 7', N'Nhân viên soát vé', 0, 1)
INSERT [dbo].[NguoiDung] ([MaND], [MaVT], [MaCN], [Email], [MatKhau], [Hinh], [HoTen], [NgaySinh], [Phai], [SDT], [CCCD], [DiaChi], [ChucVu], [DiemHoiVien], [TrangThai]) VALUES (1168, N'staff', N'CNQ7', N'ha.staff@cinemavn.vn', N'$2a$12$bO5VLznxRUDDLIvl33GU1eJAIXJH0zNBJ8aVmRqekVGv/hruWdtUu', NULL, N'Lê Thị Thu Hà', CAST(N'2004-04-04' AS Date), 0, N'0366998877', N'079204001155', N'Nguyễn Văn Linh, Quận 7', N'Nhân viên bán vé', 0, 1)
INSERT [dbo].[NguoiDung] ([MaND], [MaVT], [MaCN], [Email], [MatKhau], [Hinh], [HoTen], [NgaySinh], [Phai], [SDT], [CCCD], [DiaChi], [ChucVu], [DiemHoiVien], [TrangThai]) VALUES (1176, N'customer', NULL, N'gia.hoang@gmail.com', N'$2a$12$bO5VLznxRUDDLIvl33GU1eJAIXJH0zNBJ8aVmRqekVGv/hruWdtUu', NULL, N'Phan Hoàng Gia', CAST(N'1995-11-30' AS Date), 1, N'0388634456', NULL, N'Khu biệt thự Phú Mỹ Hưng, Quận 7', NULL, 1500, 1)
INSERT [dbo].[NguoiDung] ([MaND], [MaVT], [MaCN], [Email], [MatKhau], [Hinh], [HoTen], [NgaySinh], [Phai], [SDT], [CCCD], [DiaChi], [ChucVu], [DiemHoiVien], [TrangThai]) VALUES (1184, N'customer', NULL, N'quy.trung@gmail.com', N'$2a$12$bO5VLznxRUDDLIvl33GU1eJAIXJH0zNBJ8aVmRqekVGv/hruWdtUu', NULL, N'Nguyễn Ngọc Trung Quý', CAST(N'2004-12-12' AS Date), 1, N'0388123456', NULL, N'Phạm Hùng, Quận 8', NULL, 450, 1)
INSERT [dbo].[NguoiDung] ([MaND], [MaVT], [MaCN], [Email], [MatKhau], [Hinh], [HoTen], [NgaySinh], [Phai], [SDT], [CCCD], [DiaChi], [ChucVu], [DiemHoiVien], [TrangThai]) VALUES (1192, N'customer', NULL, N'thinh.phuc@gmail.com', N'$2a$12$bO5VLznxRUDDLIvl33GU1eJAIXJH0zNBJ8aVmRqekVGv/hruWdtUu', NULL, N'Nguyễn Hồ Phúc Thịnh', CAST(N'2003-06-05' AS Date), 1, N'0355444333', NULL, N'Dương Bá Trạc, Quận 8', NULL, 0, 0)
INSERT [dbo].[NguoiDung] ([MaND], [MaVT], [MaCN], [Email], [MatKhau], [Hinh], [HoTen], [NgaySinh], [Phai], [SDT], [CCCD], [DiaChi], [ChucVu], [DiemHoiVien], [TrangThai]) VALUES (1224, N'customer', NULL, N'levandat10a14@gmail.com', N'$2a$11$PSbPQN1N4vT1UgvtZKUFpuzq3XA2PbDUegPuHid33Ibb2zM8AZXXG', NULL, N'Lê Văn Đạt', CAST(N'2003-11-22' AS Date), 1, N'0832542882', NULL, N'', N'Khách hàng', 0, 1)
SET IDENTITY_INSERT [dbo].[NguoiDung] OFF
GO

SET IDENTITY_INSERT [dbo].[Phim] ON 
INSERT [dbo].[Phim] ([MaPhim], [TenPhim], [MoTa], [ThoiLuong], [NgayChieu], [DaoDien], [Poster], [Trailer], [DoTuoi], [TrangThai]) VALUES (10000, N'Avengers: Secret Wars', N'Sau sự kiện đa vũ trụ sụp đổ, các siêu anh hùng từ nhiều dòng thời gian khác nhau buộc phải hợp tác để chống lại một thế lực có khả năng viết lại thực tại. Trận chiến cuối cùng sẽ quyết định số phận của toàn bộ vũ trụ Marvel.', 150, CAST(N'2026-05-01' AS Date), N'Marvel Studios', N'cinemavn-poster-10000.png', N'https://www.youtube.com/watch?v=jAdMRTJOvNI', 13, 0)
INSERT [dbo].[Phim] ([MaPhim], [TenPhim], [MoTa], [ThoiLuong], [NgayChieu], [DaoDien], [Poster], [Trailer], [DoTuoi], [TrangThai]) VALUES (10002, N'Detective Conan: Black Iron Submarine', N'Conan cùng tổ chức FBI đối đầu với tổ chức Áo Đen trong một âm mưu liên quan đến hệ thống nhận diện khuôn mặt toàn cầu. Một bí mật động trời có thể làm thay đổi mọi thứ.', 110, CAST(N'2025-12-20' AS Date), N'Gosho Aoyama', N'cinemavn-poster-10002.png', N'v=a9YyrgWpcMA', 13, 1)
INSERT [dbo].[Phim] ([MaPhim], [TenPhim], [MoTa], [ThoiLuong], [NgayChieu], [DaoDien], [Poster], [Trailer], [DoTuoi], [TrangThai]) VALUES (10004, N'Quỷ Nhập Tràng', N'Một gia đình chuyển đến vùng quê sinh sống và phát hiện ra những hiện tượng kỳ lạ liên quan đến một lời nguyền cổ xưa. Những cái chết bí ẩn bắt đầu xảy ra khi họ cố gắng khám phá sự thật.', 95, CAST(N'2025-10-10' AS Date), N'Nguyễn Văn X', N'cinemavn-poster-10004.png', N'youtube.com/watch?v=VCI9XTxlQxk', 18, 1)
INSERT [dbo].[Phim] ([MaPhim], [TenPhim], [MoTa], [ThoiLuong], [NgayChieu], [DaoDien], [Poster], [Trailer], [DoTuoi], [TrangThai]) VALUES (10006, N'Doraemon: Nobita và Bản Giao Hưởng Địa Cầu', N'Nobita và nhóm bạn bước vào một cuộc phiêu lưu âm nhạc kỳ diệu để cứu lấy hành tinh khỏi sự biến mất của âm thanh. Một hành trình đầy cảm xúc và ý nghĩa.', 100, CAST(N'2025-06-01' AS Date), N'Fujiko F Fujio', N'cinemavn-poster-10006.png', N'https://www.youtube.com/watch?v=Yug8gbDd5EQ', 0, 1)
INSERT [dbo].[Phim] ([MaPhim], [TenPhim], [MoTa], [ThoiLuong], [NgayChieu], [DaoDien], [Poster], [Trailer], [DoTuoi], [TrangThai]) VALUES (10008, N'Fast & Furious 11', N'Dom và gia đình phải đối đầu với kẻ thù nguy hiểm nhất từ trước đến nay – một người có mối liên hệ sâu sắc với quá khứ của họ. Những pha hành động nghẹt thở và tốc độ không giới hạn.', 140, CAST(N'2025-03-01' AS Date), N'Justin Lin', N'cinemavn-poster-10008.png', N'https://www.youtube.com/watch?v=hbQ7Tm25iQ4', 16, 2)
INSERT [dbo].[Phim] ([MaPhim], [TenPhim], [MoTa], [ThoiLuong], [NgayChieu], [DaoDien], [Poster], [Trailer], [DoTuoi], [TrangThai]) VALUES (10010, N'Nhà Bà Nữ 2', N'Câu chuyện tiếp nối xoay quanh những mâu thuẫn gia đình, tình yêu và sự trưởng thành của các thế hệ trong một gia đình Việt hiện đại.', 115, CAST(N'2025-08-20' AS Date), N'Trấn Thành', N'cinemavn-poster-10010.png', N'https://www.youtube.com/watch?v=IkaP0KJWTsQ', 13, 1)
INSERT [dbo].[Phim] ([MaPhim], [TenPhim], [MoTa], [ThoiLuong], [NgayChieu], [DaoDien], [Poster], [Trailer], [DoTuoi], [TrangThai]) VALUES (10012, N'Interstellar 2', N'Nhân loại tiếp tục hành trình khám phá vũ trụ để tìm kiếm sự sống mới. Những bí ẩn về thời gian và không gian được hé lộ.', 160, CAST(N'2026-01-15' AS Date), N'Christopher Nolan', N'cinemavn-poster-10012.png', N'https://www.youtube.com/watch?v=9wAjZkR_Qp4', 13, 0)
SET IDENTITY_INSERT [dbo].[Phim] OFF

INSERT [dbo].[ChiTietTheLoai] ([MaPhim], [MaTL]) VALUES (10000, 3)
INSERT [dbo].[ChiTietTheLoai] ([MaPhim], [MaTL]) VALUES (10000, 18)
INSERT [dbo].[ChiTietTheLoai] ([MaPhim], [MaTL]) VALUES (10002, 3)
INSERT [dbo].[ChiTietTheLoai] ([MaPhim], [MaTL]) VALUES (10004, 9)
INSERT [dbo].[ChiTietTheLoai] ([MaPhim], [MaTL]) VALUES (10006, 15)
INSERT [dbo].[ChiTietTheLoai] ([MaPhim], [MaTL]) VALUES (10008, 3)
INSERT [dbo].[ChiTietTheLoai] ([MaPhim], [MaTL]) VALUES (10010, 6)
INSERT [dbo].[ChiTietTheLoai] ([MaPhim], [MaTL]) VALUES (10010, 12)
INSERT [dbo].[ChiTietTheLoai] ([MaPhim], [MaTL]) VALUES (10012, 18)
GO

SET IDENTITY_INSERT [dbo].[DichVu] ON 
INSERT [dbo].[DichVu] ([MaDV], [TenDV], [Gia], [LoaiDV], [HinhAnh], [SoLuongTon]) VALUES (100, N'Bắp nhỏ', CAST(45000.00 AS Decimal(18, 2)), N'Bắp', NULL, 100)
INSERT [dbo].[DichVu] ([MaDV], [TenDV], [Gia], [LoaiDV], [HinhAnh], [SoLuongTon]) VALUES (111, N'Bắp lớn', CAST(65000.00 AS Decimal(18, 2)), N'Bắp', NULL, 100)
INSERT [dbo].[DichVu] ([MaDV], [TenDV], [Gia], [LoaiDV], [HinhAnh], [SoLuongTon]) VALUES (122, N'Nước ngọt', CAST(30000.00 AS Decimal(18, 2)), N'Nước', NULL, 200)
INSERT [dbo].[DichVu] ([MaDV], [TenDV], [Gia], [LoaiDV], [HinhAnh], [SoLuongTon]) VALUES (133, N'Combo 1 (Bắp nhỏ + Nước)', CAST(75000.00 AS Decimal(18, 2)), N'Combo', NULL, 50)
INSERT [dbo].[DichVu] ([MaDV], [TenDV], [Gia], [LoaiDV], [HinhAnh], [SoLuongTon]) VALUES (144, N'Combo 2 (Bắp lớn + Nước)', CAST(100000.00 AS Decimal(18, 2)), N'Combo', NULL, 70)
INSERT [dbo].[DichVu] ([MaDV], [TenDV], [Gia], [LoaiDV], [HinhAnh], [SoLuongTon]) VALUES (155, N'Combo đôi ta', CAST(150000.00 AS Decimal(18, 2)), N'Combo', NULL, 30)
SET IDENTITY_INSERT [dbo].[DichVu] OFF
GO

SET IDENTITY_INSERT [dbo].[KhuyenMai] ON 
INSERT [dbo].[KhuyenMai] ([MaKM], [Code], [MoTa], [DieuKien], [SoDiem], [PhanTramGiam], [GiamToiDa], [NgayBD], [NgayKT], [TrangThai]) VALUES (10, N'UUDAI5', N'Đổi 100 điểm lấy mã giảm 5% (Tối đa 20k) cho đơn từ 100K', CAST(100000.0 AS Decimal(18, 1)), 100, 5, CAST(20000.00 AS Decimal(18, 2)), CAST(N'2026-01-01' AS Date), CAST(N'2026-12-31' AS Date), 0)
INSERT [dbo].[KhuyenMai] ([MaKM], [Code], [MoTa], [DieuKien], [SoDiem], [PhanTramGiam], [GiamToiDa], [NgayBD], [NgayKT], [TrangThai]) VALUES (20, N'UUDAI20', N'Đổi 300 điểm lấy mã giảm 20% (Tối đa 50k) cho đơn từ 200K', CAST(200000.0 AS Decimal(18, 1)), 300, 20, CAST(50000.00 AS Decimal(18, 2)), CAST(N'2026-01-01' AS Date), CAST(N'2026-12-31' AS Date), 0)
INSERT [dbo].[KhuyenMai] ([MaKM], [Code], [MoTa], [DieuKien], [SoDiem], [PhanTramGiam], [GiamToiDa], [NgayBD], [NgayKT], [TrangThai]) VALUES (30, N'UUDAI50', N'Đổi 800 điểm lấy mã giảm 50% (Tối đa 100k) cho đơn từ 400K', CAST(400000.0 AS Decimal(18, 1)), 800, 50, CAST(100000.00 AS Decimal(18, 2)), CAST(N'2026-01-01' AS Date), CAST(N'2026-12-31' AS Date), 0)
INSERT [dbo].[KhuyenMai] ([MaKM], [Code], [MoTa], [DieuKien], [SoDiem], [PhanTramGiam], [GiamToiDa], [NgayBD], [NgayKT], [TrangThai]) VALUES (40, N'UUDAI100', N'Đổi 1000 điểm lấy mã giảm 100K', CAST(1000.0 AS Decimal(18, 1)), 1000, 100, CAST(100000.00 AS Decimal(18, 2)), CAST(N'2026-01-01' AS Date), CAST(N'2026-12-31' AS Date), 0)
SET IDENTITY_INSERT [dbo].[KhuyenMai] OFF
GO





