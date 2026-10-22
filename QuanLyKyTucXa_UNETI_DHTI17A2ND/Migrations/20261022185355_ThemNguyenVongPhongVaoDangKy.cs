using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyKyTucXa_UNETI_DHTI17A2ND.Migrations
{
    /// <inheritdoc />
    public partial class ThemNguyenVongPhongVaoDangKy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NguyenVongPhong",
                table: "DangKyKTX",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DangKyKTX_NguyenVongPhong",
                table: "DangKyKTX",
                column: "NguyenVongPhong");

            migrationBuilder.AddForeignKey(
                name: "FK_DangKyKTX_PhongKTX_NguyenVongPhong",
                table: "DangKyKTX",
                column: "NguyenVongPhong",
                principalTable: "PhongKTX",
                principalColumn: "MaPhong",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DangKyKTX_PhongKTX_NguyenVongPhong",
                table: "DangKyKTX");

            migrationBuilder.DropIndex(
                name: "IX_DangKyKTX_NguyenVongPhong",
                table: "DangKyKTX");

            migrationBuilder.DropColumn(
                name: "NguyenVongPhong",
                table: "DangKyKTX");
        }
    }
}
