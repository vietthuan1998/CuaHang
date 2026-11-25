using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuaHang.Api.Migrations
{
    /// <inheritdoc />
    public partial class ThemRangBuocCheck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_SanPhams_DonGia",
                table: "SanPhams",
                sql: "[DonGia] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SanPhams_TonKho",
                table: "SanPhams",
                sql: "[TonKho] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DonHangs_SoLuong",
                table: "DonHangs",
                sql: "[SoLuong] > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_SanPhams_DonGia",
                table: "SanPhams");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SanPhams_TonKho",
                table: "SanPhams");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DonHangs_SoLuong",
                table: "DonHangs");
        }
    }
}
