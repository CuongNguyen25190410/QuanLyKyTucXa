using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Migrations
{
    /// <inheritdoc />
    public partial class ThemBangDonHuyPhong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DonHuyPhong",
                columns: table => new
                {
                    MaDon = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDangKy = table.Column<int>(type: "int", nullable: false),
                    NgayGui = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayRaDuKien = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LyDo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NgayXuLy = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MaTaiKhoanXuLy = table.Column<int>(type: "int", nullable: true),
                    GhiChuAdmin = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonHuyPhong", x => x.MaDon);
                    table.ForeignKey(
                        name: "FK_DonHuyPhong_DangKyKTX_MaDangKy",
                        column: x => x.MaDangKy,
                        principalTable: "DangKyKTX",
                        principalColumn: "MaDangKy",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DonHuyPhong_TaiKhoan_MaTaiKhoanXuLy",
                        column: x => x.MaTaiKhoanXuLy,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DonHuyPhong_MaDangKy",
                table: "DonHuyPhong",
                column: "MaDangKy",
                unique: true,
                filter: "[TrangThai] = N'Chờ duyệt'");

            migrationBuilder.CreateIndex(
                name: "IX_DonHuyPhong_MaTaiKhoanXuLy",
                table: "DonHuyPhong",
                column: "MaTaiKhoanXuLy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DonHuyPhong");
        }
    }
}
