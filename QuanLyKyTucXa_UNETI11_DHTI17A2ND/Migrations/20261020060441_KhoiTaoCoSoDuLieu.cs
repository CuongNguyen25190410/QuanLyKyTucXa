using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyKyTucXa_UNETI11_DHTI17A2ND.Migrations
{
    /// <inheritdoc />
    public partial class KhoiTaoCoSoDuLieu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LoaiPhong",
                columns: table => new
                {
                    MaLoaiPhong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenLoaiPhong = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoNguoiToiDa = table.Column<int>(type: "int", nullable: false),
                    DonGiaThang = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoaiPhong", x => x.MaLoaiPhong);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoan",
                columns: table => new
                {
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDangNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoan", x => x.MaTaiKhoan);
                });

            migrationBuilder.CreateTable(
                name: "PhongKTX",
                columns: table => new
                {
                    MaPhong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenPhong = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaLoaiPhong = table.Column<int>(type: "int", nullable: false),
                    ToaNha = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Tang = table.Column<int>(type: "int", nullable: false),
                    GioiTinhPhong = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhongKTX", x => x.MaPhong);
                    table.ForeignKey(
                        name: "FK_PhongKTX_LoaiPhong_MaLoaiPhong",
                        column: x => x.MaLoaiPhong,
                        principalTable: "LoaiPhong",
                        principalColumn: "MaLoaiPhong",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SinhVien",
                columns: table => new
                {
                    MaSinhVien = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GioiTinh = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Lop = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Khoa = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SinhVien", x => x.MaSinhVien);
                    table.ForeignKey(
                        name: "FK_SinhVien_TaiKhoan_MaTaiKhoan",
                        column: x => x.MaTaiKhoan,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DangKyKTX",
                columns: table => new
                {
                    MaDangKy = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaSinhVien = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NgayDangKy = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayKetThucDuKien = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NguyenVongLoaiPhong = table.Column<int>(type: "int", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DangKyKTX", x => x.MaDangKy);
                    table.ForeignKey(
                        name: "FK_DangKyKTX_LoaiPhong_NguyenVongLoaiPhong",
                        column: x => x.NguyenVongLoaiPhong,
                        principalTable: "LoaiPhong",
                        principalColumn: "MaLoaiPhong",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DangKyKTX_SinhVien_MaSinhVien",
                        column: x => x.MaSinhVien,
                        principalTable: "SinhVien",
                        principalColumn: "MaSinhVien",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HoaDon",
                columns: table => new
                {
                    MaHoaDon = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDangKy = table.Column<int>(type: "int", nullable: false),
                    MaPhong = table.Column<int>(type: "int", nullable: false),
                    Thang = table.Column<int>(type: "int", nullable: false),
                    Nam = table.Column<int>(type: "int", nullable: false),
                    SoTien = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HanThanhToan = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OrderCode = table.Column<long>(type: "bigint", nullable: true),
                    PaymentLinkId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    QrCode = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    CheckoutUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    LinkHetHan = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NgayThanhToan = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PhuongThuc = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    MaGiaoDich = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDon", x => x.MaHoaDon);
                    table.ForeignKey(
                        name: "FK_HoaDon_DangKyKTX_MaDangKy",
                        column: x => x.MaDangKy,
                        principalTable: "DangKyKTX",
                        principalColumn: "MaDangKy",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HoaDon_PhongKTX_MaPhong",
                        column: x => x.MaPhong,
                        principalTable: "PhongKTX",
                        principalColumn: "MaPhong",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhanPhong",
                columns: table => new
                {
                    MaPhanPhong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDangKy = table.Column<int>(type: "int", nullable: false),
                    MaPhong = table.Column<int>(type: "int", nullable: false),
                    NgayVao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayRa = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhanPhong", x => x.MaPhanPhong);
                    table.ForeignKey(
                        name: "FK_PhanPhong_DangKyKTX_MaDangKy",
                        column: x => x.MaDangKy,
                        principalTable: "DangKyKTX",
                        principalColumn: "MaDangKy",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhanPhong_PhongKTX_MaPhong",
                        column: x => x.MaPhong,
                        principalTable: "PhongKTX",
                        principalColumn: "MaPhong",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DangKyKTX_MaSinhVien",
                table: "DangKyKTX",
                column: "MaSinhVien",
                unique: true,
                filter: "[TrangThai] <> N'Đã hủy' AND [TrangThai] <> N'Đã trả phòng'");

            migrationBuilder.CreateIndex(
                name: "IX_DangKyKTX_NguyenVongLoaiPhong",
                table: "DangKyKTX",
                column: "NguyenVongLoaiPhong");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_MaDangKy_Thang_Nam",
                table: "HoaDon",
                columns: new[] { "MaDangKy", "Thang", "Nam" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_MaPhong",
                table: "HoaDon",
                column: "MaPhong");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_OrderCode",
                table: "HoaDon",
                column: "OrderCode",
                unique: true,
                filter: "[OrderCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LoaiPhong_TenLoaiPhong",
                table: "LoaiPhong",
                column: "TenLoaiPhong",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhanPhong_MaDangKy",
                table: "PhanPhong",
                column: "MaDangKy",
                unique: true,
                filter: "[TrangThai] = N'Đang ở'");

            migrationBuilder.CreateIndex(
                name: "IX_PhanPhong_MaPhong",
                table: "PhanPhong",
                column: "MaPhong");

            migrationBuilder.CreateIndex(
                name: "IX_PhongKTX_MaLoaiPhong",
                table: "PhongKTX",
                column: "MaLoaiPhong");

            migrationBuilder.CreateIndex(
                name: "IX_PhongKTX_TenPhong",
                table: "PhongKTX",
                column: "TenPhong",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SinhVien_MaTaiKhoan",
                table: "SinhVien",
                column: "MaTaiKhoan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoan_TenDangNhap",
                table: "TaiKhoan",
                column: "TenDangNhap",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HoaDon");

            migrationBuilder.DropTable(
                name: "PhanPhong");

            migrationBuilder.DropTable(
                name: "DangKyKTX");

            migrationBuilder.DropTable(
                name: "PhongKTX");

            migrationBuilder.DropTable(
                name: "SinhVien");

            migrationBuilder.DropTable(
                name: "LoaiPhong");

            migrationBuilder.DropTable(
                name: "TaiKhoan");
        }
    }
}
