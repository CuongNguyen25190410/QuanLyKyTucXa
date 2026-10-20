using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Migrations
{
    /// <inheritdoc />
    public partial class ThemBangHoaDon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HoaDon");
        }
    }
}
